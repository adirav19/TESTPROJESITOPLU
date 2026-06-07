namespace TESTPROJESI.Models
{
    public class BOMDto
    {
        public string  MamulKodu     { get; set; } = "";
        public decimal ReceteToplami { get; set; }
        public string  OlcuBirimi    { get; set; } = "";
    }

    public class BOMDetailDto
    {
        public string          PrmMamulKodu  { get; set; } = "";
        public string          MamulYapKod   { get; set; } = "";
        public decimal         ReceteToplami { get; set; }
        public string          OlcuBirimi    { get; set; } = "";
        public int             ReceteSayisi  { get; set; }
        public string          PrmOprBil     { get; set; } = "";
        public List<BOMItemDto> BOMItemList  { get; set; } = new();
    }

    public class BOMItemDto
    {
        public string  Mamul_Kodu  { get; set; } = "";
        public string  Ham_Kodu    { get; set; } = "";
        public decimal Miktar      { get; set; }
        public string  OpNo        { get; set; } = "";
        public string  Opr_Bil     { get; set; } = "";
        public string  H_Stok_Adi  { get; set; } = "";
        public string  H_Olcu_Br1  { get; set; } = "";
        public decimal FireMik     { get; set; }
        public string  Aciklama    { get; set; } = "";
    }

    public class BOMCreateDto
    {
        public string              PrmMamulKodu  { get; set; } = "";
        public decimal             ReceteToplami { get; set; }
        public List<BOMCreateItemDto> BOMItemList { get; set; } = new();
    }

    public class BOMCreateItemDto
    {
        public string  OpNo     { get; set; } = "";
        public string  Opr_Bil  { get; set; } = "";
        public string  Ham_Kodu { get; set; } = "";
        public decimal Miktar   { get; set; }
    }

    public class BOMUpdateFullDto
    {
        public string                  PrmMamulKodu  { get; set; } = "";
        public decimal                 ReceteToplami { get; set; }
        public string                  OlcuBirimi    { get; set; } = "";
        public List<BOMUpdateFullItemDto> BOMItemList { get; set; } = new();
    }

    public class BOMUpdateFullItemDto
    {
        public string  OpNo          { get; set; } = "";
        public string  Opr_Bil       { get; set; } = "";
        public string  Ham_Kodu      { get; set; } = "";
        public decimal Miktar        { get; set; }
        public decimal FireMik       { get; set; }
        public int?    IncKeyNo      { get; set; }
        public bool    SonOperasyon  { get; set; }
    }

    public class BOMUpdateReceteDto
    {
        public string  MamulKodu         { get; set; } = "";
        public decimal YeniReceteToplami { get; set; }
    }

    public class BOMItemUpdateDto
    {
        public string  MamulKodu  { get; set; } = "";
        public string  Ham_Kodu   { get; set; } = "";
        public decimal YeniMiktar { get; set; }
    }

    public class DeleteBOMItemDto
    {
        public string ItemCode     { get; set; } = "";
        public string ConfigCode   { get; set; } = "";
        public int    ReceteIndex  { get; set; }
    }
}
