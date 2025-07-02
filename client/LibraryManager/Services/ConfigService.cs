using LibraryManager.Models;
using Newtonsoft.Json;
using Spectre.Console;

namespace LibraryManager.Services
{
    public class ConfigService : IConfigService
    {
        private readonly string _configPath;
        
        public Config CurrentConfig { get; private set; } = new Config();

        public ConfigService()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LibraryManager"
            );
            
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }
            
            _configPath = Path.Combine(appDataFolder, "config.json");
        }

        public async Task LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    var json = await File.ReadAllTextAsync(_configPath);
                    var config = JsonConvert.DeserializeObject<Config>(json);
                    
                    if (config != null)
                    {
                        CurrentConfig = config;
                    }
                }
                else
                {
                    // Создаем конфигурацию по умолчанию
                    CurrentConfig = new Config();
                    await SaveConfig();
                }
                
                // Создаем локальный репозиторий, если он не существует
                if (!Directory.Exists(CurrentConfig.LocalRepositoryPath))
                {
                    Directory.CreateDirectory(CurrentConfig.LocalRepositoryPath);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при загрузке конфигурации: {ex.Message}[/]");
                CurrentConfig = new Config();
            }
        }

        public async Task SaveConfig()
        {
            try
            {
                var json = JsonConvert.SerializeObject(CurrentConfig, Formatting.Indented);
                await File.WriteAllTextAsync(_configPath, json);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при сохранении конфигурации: {ex.Message}[/]");
            }
        }
    }
} 