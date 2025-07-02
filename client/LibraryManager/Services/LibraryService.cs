using LibraryManager.Models;
using Newtonsoft.Json;
using Spectre.Console;
using System.IO;

namespace LibraryManager.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly IApiService _apiService;
        private readonly IConfigService _configService;
        private readonly string _installedLibrariesFile;

        private List<Library> _installedLibraries = new List<Library>();

        public LibraryService(IApiService apiService, IConfigService configService)
        {
            _apiService = apiService;
            _configService = configService;
            _installedLibrariesFile = Path.Combine(_configService.CurrentConfig.LocalRepositoryPath, "installed.json");
            
            // Загружаем список установленных библиотек
            LoadInstalledLibraries().Wait();
        }

        public async Task<List<Library>> SearchAsync(string query, string? language = null)
        {
            return await _apiService.SearchLibrariesAsync(query, language);
        }

        public async Task<bool> InstallAsync(string libraryName, string? version = null)
        {
            try
            {
                // Поиск библиотеки
                var searchResults = await _apiService.SearchLibrariesAsync(libraryName);
                var libraryToInstall = searchResults
                    .Where(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.PublishedDate)
                    .ToList();

                if (!libraryToInstall.Any())
                {
                    AnsiConsole.MarkupLine($"[red]Библиотека '{libraryName}' не найдена[/]");
                    return false;
                }

                // Выбор версии
                Library selectedLibrary;
                if (version != null)
                {
                    selectedLibrary = libraryToInstall.FirstOrDefault(l => l.Version == version) 
                        ?? throw new Exception($"Версия {version} библиотеки {libraryName} не найдена");
                }
                else
                {
                    selectedLibrary = libraryToInstall.First();
                }

                // Проверка, установлена ли библиотека уже
                if (IsInstalled(selectedLibrary.Name, selectedLibrary.Version))
                {
                    AnsiConsole.MarkupLine($"[yellow]Библиотека {selectedLibrary.Name} {selectedLibrary.Version} уже установлена[/]");
                    return true;
                }

                // Скачивание файла библиотеки
                if (string.IsNullOrEmpty(selectedLibrary.FileUrl))
                {
                    throw new Exception($"Для библиотеки {selectedLibrary.Name} {selectedLibrary.Version} не найден файл для скачивания");
                }

                var fileData = await _apiService.DownloadLibraryFileAsync(selectedLibrary.FileUrl);
                
                // Создание директории для библиотеки
                var libraryDir = Path.Combine(_configService.CurrentConfig.LocalRepositoryPath, selectedLibrary.Name, selectedLibrary.Version);
                if (!Directory.Exists(libraryDir))
                {
                    Directory.CreateDirectory(libraryDir);
                }

                // Сохранение файла
                var fileName = Path.GetFileName(selectedLibrary.FileUrl) ?? $"{selectedLibrary.Name}.dll";
                var filePath = Path.Combine(libraryDir, fileName);
                await File.WriteAllBytesAsync(filePath, fileData);

                // Сохранение метаданных
                var metadataPath = Path.Combine(libraryDir, "metadata.json");
                var metadata = JsonConvert.SerializeObject(selectedLibrary, Formatting.Indented);
                await File.WriteAllTextAsync(metadataPath, metadata);

                // Добавление в список установленных
                _installedLibraries.Add(selectedLibrary);
                await SaveInstalledLibraries();

                AnsiConsole.MarkupLine($"[green]Библиотека {selectedLibrary.Name} {selectedLibrary.Version} успешно установлена[/]");
                
                // Установка зависимостей
                if (selectedLibrary.Dependencies != null && selectedLibrary.Dependencies.Any())
                {
                    AnsiConsole.MarkupLine($"[blue]Установка зависимостей для {selectedLibrary.Name}...[/]");
                    
                    foreach (var dependency in selectedLibrary.Dependencies)
                    {
                        if (dependency.DependsOnName != null)
                        {
                            await InstallAsync(dependency.DependsOnName, dependency.VersionConstraint);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при установке библиотеки: {ex.Message}[/]");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(string libraryName)
        {
            try
            {
                // Проверяем, установлена ли библиотека
                var installed = _installedLibraries.FirstOrDefault(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
                if (installed == null)
                {
                    AnsiConsole.MarkupLine($"[yellow]Библиотека '{libraryName}' не установлена[/]");
                    return false;
                }

                // Ищем последнюю версию
                var searchResults = await _apiService.SearchLibrariesAsync(libraryName);
                var latestLibrary = searchResults
                    .Where(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.PublishedDate)
                    .FirstOrDefault();

                if (latestLibrary == null)
                {
                    AnsiConsole.MarkupLine($"[red]Не удалось найти библиотеку '{libraryName}' для обновления[/]");
                    return false;
                }

                // Проверяем, требуется ли обновление
                if (installed.Version == latestLibrary.Version)
                {
                    AnsiConsole.MarkupLine($"[green]Библиотека {libraryName} уже обновлена до последней версии ({latestLibrary.Version})[/]");
                    return true;
                }

                // Устанавливаем новую версию
                var result = await InstallAsync(libraryName);
                if (result)
                {
                    AnsiConsole.MarkupLine($"[green]Библиотека {libraryName} обновлена с версии {installed.Version} до {latestLibrary.Version}[/]");
                }

                return result;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при обновлении библиотеки: {ex.Message}[/]");
                return false;
            }
        }

        public async Task<bool> UpdateAllAsync()
        {
            try
            {
                if (!_installedLibraries.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]Нет установленных библиотек для обновления[/]");
                    return true;
                }

                var succeeded = true;
                foreach (var library in _installedLibraries.ToList())
                {
                    var result = await UpdateAsync(library.Name);
                    if (!result)
                    {
                        succeeded = false;
                    }
                }

                return succeeded;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при обновлении библиотек: {ex.Message}[/]");
                return false;
            }
        }

        public async Task<bool> UninstallAsync(string libraryName)
        {
            try
            {
                // Проверяем, установлена ли библиотека
                var installed = _installedLibraries.Where(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase)).ToList();
                if (!installed.Any())
                {
                    AnsiConsole.MarkupLine($"[yellow]Библиотека '{libraryName}' не установлена[/]");
                    return false;
                }

                // Удаляем все версии
                var libraryDir = Path.Combine(_configService.CurrentConfig.LocalRepositoryPath, libraryName);
                if (Directory.Exists(libraryDir))
                {
                    Directory.Delete(libraryDir, true);
                }

                // Обновляем список установленных библиотек
                _installedLibraries.RemoveAll(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
                await SaveInstalledLibraries();

                AnsiConsole.MarkupLine($"[green]Библиотека '{libraryName}' успешно удалена[/]");
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при удалении библиотеки: {ex.Message}[/]");
                return false;
            }
        }

        public async Task<List<Library>> ListInstalledAsync()
        {
            await LoadInstalledLibraries();
            return _installedLibraries;
        }

        public bool IsInstalled(string libraryName, string? version = null)
        {
            if (version == null)
            {
                return _installedLibraries.Any(l => l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
            }

            return _installedLibraries.Any(l => 
                l.Name.Equals(libraryName, StringComparison.OrdinalIgnoreCase) && 
                l.Version == version);
        }

        private async Task LoadInstalledLibraries()
        {
            try
            {
                if (File.Exists(_installedLibrariesFile))
                {
                    var json = await File.ReadAllTextAsync(_installedLibrariesFile);
                    var libraries = JsonConvert.DeserializeObject<List<Library>>(json);
                    
                    if (libraries != null)
                    {
                        _installedLibraries = libraries;
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при загрузке списка установленных библиотек: {ex.Message}[/]");
            }
        }

        private async Task SaveInstalledLibraries()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_installedLibraries, Formatting.Indented);
                await File.WriteAllTextAsync(_installedLibrariesFile, json);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при сохранении списка установленных библиотек: {ex.Message}[/]");
            }
        }
    }
} 