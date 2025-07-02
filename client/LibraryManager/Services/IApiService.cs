using LibraryManager.Models;

namespace LibraryManager.Services
{
    public interface IApiService
    {
        Task<List<ProgrammingLanguage>> GetLanguagesAsync();
        Task<List<Library>> GetLibrariesAsync();
        Task<Library> GetLibraryAsync(int id);
        Task<List<Library>> SearchLibrariesAsync(string query, string? language = null);
        Task<byte[]> DownloadLibraryFileAsync(string fileUrl);
    }
} 