namespace TESTPROJESI.Models
{
    public class FastPaymentListDto
    {
        public string KasaKod            { get; set; } = "";
        public string BelgeNo            { get; set; } = "";
        public string CariKod            { get; set; } = "";
        public string IslemTarihi        { get; set; } = "";
        public int    DOVTIP             { get; set; }
        public int    TahsilatKalemAdedi { get; set; }
    }

    public class FastPaymentHeaderInputDto
    {
        public string KasaKod     { get; set; } = "";
        public string CariKod     { get; set; } = "";
        public string BelgeNo     { get; set; } = "";
        public string IslemTarihi { get; set; } = "";
    }

    public class FastPaymentViewDto
    {
        public string               KasaKod     { get; set; } = "";
        public string               CariKod     { get; set; } = "";
        public string               BelgeNo     { get; set; } = "";
        public string               IslemTarihi { get; set; } = "";
        public int                  DOVTIP      { get; set; }
        public List<FastPaymentLineDto> Tahsilats { get; set; } = new();
    }

    public class FastPaymentLineDto
    {
        public string  SozKodu        { get; set; } = "";
        public int     TaksitSay      { get; set; }
        public decimal DovTutar       { get; set; }
        public decimal Kur            { get; set; }
        public decimal Tutar          { get; set; }
        public string  KartNo         { get; set; } = "";
        public string  CRapKod1       { get; set; } = "";
        public string  CRapKod2       { get; set; } = "";
        public string  PLA_KODU       { get; set; } = "";
        public string  Proje_Kodu     { get; set; } = "";
        public string  Referans_Kodu  { get; set; } = "";
        public string  Entegrefkey    { get; set; } = "";
        public string  Aciklama       { get; set; } = "";
        public string  KasaKod        { get; set; } = "";
        public int     TahsilatTipi   { get; set; }
    }

    public class FastPaymentSaveDto
    {
        public string  KasaKod     { get; set; } = "";
        public string  CariKod     { get; set; } = "";
        public string  BelgeNo     { get; set; } = "";
        public string  IslemTarihi { get; set; } = "";
        public int     DOVTIP      { get; set; }
        public List<FastPaymentLineDto> Tahsilats { get; set; } = new();
    }

    public class FastPaymentUpdateLineDto
    {
        public string  KasaKod      { get; set; } = "";
        public string  CariKod      { get; set; } = "";
        public string  BelgeNo      { get; set; } = "";
        public string  IslemTarihi  { get; set; } = "";
        public string  Referans_Kodu { get; set; } = "";
        public decimal EskiTutar    { get; set; }
        public decimal YeniTutar    { get; set; }
    }
}
