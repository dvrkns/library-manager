using LibraryManager.Models;
using Newtonsoft.Json;
using Spectre.Console;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;

namespace LibraryManager.Services
{
    public class LibraryService : ILibraryService
    {
        private readonly IApiService _apiService;
        private readonly IConfigService _configService;
        private readonly string _installedLibrariesFile;
        private readonly string _pythonVenvPath;
        private readonly string _sitePackagesPath;

        private List<Library> _installedLibraries = new List<Library>();

        public LibraryService(IApiService apiService, IConfigService configService)
        {
            _apiService = apiService;
            _configService = configService;
            _installedLibrariesFile = Path.Combine(_configService.CurrentConfig.LocalRepositoryPath, "installed.json");
            _pythonVenvPath = Path.Combine(_configService.CurrentConfig.LocalRepositoryPath, "python_libraries");
            _sitePackagesPath = Path.Combine(_pythonVenvPath, "packages");
            
            // Загружаем список установленных библиотек
            LoadInstalledLibraries().Wait();
            
            // Создаем директории для библиотек, если они не существуют
            EnsureLibraryDirectoriesExist().Wait();
        }

        public async Task<List<Library>> SearchAsync(string query, string? language = null)
        {
            return await _apiService.SearchLibrariesAsync(query, language);
        }

