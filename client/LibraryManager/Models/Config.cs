using Newtonsoft.Json;

namespace LibraryManager.Models
{
    public class Config
    {
        [JsonProperty("api_url")]
        public string ApiUrl { get; set; } = "http://localhost:8000/api";

        [JsonProperty("local_repository_path")]
        public string LocalRepositoryPath { get; set; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".library-manager"
        );

        [JsonProperty("timeout_seconds")]
        public int TimeoutSeconds { get; set; } = 30;
    }
} 