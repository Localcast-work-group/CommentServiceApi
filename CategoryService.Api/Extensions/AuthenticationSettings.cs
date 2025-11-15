namespace CategoryService.Api.Extensions
{
    public class AuthenticationSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public int ExpireHours { get; set; }
        public int RefreshExpireDays { get; set; }
    }
}
