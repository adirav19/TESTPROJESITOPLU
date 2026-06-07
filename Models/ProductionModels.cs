namespace TESTPROJESI.Models
{
    public class UakKayitDto
    {
        public string   IsEmriNo          { get; set; } = "";
        public string?  CONFSIRANO        { get; set; }
        public string   OpKodu            { get; set; } = "";
        public string   AKTIVITEKODU      { get; set; } = "";
        public DateTime BASLANGICTARIH    { get; set; } = DateTime.Now;
        public DateTime BITISTARIHSAAT    { get; set; } = DateTime.Now;
        public decimal  URETILENMIKTAR    { get; set; }
        public decimal  FIREMIKTAR        { get; set; }
        public int      USKDEPOKODU       { get; set; } = 1;
        public List<FireDetayDto> ShrinkageDetailList { get; set; } = new();
    }

    public class FireDetayDto
    {
        public string  FireKodu { get; set; } = "";
        public decimal Miktar   { get; set; }
    }

    public class ProductionOrderVm
    {
        public string    IsEmriNo      { get; set; } = "";
        public DateTime? Tarih         { get; set; }
        public string    StokKodu      { get; set; } = "";
        public decimal   Miktar        { get; set; }
        public int       DepoKodu      { get; set; }
        public int       CikisDepoKodu { get; set; }
        public DateTime? TeslimTarihi  { get; set; }
        public string    SiparisNo     { get; set; } = "";
    }

    public class CreateProductionOrderDto
    {
        public string  IsEmriNo      { get; set; } = "";
        public string  Tarih         { get; set; } = "";
        public string  StokKodu      { get; set; } = "";
        public decimal Miktar        { get; set; }
        public string  TeslimTarihi  { get; set; } = "";
        public string  SiparisNo     { get; set; } = "";
        public int     DepoKodu      { get; set; }
        public int     CikisDepoKodu { get; set; }
        public string  Aciklama      { get; set; } = "";
    }
}
