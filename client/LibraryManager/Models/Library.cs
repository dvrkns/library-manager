using Newtonsoft.Json;

namespace LibraryManager.Models
{
    public class Library
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("language")]
        public int LanguageId { get; set; }

        [JsonProperty("language_name")]
        public string? LanguageName { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("homepage")]
        public string? Homepage { get; set; }

        [JsonProperty("repository")]
        public string? Repository { get; set; }

        [JsonProperty("file")]
        public string? FileUrl { get; set; }
        
        [JsonProperty("download_url")]
        public string? DownloadUrl { get; set; }

        [JsonProperty("file_size")]
        public long FileSize { get; set; }

        [JsonProperty("published_date")]
        public DateTime PublishedDate { get; set; }

        [JsonProperty("dependencies")]
        public List<Dependency>? Dependencies { get; set; }

        [JsonProperty("dependents")]
        public List<Dependent>? Dependents { get; set; }

        public override string ToString()
        {
            return $"{Name} {Version}";
        }
    }

    public class Dependency
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("depends_on")]
        public int DependsOnId { get; set; }

        [JsonProperty("depends_on_name")]
        public string? DependsOnName { get; set; }

        [JsonProperty("version_constraint")]
        public string? VersionConstraint { get; set; }
    }

    public class Dependent
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("version_constraint")]
        public string? VersionConstraint { get; set; }
    }
} 