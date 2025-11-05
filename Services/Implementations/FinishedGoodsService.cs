using System.Text.Json;
using TESTPROJESI.Business.DTOs;
using TESTPROJESI.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace TESTPROJESI.Services.Implementations
{
    public class FinishedGoodsService : BaseModuleService<FinishedGoodsService>, IFinishedGoodsService
    {
        public FinishedGoodsService(IBaseApiService apiService, ITokenManager tokenManager, ILogger<FinishedGoodsService> logger)
            : base(apiService, tokenManager, logger) { }

        public async Task<List<FinishedGoodsCreateDto>> GetAllAsync(string queryParams = null)
        {
            string token = await _tokenManager.GetTokenAsync();
            string endpoint = "FinishedGoodsReceiptWChanges?limit=20";

            var responseJson = await _apiService.GetAsync<JsonElement>(endpoint, token);
            var list = new List<FinishedGoodsCreateDto>();

            JsonElement dataArray;

            // 🔹 "Data" varsa oradan al, yoksa doğrudan diziyi al
            if (responseJson.ValueKind == JsonValueKind.Object &&
                responseJson.TryGetProperty("Data", out var innerData) && innerData.ValueKind == JsonValueKind.Array)
            {
                dataArray = innerData;
            }
            else if (responseJson.ValueKind == JsonValueKind.Array)
            {
                dataArray = responseJson;
            }
            else
            {
                _logger.LogWarning("⚠️ Beklenmeyen JSON formatı geldi: {Json}", responseJson.ToString());
                return list;
            }

            foreach (var item in dataArray.EnumerateArray())
            {
                list.Add(new FinishedGoodsCreateDto
                {
                    FisNo = TryGetString(item, "UretSon_FisNo"),
                    Tarih = TryGetString(item, "UretSon_Tarih"),
                    Depo = TryGetString(item, "UretSon_Depo"),
                    Malzeme = TryGetString(item, "UretSon_Mamul"),
                    Miktar = TryGetDecimal(item, "UretSon_Miktar"),
                    Birim = "Adet"
                });
            }

            return list;
        }

        // 🔧 Yardımcı metodlar:
        private static string TryGetString(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                return val.ValueKind switch
                {
                    JsonValueKind.String => val.GetString(),
                    JsonValueKind.Number => val.GetRawText(), // sayıyı string olarak döndür
                    _ => ""
                };
            }
            return "";
        }

        private static decimal TryGetDecimal(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.Number && val.TryGetDecimal(out var num))
                    return num;

                if (val.ValueKind == JsonValueKind.String && decimal.TryParse(val.GetString(), out var strNum))
                    return strNum;
            }
            return 0;
        }



        public async Task<FinishedGoodsDetailDto> GetByIdAsync(string fisNo)
        {
            string token = await _tokenManager.GetTokenAsync();
            string endpoint = $"FinishedGoodsReceiptWChanges/{fisNo}";

            var responseJson = await _apiService.GetAsync<JsonElement>(endpoint, token);

            if (responseJson.ValueKind != JsonValueKind.Object ||
                !responseJson.TryGetProperty("Data", out var data))
            {
                _logger.LogWarning("⚠️ Beklenmeyen JSON formatı geldi: {Json}", responseJson.ToString());
                return null;
            }

            var dto = new FinishedGoodsDetailDto
            {
                UretSon_FisNo = TryGetString(data, "UretSon_FisNo"),
                UretSon_Tarih = TryGetString(data, "UretSon_Tarih"),
                UretSon_SipNo = TryGetString(data, "UretSon_SipNo"),
                UretSon_Mamul = TryGetString(data, "UretSon_Mamul"),
                UretSon_Miktar = TryGetDecimal(data, "UretSon_Miktar"),
                UretSon_Depo = (int)TryGetDecimal(data, "UretSon_Depo"),
                Aciklama = TryGetString(data, "Aciklama"),
                KayitYapanKul = TryGetString(data, "KayitYapanKul"),
                Kalem = new List<KalemDto>()
            };

            // 🔹 Kalem listesini işle
            if (data.TryGetProperty("Kalem", out var kalemArray) && kalemArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in kalemArray.EnumerateArray())
                {
                    dto.Kalem.Add(new KalemDto
                    {
                        Index = (int)TryGetDecimal(item, "Index"),
                        IncKeyNo = (int)TryGetDecimal(item, "IncKeyNo"),
                        StokKodu = TryGetString(item, "StokKodu"),
                        DepoKodu = (int)TryGetDecimal(item, "DepoKodu"),
                        Miktar = (double)TryGetDecimal(item, "Miktar"),
                        Aciklama = TryGetString(item, "Aciklama"),
                        SeriVarMi = TryGetBool(item, "SeriVarMi"),
                        BGTIP = TryGetString(item, "BGTIP")
                    });
                }
            }

            return dto;
        }

        // 🔧 Yeni yardımcı metot:
        private static bool TryGetBool(JsonElement item, string propName)
        {
            if (item.TryGetProperty(propName, out var val))
            {
                if (val.ValueKind == JsonValueKind.True) return true;
                if (val.ValueKind == JsonValueKind.False) return false;
            }
            return false;
        }


        public async Task<JsonElement> CreateAsync(FinishedGoodsCreateDto dto)
        {
            // Şimdilik mock data dönüyor, ileride gerçek POST eklenecek
            string json = @"{ ""success"": true, ""message"": ""Mock fiş kaydedildi!"" }";
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        public async Task<JsonElement> UpdateQuantityAsync(KalemDto dto)
        {
            string token = await _tokenManager.GetTokenAsync();

            // önce mevcut fiş detayını çek
            // 💡 geçici olarak test fiş numarasını buraya koy
            var fisNo = dto.FisNo;
            if (string.IsNullOrWhiteSpace(fisNo))
            {
                string json = @"{ ""success"": false, ""message"": ""Fiş numarası belirtilmedi!"" }";
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
            var current = await GetByIdAsync(fisNo);

            // mevcut fiş yoksa çık
            if (current == null)
            {
                string json = @"{ ""success"": false, ""message"": ""Fiş bulunamadı!"" }";
                return JsonSerializer.Deserialize<JsonElement>(json);
            }

            // sadece değişen kalemi güncelle
            var updatedKalem = current.Kalem.FirstOrDefault(x => x.StokKodu == dto.StokKodu);
            if (updatedKalem != null)
                updatedKalem.Miktar = dto.Miktar;

            // ✅ NetOpenX Save endpoint'ine doğru formatta gönder
            var payload = new
            {
                current.UretSon_FisNo,
                current.UretSon_Tarih,
                current.UretSon_Depo,
                current.UretSon_Mamul,
                current.UretSon_Miktar,
                current.Aciklama,
                current.KayitYapanKul,
                Kalem = current.Kalem
            };

            try
            {
                var response = await _apiService.PostAsync<JsonElement>(
                    "FinishedGoodsReceiptWChanges/Save",
                    payload,
                    token
                );

                _logger.LogInformation("✅ {StokKodu} stok kaleminin miktarı {Miktar} olarak kaydedildi (Fiş: {Fis})",
                    dto.StokKodu, dto.Miktar, current.UretSon_FisNo);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Miktar güncelleme başarısız: {StokKodu}", dto.StokKodu);
                string json = @"{ ""success"": false, ""message"": ""Sunucu hatası!"" }";
                return JsonSerializer.Deserialize<JsonElement>(json);
            }
        }


    }
}
