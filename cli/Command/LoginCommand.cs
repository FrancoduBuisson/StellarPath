using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CLI.Command
{
    public class LoginCommand
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string _baseUrl = "https://localhost:5291/api/auth";

        //public LoginCommand(HttpClient client)
        //{
        //    this.client = client;
        //}

        public async Task LoginAsync()
        {
            string? idToken = null;

            // Start local callback server
            var tokenTask = Services.GoogleAuthService.StartLocalCallbackServer(_baseUrl);

            try
            {
                // Request login URL from API
                var loginResponse = await _client.GetStringAsync(_baseUrl + "/google");
                Console.WriteLine("Open this URL to log in: " + loginResponse);

                // Open browser automatically
                try
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {loginResponse}") { CreateNoWindow = true });
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to open browser: " + e.Message);
                }

                // 2️⃣ Wait for user to authenticate and retrieve id token
                idToken = await Task.WhenAny(tokenTask, Task.Delay(60000)) == tokenTask
                    ? tokenTask.Result
                    : null;

                if (idToken != null)
                {
                    //Call auth api with idToken
                    Services.GoogleAuthService.DecodeAndStoreUserInfo(idToken);
                }
                else
                {
                    Console.WriteLine("Failed to retrieve id token.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during token retrieval: " + e.Message);
            }
        }


    }

}
