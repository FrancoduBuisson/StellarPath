using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using Spectre.Console;
using Stellarpath.CLI.Models;

namespace StellarPath.ConsoleClient.Services.Auth
{
    public class LoginHandler
    {
        private readonly HttpClient _httpClient;

        private const string ClientId = "834046723373-j2obq430fp7sfc538uk6m42o4rmbmgvf.apps.googleusercontent.com";
        private const string RedirectUri = "http://localhost:5500/callback";
        private const string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/auth";
        private const string Scope = "openid email profile";

        public LoginHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(string IdToken, string AccessToken)> GetGoogleAuth()
        {
            try
            {
                var authUrl = GetAuthorizationUrl();
                AnsiConsole.MarkupLine("[grey]Opening browser for Google authentication...[/]");
                OpenBrowser(authUrl);

                var (idToken, accessToken) = await WaitForAuthorizationCode();

                if (!string.IsNullOrEmpty(idToken))
                {
                    AnsiConsole.MarkupLine("[green]Google authentication successful![/]");
                    return (idToken, accessToken);
                }

                AnsiConsole.MarkupLine("[red]Google authentication failed or timed out.[/]");
                return (null, null);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred during authentication: {ex.Message}[/]");
                return (null, null);
            }
        }

        private string GetAuthorizationUrl()
        {
            var encodedRedirectUri = Uri.EscapeDataString(RedirectUri);
            var encodedScope = Uri.EscapeDataString(Scope);
            var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            return $"{GoogleAuthUrl}?" +
                   $"client_id={ClientId}" +
                   $"&redirect_uri={encodedRedirectUri}" +
                   $"&scope={encodedScope}" +
                   $"&response_type=id_token token" +
                   $"&prompt=select_account" +
                   $"&nonce={nonce}";
        }

        private void OpenBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                AnsiConsole.MarkupLine($"[yellow]Unable to open browser. Please visit: {url}[/]");
            }
        }

        private async Task<(string, string)> WaitForAuthorizationCode()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5500/");

            var tcs = new TaskCompletionSource<(string, string)>();
            var timeout = TimeSpan.FromMinutes(5);
            var cts = new CancellationTokenSource(timeout);

            try
            {
                listener.Start();
                AnsiConsole.MarkupLine("[grey]Waiting for authentication callback...[/]");

                cts.Token.Register(() =>
                {
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.TrySetCanceled();
                        listener.Stop();
                    }
                });

                _ = Task.Run(async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        var context = await listener.GetContextAsync();

                        if (context.Request.Url.AbsolutePath == "/callback")
                        {
                            string html = @"
                                <html>
                                    <head><title>Auth Successful</title></head>
                                    <body>
                                        <h1>Authentication successful!</h1>
                                        <p>You can close this window.</p>
                                        <script>
                                            const hash = window.location.hash.substring(1);
                                            const params = new URLSearchParams(hash);
                                            const idToken = params.get('id_token');
                                            const accessToken = params.get('access_token');
                                            fetch('/token?id_token=' + idToken + '&access_token=' + accessToken, { method: 'POST' });
                                        </script>
                                    </body>
                                </html>";
                            var buffer = Encoding.UTF8.GetBytes(html);
                            context.Response.ContentLength64 = buffer.Length;
                            await context.Response.OutputStream.WriteAsync(buffer);
                            context.Response.Close();
                        }
                        else if (context.Request.Url.AbsolutePath == "/token")
                        {
                            var query = HttpUtility.ParseQueryString(context.Request.Url.Query);
                            var idToken = query["id_token"];
                            var accessToken = query["access_token"];

                            context.Response.StatusCode = 200;
                            var buffer = Encoding.UTF8.GetBytes("Token received");
                            await context.Response.OutputStream.WriteAsync(buffer);
                            context.Response.Close();

                            tcs.TrySetResult((idToken, accessToken));
                            listener.Stop();
                            break;
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            context.Response.Close();
                        }
                    }
                });

                return await tcs.Task;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]HTTP listener error: {ex.Message}[/]");
                return (null, null);
            }
            finally
            {
                if (listener.IsListening)
                    listener.Stop();
            }
        }

        public async Task<AuthResult> AuthenticateWithBackend((string IdToken, string AccessToken) authTuple)
        {
            var result = new AuthResult { Success = false };

            try
            {
                AnsiConsole.MarkupLine("[grey]Authenticating with StellarPath API...[/]");

                var request = new GoogleAuthRequest
                {
                    IdToken = authTuple.IdToken,
                    AuthToken = authTuple.AccessToken
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("/api/auth/google", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    AnsiConsole.MarkupLine($"[red]Authentication failed: {response.StatusCode}[/]");
                    AnsiConsole.MarkupLine($"[grey]{error}[/]");
                    return result;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, options);

                if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                {
                    AnsiConsole.MarkupLine("[red]Invalid response from API.[/]");
                    return result;
                }

                result.Success = true;
                result.Token = authResponse.Token;
                result.User = authResponse.User;
                return result;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Backend authentication error: {ex.Message}[/]");
                return result;
            }
        }
    }
}
