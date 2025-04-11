using System.Text.Json;
using Stellarpath.CLI.Models;

namespace StellarPath.CLI.Utility
{
    public class SessionData
    {
        public string? JwtToken { get; set; }
        public UserInfo? CurrentUser { get; set; }
    }

    public class SessionManager
    {
        private static readonly string SessionDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".stellarpath");

        private static readonly string SessionFilePath = Path.Combine(SessionDirectory, "session.json");

        public static void SaveSession(string jwtToken, UserInfo user)
        {
            var sessionData = new SessionData
            {
                JwtToken = jwtToken,
                CurrentUser = user
            };

            // Ensure directory exists
            if (!Directory.Exists(SessionDirectory))
            {
                Directory.CreateDirectory(SessionDirectory);
            }

            // Serialize and save session data
            string jsonContent = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SessionFilePath, jsonContent);
        }

        public static SessionData? LoadSession()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                {
                    string jsonContent = File.ReadAllText(SessionFilePath);
                    return JsonSerializer.Deserialize<SessionData>(jsonContent);
                }
            }
            catch (Exception ex)
            {
                // Handle silently, return null which indicates no valid session
                Console.WriteLine($"Error loading session: {ex.Message}");
            }

            return null;
        }

        public static void ClearSession()
        {
            if (File.Exists(SessionFilePath))
            {
                try
                {
                    File.Delete(SessionFilePath);
                }
                catch
                {
                    // Silently fail if we can't delete the file
                }
            }
        }
    }
}