using CommandLine;
using LibraryManager.Commands;
using LibraryManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace LibraryManager
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Регистрация зависимостей
                var serviceProvider = new ServiceCollection()
                    .AddSingleton<IApiService, ApiService>()
                    .AddSingleton<ILibraryService, LibraryService>()
                    .AddSingleton<IConfigService, ConfigService>()
                    .BuildServiceProvider();

                // Конфигурация приложения
                var configService = serviceProvider.GetRequiredService<IConfigService>();
                await configService.LoadConfig();

                // Парсинг командной строки
                return await Parser.Default.ParseArguments<
                    SearchOptions,
                    InstallOptions,
                    UpdateOptions,
                    UninstallOptions,
                    ListOptions
                >(args)
                    .MapResult(
                        (SearchOptions opts) => CommandHandler.HandleSearch(opts, serviceProvider),
                        (InstallOptions opts) => CommandHandler.HandleInstall(opts, serviceProvider),
                        (UpdateOptions opts) => CommandHandler.HandleUpdate(opts, serviceProvider),
                        (UninstallOptions opts) => CommandHandler.HandleUninstall(opts, serviceProvider),
                        (ListOptions opts) => CommandHandler.HandleList(opts, serviceProvider),
                        errs => Task.FromResult(1)
                    );
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Ошибка: {ex.Message}[/]");
                return 1;
            }
        }
    }
} 