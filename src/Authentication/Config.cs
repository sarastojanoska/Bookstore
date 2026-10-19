using Duende.IdentityServer.Models;

namespace Authentication
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            [
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            ];

        public static IEnumerable<ApiScope> ApiScopes =>
            [
                new ApiScope("books.crud", "Full CRUD access to Books API"),
                new ApiScope("books.search", "Search access to Books API"),
            ];

        public static IEnumerable<ApiResource> ApiResources =>
            [
                new ApiResource("bookstore", "bookstore API")
                {
                    Scopes = { "books.crud", "books.search" }
                }
            ];

        public static IEnumerable<Client> Clients =>
            [
                // Implicit flow client
                new() {
                    ClientId = "books.search.client",
                    ClientName = "Books search service",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = { "https://localhost:3001/swagger/oauth2-redirect.html" },
                    AllowedScopes = { "openid", "profile", "books.search" },
                    RequireConsent = false
                },

                // Client credentials flow client
                new() {
                    ClientId = "books.service.client",
                    ClientName = "Books service client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "books.crud" },

                    AllowedCorsOrigins = { "https://localhost:3001" }
                },
            ];
    }
}
