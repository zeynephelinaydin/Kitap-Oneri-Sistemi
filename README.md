<img width="1456" alt="Ekran Resmi 2025-07-08 09 57 29" src="https://github.com/user-attachments/assets/37d33c77-ca76-4126-84d4-be9dea8baa9e" /># 📚 Kitap Öneri Sistemi

Merhaba! Bu proje, kullanıcıların kitap arayabileceği, öneri alabileceği ve detaylarına ulaşabileceği bir kitap öneri sistemidir. ASP.NET MVC yapısı kullanılarak geliştirilmiştir ve kullanıcı dostu bir arayüz ile zenginleştirilmiştir.

## ✨ Özellikler

- **🔍 Kitap Arama:** Kullanıcılar kitap ismine göre arama yapabilir.
- **📖 Detay Görüntüleme:** Kitabın yazar, yayın evi, ISBN ve açıklama gibi detaylarını görebilir.
- **💡 Kitap Önerisi:** Kullanıcılara ilgi alanlarına göre kitap önerisi sunar.
- **🔐 Giriş/Kayıt Sistemi:** Kullanıcılar sisteme kayıt olabilir ve giriş yapabilir.
- **⭐ Yorum ve Puanlama (Geliştirilebilir):** Kitaplara puan verilebilir, yorum yapılabilir (özellik eklenmeye açıktır).

## 🛠️ Kullanılan Teknolojiler

- **ASP.NET MVC 5:** Uygulamanın genel yapısı ve iş mantığı için.
- **Entity Framework:** Veritabanı işlemleri için.
- **SQL Server:** Verilerin saklanması için.
- **Bootstrap:** Modern ve responsive tasarım için.
- **LINQ:** Veri filtreleme ve sorgulama işlemleri için.

## 📁 Proje Yapısı

- `Models/`: Kitap, Yazar gibi temel veri modelleri.
- `Controllers/`: Sayfaların iş mantığını yöneten denetleyiciler.
- `Views/`: Razor syntax ile yazılmış HTML sayfaları.
- `wwwroot/`: CSS, JS, resimler gibi statik dosyalar.
- `App_Data/`: Veritabanı dosyası (Varsa).

## 🧭 Kullanım

- **Giriş Sayfası (`/login`)**: Kayıtlı kullanıcılar giriş yapabilir.
- **Kayıt Sayfası (`/register`)**: Yeni kullanıcılar sisteme kayıt olabilir.
- **Anasayfa (`/Books/Index`)**: Popüler veya önerilen kitaplar gösterilir.
- **Arama**: Anasayfa üzerinden kitap adına göre arama yapılabilir.
- **Detay Sayfası (`/Books/Details?isbn=...`)**: Seçilen kitabın detaylarına ulaşılır.
- **Yönlendirme**: Giriş yapılmadan detay sayfasına gidilirse otomatik login ekranına yönlendirilir.

## 🧠 Geliştirme Önerileri

- Kullanıcı profili oluşturma ve favorilere ekleme.
- Yorum ve değerlendirme sistemi.
- Kategorilere göre filtreleme.
- Makine öğrenmesiyle kitap öneri sistemi geliştirme.

## 🖼️ Ekran Görüntüleri

> Aşağıdaki görseller sistemin çeşitli sayfalarını göstermektedir:

<img width="1461" alt="Ekran Resmi 2025-07-08 09 57 25" src="https://github.com/user-attachments/assets/aafd48c5-248b-4105-b961-1372e2a8a00b" />
<img width="1470" alt="VBotO" src="https://github.com/user-attachments/assets/01977257-0df9-4a45-80ce-d7c145d5f2a4" />
<img width="1466" alt="Ekran Resmi 2025-07-08 09 57 05" src="https://github.com/user-attachments/assets/79e66d84-1a42-4746-80b2-bfd13f22cf76" />
<img width="1461" alt="Ekran Resmi 2025-07-08 10 05 04" src="https://github.com/user-attachments/assets/5825293a-27ef-4fc7-bfc7-23cbac6ede60" />
<img width="1456" alt="Ekran Resmi 2025-07-08 09 57 29" src="https://github.com/user-attachments/assets/ddb5d7a2-bb08-4473-a233-9da6a3491498" />
<img width="1465" alt="Ekran Resmi 2025-07-08 10 07 53" src="https://github.com/user-attachments/assets/f40a0b65-6c38-40a1-be2c-5deb78b34c0c" />
<img width="1467" alt="Ekran Resmi 2025-07-08 09 57 40" src="https://github.com/user-attachments/assets/9e349de2-2da8-4eb4-b78a-e98e2ccc40c9" />
<img width="1464" alt="Ekran Resmi 2025-07-08 10 08 11" src="https://github.com/user-attachments/assets/47916e6d-9b57-4671-ab60-0a31fadfa33a" />
<img width="1466" alt="Ekran Resmi 2025-07-08 10 02 08" src="https://github.com/user-attachments/assets/76ae2078-5657-4557-a0f7-69ba9dd61ab1" />
<img width="1460" alt="Ekran Resmi 2025-07-08 10 02 33" src="https://github.com/user-attachments/assets/e68547f9-d8ab-4b7f-9ace-c6d06d797a32" />
<img width="1446" alt="Ekran Resmi 2025-07-08 10 02 43" src="https://github.com/user-attachments/assets/a698c029-6e1c-4a84-8426-399729f47a3e" />
<img width="1466" alt="Ekran Resmi 2025-07-08 10 02 58" src="https://github.com/user-attachments/assets/67b47409-a7dd-4cc8-a4e8-edacf2ae0dc7" />
<img width="1456" alt="Ekran Resmi 2025-07-08 10 04 09" src="https://github.com/user-attachments/assets/946815bc-f367-4348-99fa-5f4f7e580f1e" />


## 📌 Notlar

- Bu proje eğitim amaçlı geliştirilmiştir.
- Kodlar üzerinde değişiklik yaparak kendi kitap arşivinizi oluşturabilirsiniz.
- Geri bildirimlerinizi paylaşmaktan çekinmeyin 😊

---

📝 Lisans: MIT  
👩‍💻 Geliştirici: [@zeynephelinaydin](https://github.com/zeynephelinaydin)
