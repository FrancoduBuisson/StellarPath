using System.Net.Http.Headers;
using Stellarpath.CLI.Models;

namespace Stellarpath.CLI.Core;

public class CommandContext
{
    public string JwtToken { get; set; }
    public UserInfo CurrentUser { get; set; }
    public bool IsLoggedIn => CurrentUser != null;
    public HttpClient HttpClient { get; }

    public CommandContext()
    {
        HttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://stellar-path-api-alb-357221888.af-south-1.elb.amazonaws.com")
        };
    }

    public void SetAuth(string token, UserInfo user)
    {
        JwtToken = token;
        CurrentUser = user;
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearAuth()
    {
        JwtToken = null;
        CurrentUser = null;
        HttpClient.DefaultRequestHeaders.Authorization = null;
    }
}
