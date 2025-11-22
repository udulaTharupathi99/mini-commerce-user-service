using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCommerce.UserService.Infrastructure.Services
{
    public class JwtSettings
    {
        public string Secret { get; set; } = null!;
        public string Issuer { get; set; } = "MiniCommerce.UserService";
        public string Audience { get; set; } = "MiniCommerceClients";
        public int AccessTokenExpirationSeconds { get; set; } = 3600; // 1 hour
        public int RefreshTokenDays { get; set; } = 30; // refresh token lifetime
    }
}
