# Minimal MVC Projesi

## 📁 Proje Yapısı

```
MinimalProject/
├── Controllers/           # Tüm business logic burada
│   ├── HomeController.cs
│   ├── CariController.cs
│   └── FinishedGoodsController.cs
├── Views/
│   ├── Home/
│   ├── Cari/
│   ├── FinishedGoods/
│   └── Shared/
├── Program.cs            # Minimal konfigürasyon
├── appsettings.json      # API ayarları
└── MinimalProject.csproj
```

## ✨ Özellikler

- ✅ **Sadece Controller + View** yapısı
- ✅ Servis katmanı YOK
- ✅ Business klasörü YOK
- ✅ DTO'lar controller içinde
- ✅ Token yönetimi Memory Cache ile
- ✅ HttpClient direkt kullanım
- ✅ Inline editing (double click)
- ✅ AJAX CRUD işlemleri

## 🚀 Kullanım

1. `appsettings.json` dosyasında NetOpenX ayarlarını yapın
2. Projeyi çalıştırın: `dotnet run`
3. Ana sayfadan modüllere gidin

## 📋 Modüller

### Cari İşlemleri (`/Cari/Index`)
- Cari listeleme
- Yeni cari ekleme
- Cari güncelleme
- Cari silme
- Inline editing (çift tıklama)

### Üretim Fişleri (`/FinishedGoods/Index`)
- Fiş listeleme
- Fiş detay görüntüleme
- Miktar güncelleme (inline)

## 🔧 Teknik Detaylar

- **Framework**: ASP.NET Core 8.0
- **Pattern**: Controller + View (No Service Layer)
- **Cache**: Memory Cache
- **Logging**: Serilog
- **HTTP**: HttpClientFactory
- **UI**: Bootstrap 5

## 📝 Notlar

- Token otomatik cache'lenir (20 dk)
- Tüm AJAX işlemleri sayfa yenilemeden çalışır
- Hata logları `Logs/` klasöründe
