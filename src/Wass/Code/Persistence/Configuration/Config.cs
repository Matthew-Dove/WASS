namespace Wass.Code.Persistence.Configuration
{
    public static class Config
    {
        public static SecurityConfig Security { get; set; } = new();
        public static S3Config S3 { get; set; } = new();
    }
}
