# 🔧 Sorun Giderme Rehberi

## ❌ Yaygın Hatalar ve Çözümleri

### 1. Token 404 Hatası

**Hata:**
```
Response status code does not indicate success: 404 (Not Found).
POST http://localhost:7172/token
```

**Neden:**
- NetOpenX API'si çalışmıyor
- BaseUrl yanlış girilmiş
- Token endpoint'i farklı

**Çözüm:**
1. NetOpenX API'sinin çalıştığından emin olun
2. `appsettings.json` dosyasını kontrol edin:
```json
"NetOpenX": {
    "BaseUrl": "http://localhost:7172/api/v2"  // ✅ Doğru format
}
```
3. Token endpoint'ini test edin:
```bash
curl -X POST http://localhost:7172/api/v2/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=NETSIS&password=..."
```

### 2. Token Cache Hatası

**Hata:**
```
Token bulunamadı! Önce login olun.
```

**Neden:**
- IConfiguration inject edilmemiş
- Cache çalışmıyor

**Çözüm:**
Controller constructor'ında `IConfiguration` olduğundan emin olun:
```csharp
public CariController(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    IConfiguration config,  // ✅ Bu olmalı
    ILogger<CariController> logger)
```

### 3. CORS Hatası

**Hata:**
```
Access to fetch has been blocked by CORS policy
```

**Neden:**
- Frontend'den farklı port'a istek atılıyor
- API CORS ayarları yok

**Çözüm:**
NetOpenX API'sinde CORS ayarlarını yapın veya aynı origin'den çağırın.

### 4. Liste Gelmiyor

**Hata:**
```
Yükleniyor... (sonsuza kadar)
```

**Neden:**
- Token alınamadı
- API endpoint yanlış
- JSON parse hatası

**Çözüm:**
1. Browser Console'u açın (F12)
2. Network tab'ına bakın
3. İsteği kontrol edin:
   - Status Code: 200 ✅ / 400-500 ❌
   - Response: JSON formatında mı?
4. Console'da hata var mı kontrol edin

### 5. Inline Edit Çalışmıyor

**Hata:**
- Çift tıklama yapılıyor ama değişmiyor

**Neden:**
- `data-editable="true"` atribütü yok
- JavaScript çalışmıyor

**Çözüm:**
View dosyasında:
```html
<td class="editable">...</td>  <!-- ✅ class olmalı -->
```

## 🔍 Debug İpuçları

### Logları İnceleyin

```bash
# Log dosyasını açın
tail -f Logs/app_*.txt

# Token alınıyor mu?
[15:30:47 INF] 🔐 Yeni token alınıyor...
[15:30:47 INF] 📍 Token URL: http://localhost:7172/api/v2/token
[15:30:47 INF] ✅ Token alındı ve cache'lendi
```

### Browser Console

```javascript
// Token var mı kontrol et
fetch('/Cari/List')
  .then(r => r.json())
  .then(d => console.log(d))

// Başarılıysa: Array veya Object döner
// Hatalıysa: { success: false, message: "..." }
```

### API Test

```bash
# Postman veya curl ile test edin
curl -X GET http://localhost:5093/Cari/List
```

## 🛠️ Genel Kontrol Listesi

**Uygulama Başlatmadan Önce:**
- [ ] NetOpenX API çalışıyor mu?
- [ ] `appsettings.json` güncel mi?
- [ ] NuGet paketleri yüklü mü? (`dotnet restore`)

**Hata Alırsanız:**
- [ ] Logs klasörünü kontrol ettiniz mi?
- [ ] Browser console'da hata var mı?
- [ ] Network tab'ında istek başarılı mı?
- [ ] Token endpoint'i doğru mu?

**Performans Sorunları:**
- [ ] Token cache çalışıyor mu? (Log'da "🔁 Cache'den token alındı" yazmalı)
- [ ] API response süreleri normal mi?
- [ ] Çok fazla kayıt geliyorsa limit azaltın

## 📞 Yardım

Hala sorun varsa:
1. Logs/app_*.txt dosyasını inceleyin
2. Browser console'daki hataları kontrol edin
3. Network tab'ındaki istekleri inceleyin
4. README.md ve QUICKSTART.md dosyalarını okuyun

## 🎯 Sık Sorulan Sorular

**Q: Token süresi dolunca ne olur?**
A: Otomatik yeni token alınır. Cache 20 dakika geçerli.

**Q: Birden fazla controller token alırsa sorun olur mu?**
A: Hayır, cache paylaşımlı. İlk token alan cache'e koyar, diğerleri oradan kullanır.

**Q: BaseUrl nasıl değiştirilir?**
A: `appsettings.json` dosyasında `NetOpenX:BaseUrl` değerini güncelleyin.

**Q: Development ve Production ayarları ayrı mı?**
A: `appsettings.Development.json` oluşturup farklı ayarlar girebilirsiniz.

**Q: HTTPS zorunlu mu?**
A: Development'ta hayır, production'da evet (Program.cs'de ayarlanabilir).