        public async Task<bool> InstallAsync(string libraryName, string? version = null)
        {
            try
            {
                // Проверяем, является ли это тестовым локальным пакетом
                if (libraryName.Equals("test_package", StringComparison.OrdinalIgnoreCase))
                {
                    return await TestInstallLocalPackage();
                }
                
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

                // Проверяем наличие файла или URL для скачивания
                string tempFilePath = null;
                bool needCleanup = false;

                if (!string.IsNullOrEmpty(selectedLibrary.FileUrl))
                {
                    // Скачивание файла библиотеки из хранилища сервера
                    var fileData = await _apiService.DownloadLibraryFileAsync(selectedLibrary.FileUrl);
                    
                    // Создание временного файла
                    tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(selectedLibrary.FileUrl) ?? $"{selectedLibrary.Name}.whl");
                    await File.WriteAllBytesAsync(tempFilePath, fileData);
                    needCleanup = true;
                }
                else if (!string.IsNullOrEmpty(selectedLibrary.DownloadUrl))
                {
                    // Скачиваем файл по внешней ссылке
                    tempFilePath = Path.Combine(Path.GetTempPath(), $"{selectedLibrary.Name}-{selectedLibrary.Version}.whl");
                    
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync(selectedLibrary.DownloadUrl);
                        response.EnsureSuccessStatusCode();
                        
                        using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                    
                    needCleanup = true;
                }
                else
                {
                    throw new Exception($"Для библиотеки {selectedLibrary.Name} {selectedLibrary.Version} не найден файл для скачивания");
                }

                // Установка библиотеки
                AnsiConsole.MarkupLine($"[blue]Установка библиотеки {selectedLibrary.Name} {selectedLibrary.Version}...[/]");
                
                bool installResult = await InstallPackageDirectly(tempFilePath, selectedLibrary.Name);
                
                // Очистка временного файла
                if (needCleanup && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                
                if (!installResult)
                {
                    throw new Exception($"Не удалось установить библиотеку {selectedLibrary.Name}");
                }

                // Добавление в список установленных
                _installedLibraries.Add(selectedLibrary);
                await SaveInstalledLibraries();

                AnsiConsole.MarkupLine($"[green]Библиотека {selectedLibrary.Name} {selectedLibrary.Version} успешно установлена[/]");
                
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

                // Удаляем библиотеку
                AnsiConsole.MarkupLine($"[blue]Удаление библиотеки {libraryName}...[/]");
                
                // Удаляем директорию библиотеки
                var libraryDir = Path.Combine(_sitePackagesPath, libraryName.ToLower());
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
        
        private async Task<bool> TestInstallLocalPackage()
        {
            try
            {
                AnsiConsole.MarkupLine("[blue]Тестирование установки локального пакета...[/]");
                
                string packageName = "test_package";
                string packagePath = Path.Combine(Directory.GetCurrentDirectory(), "test_package.zip");
                
                if (!File.Exists(packagePath))
                {
                    throw new Exception($"Файл {packagePath} не найден");
                }
                
                // Проверка, установлена ли библиотека уже
                if (IsInstalled(packageName))
                {
                    // Удаляем существующую установку
                    await UninstallAsync(packageName);
                }
                
                // Устанавливаем пакет
                bool installResult = await InstallPackageDirectly(packagePath, packageName);
                
                if (!installResult)
                {
                    throw new Exception($"Не удалось установить пакет {packageName}");
                }
                
                // Добавляем в список установленных библиотек
                var library = new Library
                {
                    Id = _installedLibraries.Count + 1,
                    Name = packageName,
                    Version = "1.0.0",
                    Description = "Тестовый локальный пакет",
                    LanguageId = 1, // Python
                    LanguageName = "Python",
                    PublishedDate = DateTime.Now
                };
                
                _installedLibraries.Add(library);
                await SaveInstalledLibraries();
                
                AnsiConsole.MarkupLine($"[green]Тестовый пакет {packageName} успешно установлен[/]");
                
                // Показываем путь к установленному пакету
                string installPath = Path.Combine(_sitePackagesPath, packageName.ToLower());
                AnsiConsole.MarkupLine($"[blue]Путь установки: {installPath}[/]");
                
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при тестировании установки: {ex.Message}[/]");
                return false;
            }
        }
        
        private async Task EnsureLibraryDirectoriesExist()
        {
            try
            {
                // Создаем основную директорию для библиотек
                if (!Directory.Exists(_configService.CurrentConfig.LocalRepositoryPath))
                {
                    Directory.CreateDirectory(_configService.CurrentConfig.LocalRepositoryPath);
                }
                
                // Создаем директорию для Python библиотек
                if (!Directory.Exists(_pythonVenvPath))
                {
                    Directory.CreateDirectory(_pythonVenvPath);
                }
                
                // Создаем директорию для пакетов
                if (!Directory.Exists(_sitePackagesPath))
                {
                    Directory.CreateDirectory(_sitePackagesPath);
                }
                
                // Создаем файл __init__.py для обозначения директории как Python пакета
                string initPyPath = Path.Combine(_sitePackagesPath, "__init__.py");
                if (!File.Exists(initPyPath))
                {
                    await File.WriteAllTextAsync(initPyPath, "# Автоматически созданный файл для библиотек Python\n");
                }
                
                AnsiConsole.MarkupLine("[green]Директории для библиотек успешно созданы[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при создании директорий для библиотек: {ex.Message}[/]");
                throw;
            }
        }
        
        private async Task<bool> InstallPackageDirectly(string packagePath, string libraryName)
        {
            try
            {
                AnsiConsole.MarkupLine($"[blue]Установка пакета из файла: {packagePath}[/]");
                
                // Проверяем расширение файла
                string extension = Path.GetExtension(packagePath).ToLower();
                
                // Путь для установки библиотеки
                string libraryDir = Path.Combine(_sitePackagesPath, libraryName.ToLower());
                
                // Создаем временную директорию для распаковки
                string tempExtractDir = Path.Combine(Path.GetTempPath(), $"extract_{Guid.NewGuid()}");
                Directory.CreateDirectory(tempExtractDir);
                
                try
                {
                    // Распаковываем файл в зависимости от его типа
                    if (extension == ".whl" || extension == ".zip")
                    {
                        // Распаковываем wheel или zip файл
                        AnsiConsole.MarkupLine($"[blue]Распаковка {extension} файла...[/]");
                        ZipFile.ExtractToDirectory(packagePath, tempExtractDir);
                    }
                    else if (extension == ".tar" || extension == ".gz" || extension == ".tgz")
                    {
                        // Для tar/gz файлов нужно использовать внешний инструмент
                        // В данной реализации мы будем поддерживать только .whl и .zip
                        throw new Exception($"Формат файла {extension} в данный момент не поддерживается для автономной установки");
                    }
                    else
                    {
                        // Если это просто .py файл, копируем его напрямую
                        if (extension == ".py")
                        {
                            if (!Directory.Exists(libraryDir))
                            {
                                Directory.CreateDirectory(libraryDir);
                            }
                            
                            string targetFile = Path.Combine(libraryDir, Path.GetFileName(packagePath));
                            File.Copy(packagePath, targetFile, true);
                            
                            // Создаем __init__.py если его нет
                            string initPyPath = Path.Combine(libraryDir, "__init__.py");
                            if (!File.Exists(initPyPath))
                            {
                                await File.WriteAllTextAsync(initPyPath, "# Автоматически созданный файл\n");
                            }
                            
                            AnsiConsole.MarkupLine($"[green]Файл {Path.GetFileName(packagePath)} успешно установлен[/]");
                            return true;
                        }
                        else
                        {
                            throw new Exception($"Неподдерживаемый формат файла: {extension}");
                        }
                    }
                    
                    // Находим директорию с исходным кодом библиотеки
                    string sourceDir = FindSourceDirectory(tempExtractDir, libraryName);
                    
                    // Создаем директорию для библиотеки, если она не существует
                    if (Directory.Exists(libraryDir))
                    {
                        Directory.Delete(libraryDir, true);
                    }
                    
                    // Копируем файлы библиотеки
                    CopyDirectory(sourceDir, libraryDir);
                    
                    // Создаем файл метаданных для библиотеки
                    await CreateMetadataFile(libraryDir, libraryName);
                    
                    AnsiConsole.MarkupLine($"[green]Библиотека успешно установлена в {libraryDir}[/]");
                    return true;
                }
                finally
                {
                    // Очистка временной директории
                    if (Directory.Exists(tempExtractDir))
                    {
                        Directory.Delete(tempExtractDir, true);
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при установке пакета: {ex.Message}[/]");
                return false;
            }
        }
        
        private string FindSourceDirectory(string extractDir, string libraryName)
        {
            // Ищем директорию с именем библиотеки
            var possibleDirs = Directory.GetDirectories(extractDir, $"{libraryName}*", SearchOption.AllDirectories);
            if (possibleDirs.Length > 0)
            {
                return possibleDirs[0];
            }
            
            // Ищем setup.py
            var setupFiles = Directory.GetFiles(extractDir, "setup.py", SearchOption.AllDirectories);
            if (setupFiles.Length > 0)
            {
                return Path.GetDirectoryName(setupFiles[0]) ?? extractDir;
            }
            
            // Ищем .py файлы
            var pyFiles = Directory.GetFiles(extractDir, "*.py", SearchOption.TopDirectoryOnly);
            if (pyFiles.Length > 0)
            {
                return extractDir;
            }
            
            // Если ничего не нашли, возвращаем корневую директорию
            return extractDir;
        }
        
        private async Task CreateMetadataFile(string libraryDir, string libraryName)
        {
            // Создаем файл метаданных
            string metadataPath = Path.Combine(libraryDir, "__metadata__.py");
            
            string metadata = $@"# Метаданные библиотеки {libraryName}
# Автоматически созданы LibraryManager

name = '{libraryName}'
version = 'local'
install_date = '{DateTime.Now}'
";
            
            await File.WriteAllTextAsync(metadataPath, metadata);
        }
        
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            // Создаем целевую директорию, если она не существует
            Directory.CreateDirectory(targetDir);
            
            // Копируем файлы
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }
            
            // Копируем поддиректории
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectory(directory, targetSubDir);
            }
        }

        private async Task LoadInstalledLibraries()
        {
            try
            {
                if (File.Exists(_installedLibrariesFile))
                {
                    var json = await File.ReadAllTextAsync(_installedLibrariesFile);
                    _installedLibraries = JsonConvert.DeserializeObject<List<Library>>(json) ?? new List<Library>();
                }
                else
                {
                    _installedLibraries = new List<Library>();
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка при загрузке списка установленных библиотек: {ex.Message}[/]");
                _installedLibraries = new List<Library>();
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