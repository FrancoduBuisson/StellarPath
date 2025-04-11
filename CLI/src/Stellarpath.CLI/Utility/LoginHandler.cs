using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Spectre.Console;
using System.Diagnostics;
using global::StellarPath.CLI.Utility;
using global::StellarPath.ConsoleClient;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Utility
{
        public class LoginHandler
        {
            private readonly HttpClient _httpClient;

            // Google OAuth configuration
            private static readonly string ClientId = "834046723373-j2obq430fp7sfc538uk6m42o4rmbmgvf.apps.googleusercontent.com";
            private static readonly string RedirectUri = "http://localhost:5500/callback";
            private static readonly string GoogleAuthUrl = "https://accounts.google.com/o/oauth2/auth";
            private static readonly string Scope = "openid email profile";

            public LoginHandler(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<AuthResult> LoginAsync()
            {
                var result = new AuthResult { Success = false };

                AnsiConsole.Status()
                    .Start("Starting authentication process...", async ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Star);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        // Get Google ID token
                        var authTuple = await GetGoogleAuth();

                        if (string.IsNullOrEmpty(authTuple.Item1))
                        {
                            AnsiConsole.MarkupLine("[red]Authentication failed. Unable to obtain Google ID token.[/]");
                            return;
                        }

                        // Send the ID token to our backend
                        result = await AuthenticateWithBackend(authTuple);

                        if (!result.Success)
                        {
                            AnsiConsole.MarkupLine("[red]Authentication failed. Could not authenticate with StellarPath API.[/]");
                            return;
                        }

                        // Save the session
                        SessionManager.SaveSession(result.Token, result.User);

                        // Configure the HTTP client with the JWT token for future requests
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Token);
                    });

                return result;
            }

            private string GetAuthorizationUrl()
            {
                string encodedRedirectUri = Uri.EscapeDataString(RedirectUri);
                string encodedScope = Uri.EscapeDataString(Scope);
                string nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                return $"{GoogleAuthUrl}" +
                       $"?client_id={ClientId}" +
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
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch
                {
                    AnsiConsole.MarkupLine($"[yellow]Unable to open browser automatically. Please manually navigate to: {url}[/]");
                }
            }

            private async Task<(string, string)> GetGoogleAuth()
            {
                try
                {
                    // Create the authorization URL
                    string authorizationUrl = GetAuthorizationUrl();
                    AnsiConsole.MarkupLine("[grey]Opening browser for Google authentication...[/]");

                    // Open the default browser with the authorization URL
                    OpenBrowser(authorizationUrl);

                    // Start the local HTTP server to receive the callback
                    (string, string) tuple = await WaitForAuthorizationCode();

                    string idToken = tuple.Item1;
                    string accessToken = tuple.Item2;

                    if (!string.IsNullOrEmpty(idToken))
                    {
                        AnsiConsole.MarkupLine("[green]Google authentication successful![/]");
                        return tuple;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Google authentication failed or timed out.[/]");
                        return (null, null);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]An error occurred during Google authentication: {ex.Message}[/]");
                    return (null, null);
                }
            }

            private async Task<(string, string)> WaitForAuthorizationCode()
            {
                var authTaskSource = new TaskCompletionSource<(string, string)>();
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:5500/");

                try
                {
                    listener.Start();
                    AnsiConsole.MarkupLine("[grey]Waiting for authentication callback...[/]");

                    // Set a timeout of 5 minutes
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                    cancellationTokenSource.Token.Register(() =>
                    {
                        if (!authTaskSource.Task.IsCompleted)
                        {
                            authTaskSource.TrySetCanceled();
                            if (listener.IsListening)
                            {
                                listener.Stop();
                            }
                        }
                    });

                    // Begin async listening for the callback
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            bool receivedIdToken = false;

                            while (!receivedIdToken && !cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                try
                                {
                                    var context = await listener.GetContextAsync();

                                    if (context.Request.Url.AbsolutePath == "/callback")
                                    {
                                        // Serve the HTML page with JavaScript to extract ID token from hash fragment
                                        string html = @"
                                    <html>
                                    <head>
                                        <title>Authentication Successful</title>
                                        <style>
                                            body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
                                            .container { max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 5px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
                                            h1 { color: #4CAF50; }
                                        </style>
                                    </head>
                                    <body>
                                        <div class='container'>
                                            <h1>Authentication Successful</h1>
                                            <p>You have successfully authenticated with Google. You can now close this window and return to the StellarPath CLI.</p>
                                        </div>
                                        <script>
                                            const hash = window.location.hash.substring(1);
                                            const params = new URLSearchParams(hash);
                                            const idToken = params.get('id_token');
                                            const accessToken = params.get('access_token');
                                            fetch('/token?id_token=' + idToken + '&access_token=' + accessToken, { method: 'POST' });
                                        </script>
                                    </body>
                                    </html>";

                                        byte[] buffer = Encoding.UTF8.GetBytes(html);
                                        context.Response.ContentLength64 = buffer.Length;
                                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                        context.Response.Close();
                                    }
                                    else if (context.Request.Url.AbsolutePath == "/token")
                                    {
                                        // Extract the ID token from the query string
                                        string query = context.Request.Url.Query;
                                        string idToken = HttpUtility.ParseQueryString(query)["id_token"];
                                        string accessToken = HttpUtility.ParseQueryString(query)["access_token"];

                                        if (!string.IsNullOrEmpty(idToken) && !string.IsNullOrEmpty(accessToken))
                                        {
                                            // Return a simple acknowledgment response
                                            string response = "Token received";
                                            byte[] buffer = Encoding.UTF8.GetBytes(response);
                                            context.Response.ContentLength64 = buffer.Length;
                                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                                            context.Response.Close();

                                            // Complete the task with the ID token
                                            receivedIdToken = true;
                                            authTaskSource.TrySetResult((idToken, accessToken));

                                            // Stop listening after receiving the token
                                            listener.Stop();
                                        }
                                        else
                                        {
                                            context.Response.StatusCode = 400;
                                            context.Response.Close();
                                        }
                                    }
                                    else
                                    {
                                        // Handle any other requests with a 404
                                        context.Response.StatusCode = 404;
                                        context.Response.Close();
                                    }
                                }
                                catch (HttpListenerException ex)
                                {
                                    // Check if the listener has been stopped
                                    if (!listener.IsListening)
                                    {
                                        break;
                                    }

                                    AnsiConsole.MarkupLine($"[red]HTTP listener error: {ex.Message}[/]");
                                    if (!authTaskSource.Task.IsCompleted)
                                    {
                                        authTaskSource.TrySetException(ex);
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Error in HTTP listener: {ex.Message}[/]");
                            if (!authTaskSource.Task.IsCompleted)
                            {
                                authTaskSource.TrySetException(ex);
                            }
                        }
                        finally
                        {
                            // Ensure listener is stopped when done
                            if (listener.IsListening)
                            {
                                listener.Stop();
                            }
                        }
                    });

                    // Wait for the task to complete or timeout
                    return await authTaskSource.Task;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error setting up HTTP listener: {ex.Message}[/]");
                    throw;
                }
                finally
                {
                    // Stop the listener when done
                    if (listener.IsListening)
                    {
                        try
                        {
                            listener.Stop();
                        }
                        catch
                        {
                            // Ignore errors during listener shutdown
                        }
                    }
                }
            }

            public async Task<AuthResult> AuthenticateWithBackend((string, string) authTuple)
            {
                var result = new AuthResult { Success = false };

                try
                {
                    AnsiConsole.MarkupLine("[grey]Authenticating with StellarPath API...[/]");

                    // Create the request
                    var request = new GoogleAuthRequest
                    {
                        IdToken = authTuple.Item1,
                        AuthToken = authTuple.Item2,
                    };

                    // Prepare the request manually to have more control
                    var content = new StringContent(
                        JsonSerializer.Serialize(request),
                        Encoding.UTF8,
                        "application/json");

                    // Send the request to the backend API
                    var response = await _httpClient.PostAsync("/api/auth/google", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        AnsiConsole.MarkupLine($"[red]API authentication failed: {response.StatusCode}[/]");
                        AnsiConsole.MarkupLine($"[red]Error details: {errorContent}[/]");
                        return result;
                    }

                    // Read the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the response
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, options);

                    if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
                    {
                        AnsiConsole.MarkupLine("[red]Invalid response from API[/]");
                        AnsiConsole.MarkupLine($"[grey]Response content: {responseContent}[/]");
                        return result;
                    }

                    result.Success = true;
                    result.Token = authResponse.Token;
                    result.User = authResponse.User;

                    return result;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error authenticating with backend: {ex.Message}[/]");
                    if (ex.InnerException != null)
                    {
                        AnsiConsole.MarkupLine($"[red]Inner exception: {ex.InnerException.Message}[/]");
                    }
                    return result;
                }
            }
        }
    }

