using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using StellarPath.ConsoleClient;

namespace Stellarpath.CLI.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("user")]
        public UserInfo User { get; set; }
    }
}
