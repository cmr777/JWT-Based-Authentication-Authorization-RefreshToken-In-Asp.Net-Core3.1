namespace WebApi.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string AccessTokenExpiration { get; set; }
        public string RefreshTokenExpiration { get; set; }
    }
}