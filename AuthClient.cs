namespace Nebula.Identity
{
    public class AuthClient
    {
        public string ClientId { get; set; }
        public string AllowedGrantTypes { get; set; }
        public string ClientUrl { get; set; }
        public int AccessTokenLifetime { get; set; }
    }
}