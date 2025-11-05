namespace TESTPROJESI.Models
{
    public class LoginRequest
    {
        public string grant_type { get; set; } = "password";
        public string branchcode { get; set; }
        public string password { get; set; }
        public string username { get; set; }
        public string dbname { get; set; }
        public string dbuser { get; set; }
        public string dbpassword { get; set; }
        public string dbtype { get; set; }
    }
}
