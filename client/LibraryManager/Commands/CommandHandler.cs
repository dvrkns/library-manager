using LibraryManager.Models;
using LibraryManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace LibraryManager.Commands
{
    public static class CommandHandler
    {
        public static async Task<int> HandleSearch(SearchOptions options, ServiceProvider serviceProvider)
        {
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            AnsiConsole.MarkupLine($"[blue]Поиск библиотек по запросу: [bold]{options.Query}[/][/]");
            
            if (!string.IsNullOrEmpty(options.Language))
            {
                AnsiConsole.MarkupLine($"[blue]Фильтр по языку: [bold]{options.Language}[/][/]");
            }

            var results = await libraryService.SearchAsync(options.Query, options.Language);

            if (options.Version != null)
            {
                results = results.Where(l => l.Version.Contains(options.Version)).ToList();
            }

            if (!results.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Ничего не найдено[/]");
                return 0;
            }

            var table = new Table();
            table.AddColumn("Название");
            table.AddColumn("Версия");

            foreach (var library in results)
            {
                table.AddRow(
                    library.Name,
                    library.Version
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]Найдено библиотек: {results.Count}[/]");

            return 0;
        }

        public static async Task<int> HandleInstall(InstallOptions options, ServiceProvider serviceProvider)
        {
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            AnsiConsole.MarkupLine($"[blue]Установка библиотеки: [bold]{options.Name}[/][/]");
            
            if (!string.IsNullOrEmpty(options.Version))
            {
                AnsiConsole.MarkupLine($"[blue]Версия: [bold]{options.Version}[/][/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[blue]Будет установлена последняя версия[/]");
            }

            var result = await libraryService.InstallAsync(options.Name, options.Version);
            return result ? 0 : 1;
        }

        public static async Task<int> HandleUpdate(UpdateOptions options, ServiceProvider serviceProvider)
        {
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            if (string.IsNullOrEmpty(options.Name))
            {
                AnsiConsole.MarkupLine("[blue]Обновление всех установленных библиотек[/]");
                var result = await libraryService.UpdateAllAsync();
                return result ? 0 : 1;
            }
            else
            {
                AnsiConsole.MarkupLine($"[blue]Обновление библиотеки: [bold]{options.Name}[/][/]");
                var result = await libraryService.UpdateAsync(options.Name);
                return result ? 0 : 1;
            }
        }

        public static async Task<int> HandleUninstall(UninstallOptions options, ServiceProvider serviceProvider)
        {
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            AnsiConsole.MarkupLine($"[blue]Удаление библиотеки: [bold]{options.Name}[/][/]");
            var result = await libraryService.UninstallAsync(options.Name);
            return result ? 0 : 1;
        }

        public static async Task<int> HandleList(ListOptions options, ServiceProvider serviceProvider)
        {
            var libraryService = serviceProvider.GetRequiredService<ILibraryService>();

            AnsiConsole.MarkupLine("[blue]Список установленных библиотек:[/]");
            var libraries = await libraryService.ListInstalledAsync();

            if (!libraries.Any())
            {
                AnsiConsole.MarkupLine("[yellow]Нет установленных библиотек[/]");
                return 0;
            }

            var table = new Table();
            table.AddColumn("Название");
            table.AddColumn("Версия");

            foreach (var library in libraries)
            {
                table.AddRow(
                    library.Name,
                    library.Version
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[green]Всего установлено библиотек: {libraries.Count}[/]");

            return 0;
        }
    }
} 