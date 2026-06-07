namespace TESTPROJESI.Models
{
    public class CariDto
    {
        public string CARI_KOD  { get; set; } = "";
        public string CARI_ISIM { get; set; } = "";
        public string CARI_TEL  { get; set; } = "";
        public string CARI_IL   { get; set; } = "";
        public string EMAIL     { get; set; } = "";
    }

    public class DeleteDto
    {
        public string cariKodu { get; set; } = "";
    }
}
