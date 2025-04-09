using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using System.IO;


namespace CLI.Services
{
    internal class GoogleAuthService
    {
        private static HttpClient client = new HttpClient();

        public static async Task<string> StartLocalCallbackServer(string baseUrl)
        {
            var tokenFuture = new TaskCompletionSource<string>();
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://localhost:9090/auth/callback/");
            server.Start();

            server.BeginGetContext(async result =>
            {
                var context = server.EndGetContext(result);
                var response = context.Response;

                try
                {
                    if (context.Request.HttpMethod == "GET")
                    {
                        var query = context.Request.Url.Query;
                        var code = query.StartsWith("?code=") ? query.Substring(6) : null;

                        if (code != null)
                        {
                            try
                            {
                                var tokenRequest = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/auth/callback?code={code}");
                                var tokenResponse = await client.SendAsync(tokenRequest);
                                var token = await tokenResponse.Content.ReadAsStringAsync();

                                if (!string.IsNullOrEmpty(token))
                                {
                                    tokenFuture.SetResult(token);
                                    string responseMessage = "id token received. You can close this window.";
                                    response.StatusCode = (int)HttpStatusCode.OK;
                                    byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                                    response.ContentLength64 = buffer.Length;
                                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                else
                                {
                                    string responseMessage = "Failed to retrieve id token.";
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                                    response.ContentLength64 = buffer.Length;
                                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                string responseMessage = $"Error during token retrieval: {ex.Message}";
                                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                                response.ContentLength64 = buffer.Length;
                                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                            }
                        }
                        else
                        {
                            string responseMessage = "Failed to retrieve authorization code.";
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                            response.ContentLength64 = buffer.Length;
                            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        string responseMessage = "Method Not Allowed.";
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    string responseMessage = $"Error processing request: {ex.Message}";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseMessage);
                    response.ContentLength64 = buffer.Length;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                finally
                {
                    response.OutputStream.Close();
                    server.Stop();
                }
            }, null);

            return await tokenFuture.Task;
        }

        public static void DecodeAndStoreUserInfo(string idToken)
        {
            try
            {
                // Store token in the CurrentUser
                CurrentUser.SetToken(idToken);

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(idToken);
                var userInfo = Convert.FromBase64String(jwtToken.Payload.SerializeToJson());
                var userInfoJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(Encoding.UTF8.GetString(userInfo));

                // Extract user information
                var firstName = userInfoJson.ContainsKey("given_name") ? userInfoJson["given_name"].ToString() : "Unknown";
                CurrentUser.SetUserName(firstName);

                var userId = userInfoJson.ContainsKey("sub") ? userInfoJson["sub"].ToString() : null;
                CurrentUser.SetId(userId);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to decode id token: {ex.Message}");
            }
        }

        public static void ClearUserInfo()
        {
            CurrentUser.SetToken(null);
            CurrentUser.SetUserName(null);
            CurrentUser.SetId(null);
        }

        public static async Task DeleteUserInfo(string baseUrl)
        {
            try
            {
                CurrentUser.SetUserName(null);

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{baseUrl}/api/users/{CurrentUser.GetId()}")
                {
                    Headers =
                    {
                        { "Authorization", CurrentUser.GetToken() }
                    }
                };

                var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("User info deleted successfully from the database.");
                }
                else
                {
                    Console.Error.WriteLine($"Failed to delete user info from the database: {await response.Content.ReadAsStringAsync()}");
                }

                CurrentUser.SetId(null);
                CurrentUser.SetToken(null);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete user info: {ex.Message}");
            }
        }
    }

    public static class CurrentUser
    {
        public static string Token { get; private set; }
        public static string UserName { get; private set; }
        public static string Id { get; private set; }

        public static void SetToken(string token) => Token = token;
        public static void SetUserName(string userName) => UserName = userName;
        public static void SetId(string id) => Id = id;

        public static string GetToken() => Token;
        public static string GetUserName() => UserName;
        public static string GetId() => Id;
    }
}

