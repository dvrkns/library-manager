using LibraryManager.Models;
using Newtonsoft.Json;
using Spectre.Console;
using System.Net.Http.Headers;
using System.Text;

namespace LibraryManager.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfigService _configService;

        public ApiService(IConfigService configService)
        {
            _configService = configService;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configService.CurrentConfig.ApiUrl),
                Timeout = TimeSpan.FromSeconds(_configService.CurrentConfig.TimeoutSeconds)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<List<ProgrammingLanguage>> GetLanguagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/languages/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<ProgrammingLanguage>>(content);

                return result?.Results ?? new List<ProgrammingLanguage>();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при получении списка языков: {ex.Message}[/]");
                return new List<ProgrammingLanguage>();
            }
        }

        public async Task<List<Library>> GetLibrariesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/libraries/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<Library>>(content);

                return result?.Results ?? new List<Library>();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при получении списка библиотек: {ex.Message}[/]");
                return new List<Library>();
            }
        }

        public async Task<Library> GetLibraryAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/libraries/{id}/");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var library = JsonConvert.DeserializeObject<Library>(content);

                return library ?? throw new Exception($"Не удалось получить библиотеку с ID {id}");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при получении библиотеки: {ex.Message}[/]");
                throw;
            }
        }

        public async Task<List<Library>> SearchLibrariesAsync(string query, string? language = null)
        {
            try
            {
                var requestUrl = $"/libraries/search/?q={Uri.EscapeDataString(query)}";
                
                if (!string.IsNullOrEmpty(language))
                {
                    requestUrl += $"&lang={Uri.EscapeDataString(language)}";
                }

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ApiResponse<Library>>(content);

                return result?.Results ?? new List<Library>();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при поиске библиотек: {ex.Message}[/]");
                return new List<Library>();
            }
        }

        public async Task<byte[]> DownloadLibraryFileAsync(string fileUrl)
        {
            try
            {
                // Если URL относительный, добавляем базовый URL
                var fullUrl = fileUrl.StartsWith("http") ? fileUrl : $"{_configService.CurrentConfig.ApiUrl}{fileUrl}";
                
                var response = await _httpClient.GetAsync(fullUrl);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при скачивании файла библиотеки: {ex.Message}[/]");
                throw;
            }
        }
    }
} 