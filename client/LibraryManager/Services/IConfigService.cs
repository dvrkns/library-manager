using LibraryManager.Models;

namespace LibraryManager.Services
{
    public interface IConfigService
    {
        Task LoadConfig();
        Task SaveConfig();
        Config CurrentConfig { get; }
    }
} 