using KitapOneri.Models;
using Microsoft.EntityFrameworkCore;

namespace KitapOneri.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> books { get; set; }
        public DbSet<Rating> ratings { get; set; }
        public DbSet<Author> authors { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<UserBook> user_books { get; set; }
        
        public DbSet<BlogComment> BlogComments { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Book entity konfigürasyonu
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");
                entity.Property(b => b.name).HasColumnName("name");
                entity.Property(b => b.author).HasColumnName("author");
                entity.Property(b => b.book_img).HasColumnName("book_img");
                entity.Property(b => b.explanation).HasColumnName("explanation");
                entity.Property(b => b.isbn).HasColumnName("isbn");
                entity.HasKey(b => b.isbn);
            });

            // Rating entity konfigürasyonu
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("ratings");
                entity.Property(r => r.isbn).HasColumnName("isbn");
                entity.Property(r => r.average_rating).HasColumnName("average_rating");
                entity.HasKey(r => r.isbn);
                entity.HasOne(r => r.book)
                      .WithOne(b => b.rating)
                      .HasForeignKey<Rating>(r => r.isbn)
                      .HasPrincipalKey<Book>(b => b.isbn);
            });

            // UserBook entity konfigürasyonu (Composite Key ile)
            modelBuilder.Entity<UserBook>(entity =>
            {
                entity.ToTable("user_books");
                
                // Composite Primary Key (UserId + Isbn)
                entity.HasKey(ub => new { ub.UserId, ub.Isbn });

                // Sütun adlarını doğru eşleştiriyoruz
                entity.Property(ub => ub.UserId).HasColumnName("user_id");
                entity.Property(ub => ub.Isbn).HasColumnName("isbn");
                entity.Property(ub => ub.Wishlist).HasColumnName("wishlist");
                entity.Property(ub => ub.ReadBook).HasColumnName("readbook");  // 'read_book' -> 'readbook' olarak düzeltilmiştir.
                entity.Property(ub => ub.AddedDate).HasColumnName("added_date");
                entity.Property(ub => ub.FinishedDate).HasColumnName("finished_date");

                // İlişkiler
                entity.HasOne(ub => ub.User)
                      .WithMany(u => u.UserBooks)
                      .HasForeignKey(ub => ub.UserId);

                entity.HasOne(ub => ub.Book)
                      .WithMany(b => b.UserBooks)
                      .HasForeignKey(ub => ub.Isbn);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
