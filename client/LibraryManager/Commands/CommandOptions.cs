using CommandLine;

namespace LibraryManager.Commands
{
    [Verb("search", HelpText = "Поиск библиотек по ключевым словам и фильтрам")]
    public class SearchOptions
    {
        [Value(0, MetaName = "query", Required = true, HelpText = "Поисковый запрос")]
        public string Query { get; set; } = string.Empty;

        [Option('l', "lang", Required = false, HelpText = "Фильтр по языку программирования")]
        public string? Language { get; set; }

        [Option('v', "ver", Required = false, HelpText = "Фильтр по версии")]
        public string? Version { get; set; }
    }

    [Verb("install", HelpText = "Установка библиотеки")]
    public class InstallOptions
    {
        [Value(0, MetaName = "name", Required = true, HelpText = "Название библиотеки")]
        public string Name { get; set; } = string.Empty;

        [Option('v', "version", Required = false, HelpText = "Версия библиотеки (по умолчанию - последняя)")]
        public string? Version { get; set; }
    }

    [Verb("update", HelpText = "Обновление библиотек")]
    public class UpdateOptions
    {
        [Value(0, MetaName = "name", Required = false, HelpText = "Название библиотеки (если не указано, обновляются все)")]
        public string? Name { get; set; }
    }

    [Verb("uninstall", HelpText = "Удаление библиотеки")]
    public class UninstallOptions
    {
        [Value(0, MetaName = "name", Required = true, HelpText = "Название библиотеки")]
        public string Name { get; set; } = string.Empty;
    }

    [Verb("list", HelpText = "Список установленных библиотек")]
    public class ListOptions
    {
    }
} 