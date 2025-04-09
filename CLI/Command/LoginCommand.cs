using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace CLI.Command
{
    public class LoginCommand
    {

        private static string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
        private static string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json");

        public async Task LoginAsync()
        {
            string[] Scopes = { Oauth2Service.Scope.UserinfoProfile };


            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(new FileStream(credPath, FileMode.Open, FileAccess.Read)).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("token.json", true));

            var oauthService = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Auth C# Console"
            });

            // Get the user's information
            UserInformation userInfo = await oauthService.Userinfo.Get().ExecuteAsync();
            Console.WriteLine($"Welcome: { userInfo.Name}");
        }

        public void Logout()
        {

            if (Directory.Exists(tokenPath))
            {
                Directory.Delete(tokenPath, true);
                Console.WriteLine("You have successfully logged out.");
            }
            else
            {
                Console.WriteLine("No active session found.");
            }
        }

    }
    internal class UserInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public static implicit operator UserInformation(Userinfo v)
        {
            if(v == null) 
            {
                throw new NotImplementedException();
            }

            return new UserInformation
            {
                Id = v.Id,
                Name = v.Name
            };

        }
    }

}
