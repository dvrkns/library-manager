using Newtonsoft.Json;

namespace LibraryManager.Models
{
    public class ProgrammingLanguage
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("slug")]
        public string Slug { get; set; } = string.Empty;

        public override string ToString()
        {
            return Name;
        }
    }
} 