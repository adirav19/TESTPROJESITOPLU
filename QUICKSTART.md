# 🚀 Minimal MVC Projesi - Hızlı Başlangıç

## 📦 Dosya Yapısı (Toplam 13 dosya)

```
MinimalProject/
│
├── 📂 Controllers/ (3 dosya)
│   ├── HomeController.cs          # Ana sayfa
│   ├── CariController.cs          # Tüm cari işlemleri
│   └── FinishedGoodsController.cs # Tüm üretim fişi işlemleri
│
├── 📂 Views/
│   ├── Home/Index.cshtml          # Ana sayfa görünümü
│   ├── Cari/Index.cshtml          # Cari ekranı (liste + CRUD)
│   ├── FinishedGoods/Index.cshtml # Üretim fişi ekranı
│   ├── Shared/_Layout.cshtml      # Ana layout
│   ├── _ViewStart.cshtml          # Layout ayarı
│   └── _ViewImports.cshtml        # Tag helpers
│
├── Program.cs                     # Minimal startup
├── appsettings.json               # API ayarları
├── MinimalProject.csproj          # Proje dosyası
├── .gitignore                     # Git ignore
└── README.md                      # Dokümantasyon
```

## ⚡ Özellikler

### ✅ Yapılanlar
- Controller içinde tüm business logic
- DTO'lar controller içinde tanımlı
- Token yönetimi Memory Cache ile
- HttpClient direkt kullanımı
- AJAX ile sayfa yenilemeden CRUD
- Inline editing (double click)
- Arama/filtreleme

### ❌ Yapılmayanlar (İstemediğiniz şeyler)
- ❌ Services klasörü YOK
- ❌ Business klasörü YOK
- ❌ Interface'ler YOK (IService, IRepository vb.)
- ❌ Ayrı DTO klasörü YOK
- ❌ Middleware fazlalığı YOK
- ❌ Dependency Injection karmaşası YOK

## 🎯 Controller Yapısı

Her controller şunları içerir:
1. **Token Yönetimi** - Cache ile otomatik
2. **HTTP İstemcisi** - HttpClientFactory
3. **CRUD Metodları** - GET, POST, PUT, DELETE
4. **DTO Tanımları** - Controller içinde
5. **Helper Metodlar** - JSON parse için

## 🔧 Kurulum

1. Projeyi açın
2. `appsettings.json` dosyasında API bilgilerinizi güncelleyin:

```json
"NetOpenX": {
    "BaseUrl": "http://localhost:7172/api/v2",
    "Username": "NETSIS",
    "Password": "Cm1521*.",
    "DbName": "DONANIMURETIM"
    // ... diğer ayarlar
}
```

3. Terminal'de çalıştırın:

```bash
dotnet restore
dotnet run
```

4. Tarayıcıda açın: `https://localhost:7123`

## 📱 Kullanım

### Cari İşlemleri (`/Cari/Index`)

**Yeni Cari Ekle:**
- Sol üst formu doldurun
- "➕ Oluştur" butonuna tıklayın

**Cari Güncelle:**
- Orta formu doldurun
- "✏️ Güncelle" butonuna tıklayın

**Cari Sil:**
- Sağ formda kodu girin
- "🗑️ Sil" butonuna tıklayın

**Inline Düzenleme:**
- Tabloda herhangi bir hücreye çift tıklayın
- Değeri değiştirin
- Enter'a basın veya dışarı tıklayın

**Arama:**
- Arama kutusuna yazmaya başlayın
- Tablo otomatik filtrelenir

### Üretim Fişleri (`/FinishedGoods/Index`)

**Fiş Listesi:**
- Sayfa açıldığında otomatik yüklenir

**Detay Görüntüleme:**
- "🔍" butonuna tıklayın
- Modal pencerede detaylar açılır

**Miktar Güncelleme:**
- Detay modalında miktar hücresine çift tıklayın
- Yeni miktarı girin
- Enter'a basın veya dışarı tıklayın

## 🔍 Kod Örnekleri

### Controller İçinde Token Alma

```csharp
private async Task<string> GetTokenAsync()
{
    if (_cache.TryGetValue("Token", out string token))
        return token;
    
    // Token al ve cache'le
    // ...
}
```

### Controller İçinde API Çağrısı

```csharp
[HttpGet]
public async Task<IActionResult> List()
{
    var token = await GetTokenAsync();
    var client = _httpClientFactory.CreateClient("NetOpenX");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
    
    var response = await client.GetAsync("/ARPs?limit=50");
    // ...
}
```

### View'da AJAX Çağrısı

```javascript
const res = await fetch("/Cari/Create", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(dto)
});
```

## 🎨 UI/UX Özellikleri

- ✅ Bootstrap 5 ile modern tasarım
- ✅ Responsive (mobil uyumlu)
- ✅ Renkli kartlar (success, warning, danger)
- ✅ Hover efektleri
- ✅ Loading göstergeleri
- ✅ Success/error renklendirmeleri

## 📊 Veri Akışı

```
View (AJAX)
    ↓
Controller (Token + HTTP)
    ↓
NetOpenX API
    ↓
Controller (JSON Parse)
    ↓
View (Render)
```

## 🐛 Hata Yönetimi

- Try-catch blokları controller içinde
- Serilog ile loglama
- User-friendly hata mesajları
- Console'da detaylı loglar

## 💡 İpuçları

1. **Token Otomatik:** İlk API çağrısında token alınır ve 20 dk cache'lenir
2. **Inline Edit:** Çift tıklama ile düzenleme yapabilirsiniz
3. **Arama:** Tüm alanlarda çalışır (kod, isim, tel, il)
4. **Modal:** Detaylar modal pencerede açılır
5. **AJAX:** Tüm işlemler sayfa yenilemeden çalışır

## 🔐 Güvenlik Notları

- Token'lar memory cache'de tutulur
- HTTPS kullanın (production'da)
- appsettings.json'u git'e eklemeyin
- Şifreleri environment variable'da tutun

## 📝 Geliştirme Önerileri

### Eklemek İsterseniz:
- Validation (controller içinde)
- Pagination (controller metodunda)
- Export (Excel/PDF - yeni action)
- Advanced filtering (query params)

### Eklemeyin:
- ❌ Servis katmanı
- ❌ Repository pattern
- ❌ AutoMapper
- ❌ MediatR
- ❌ CQRS

## 🆘 Sorun Giderme

**Token alınamıyor:**
- appsettings.json'daki bilgileri kontrol edin
- API'nin çalıştığından emin olun

**Liste gelmiyor:**
- Browser console'u kontrol edin (F12)
- Network tab'ında istekleri inceleyin
- API endpoint'lerini kontrol edin

**Güncelleme çalışmıyor:**
- Browser console'da hata var mı?
- DTO alanları doğru mu?
- Token geçerli mi?

## 📞 Destek

Sorun yaşarsanız:
1. Browser console'u kontrol edin
2. Logs/ klasöründeki logları inceleyin
3. API yanıtlarını kontrol edin

---

**Not:** Bu proje minimalist olacak şekilde tasarlanmıştır. 
Tüm işlemler Controller + View ile yapılır. Ekstra katman yoktur.
