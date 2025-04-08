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
        private readonly HttpClient _httpClient;

        // The API URL to initiate Google login
        private string googleAuthUrl = "http://localhost:5291/api/auth/google"; // The URL to trigger Google login
        private string redirectUri = "http://localhost:5291/auth/callback"; // The redirect URI where the authorization code will be sent

        public LoginCommand(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task ExecuteAsync()
        {
            // Step 1: Open the browser to initiate the OAuth login flow
            Console.WriteLine("Opening Google login page...");
            OpenGoogleLogin();

            // Step 2: Wait for the user to complete the login and catch the code
            Console.WriteLine($"After logging in with Google, please paste the authorization code from the URL (after {redirectUri})");

            // This is where the user would provide the code received after redirect
            Console.Write("Enter the authorization code from the URL: ");
            string authorizationCode = Console.ReadLine();

            if (string.IsNullOrEmpty(authorizationCode))
            {
                Console.WriteLine("No authorization code provided.");
                return;
            }

            // Step 3: Exchange the authorization code for an access token
            await ExchangeCodeForTokenAsync(authorizationCode);
        }

        private void OpenGoogleLogin()
        {
            // This will open the URL in the default web browser
            Process.Start(new ProcessStartInfo(googleAuthUrl + "?redirect_uri=" + redirectUri) { UseShellExecute = true });
        }

        private async Task ExchangeCodeForTokenAsync(string code)
        {
            var tokenRequest = new
            {
                Code = code,
                RedirectUri = redirectUri
            };

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(tokenRequest), System.Text.Encoding.UTF8, "application/json");

            try
            {
                // Send a POST request to exchange the authorization code for an access token
                var response = await _httpClient.PostAsync("http://localhost:5291/api/auth/exchange", content);

                if (response.IsSuccessStatusCode)
                {
                    // Here you can handle the successful response, which would contain the access token
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Successfully authenticated! Response: {responseBody}");
                }
                else
                {
                    Console.WriteLine($"Failed to authenticate. Response: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

}
