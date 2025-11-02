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
        public string Issuer { get; set; } = "mini-commerce";
        public string Audience { get; set; } = "mini-commerce";
        public int ExpiryMinutes { get; set; } = 60 * 24; // 1 day default
    }
}
