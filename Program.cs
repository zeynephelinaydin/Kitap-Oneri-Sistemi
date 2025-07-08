using KitapOneri.Data;
using KitapOneri.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL bağlantı cümlesi
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Entity Framework yapılandırması
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Servisler
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RecommendationService>();


// HttpContext erişimi (Layout.cshtml'de Session kullanabilmek için şart)
builder.Services.AddHttpContextAccessor();

// Session desteği
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// MVC ve CSRF koruması - ValidateAntiforgeryToken otomatik olarak eklenecek
builder.Services.AddControllersWithViews(options =>
{
    // Global CSRF koruması ayarı - Controller'da açıkça [IgnoreAntiforgeryToken] belirtilmedikçe
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Antiforgery cookie'yi özelleştirme - SameSite=None ayarı ile CORS destekli API çağrılarına izin verme
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

var app = builder.Build();

// >>> RecommendationService başlatma işlemi (modeli 1 kez eğitiyoruz) <<<
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var recommender = scope.ServiceProvider.GetRequiredService<RecommendationService>();
    var books = db.books.ToList();
    recommender.Initialize(books);  // Kitapları başlatıyoruz
}

// Hata yönetimi
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Session middleware aktif hâle getiriliyor
app.UseSession();

// Dinamik kitap yönlendirmesi
app.MapGet("/book/{isbn}", async (string isbn, ApplicationDbContext context) =>
{
    var book = await context.books.FirstOrDefaultAsync(b => b.isbn == isbn);

    if (book == null)
    {
        return Results.NotFound();
    }

    return Results.Redirect($"/Books/Details/{isbn}");
});

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();