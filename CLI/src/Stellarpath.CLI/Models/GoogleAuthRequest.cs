using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Stellarpath.CLI.Models
{
    public class GoogleAuthRequest
    {
        [JsonPropertyName("idToken")]
        public string IdToken { get; set; }

        [JsonPropertyName("authToken")]
        public string AuthToken { get; set; }
    }
}
