
namespace Stellarpath.CLI.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserInfo User { get; set; }
    }
}
