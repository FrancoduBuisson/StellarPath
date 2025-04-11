using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stellarpath.CLI.Models
{
    public class AppState
    {
        public string JwtToken { get; set; }
        public UserInfo CurrentUser { get; set; }
        public bool IsLoggedIn { get; set; } = false;
    }
}
