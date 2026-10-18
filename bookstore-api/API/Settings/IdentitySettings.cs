namespace API.Settings
{
    public class IdentitySettings
    {
        public string Authority { get; set; } = string.Empty;
        public string ApiName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
        public string AuthorizationUrl { get; set; } = string.Empty;
        public bool RequireHttpsMetadata { get; set; } = false;
    }
}
