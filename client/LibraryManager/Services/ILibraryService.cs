using LibraryManager.Models;

namespace LibraryManager.Services
{
    public interface ILibraryService
    {
        Task<List<Library>> SearchAsync(string query, string? language = null);
        Task<bool> InstallAsync(string libraryName, string? version = null);
        Task<bool> UpdateAsync(string libraryName);
        Task<bool> UpdateAllAsync();
        Task<bool> UninstallAsync(string libraryName);
        Task<List<Library>> ListInstalledAsync();
        bool IsInstalled(string libraryName, string? version = null);
    }
} 