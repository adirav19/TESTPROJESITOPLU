namespace TESTPROJESI.Business.DTOs
{
    public class FinishedGoodsDetailDto
    {
        public string UretSon_FisNo { get; set; }
        public string UretSon_Tarih { get; set; }
        public string UretSon_SipNo { get; set; }
        public string UretSon_Mamul { get; set; }
        public decimal UretSon_Miktar { get; set; }
        public int UretSon_Depo { get; set; }
        public string Aciklama { get; set; }
        public string KayitYapanKul { get; set; }
        public List<KalemDto> Kalem { get; set; }
    }

    public class KalemDto
    {
        public string FisNo { get; set; }   // ✅ yeni eklendi
        public int Index { get; set; }
        public int IncKeyNo { get; set; }
        public string StokKodu { get; set; }
        public int DepoKodu { get; set; }
        public double Miktar { get; set; }
        public string Aciklama { get; set; }
        public bool SeriVarMi { get; set; }
        public string BGTIP { get; set; }
    }

}
