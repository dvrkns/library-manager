# Library Manager CLI

Консольная утилита для управления библиотеками программного обеспечения.

## Требования

- .NET 6.0 SDK или выше

## Установка

```bash
# Клонирование репозитория
git clone https://github.com/your-username/library-manager.git
cd library-manager/client

# Сборка проекта
dotnet build

# Запуск
dotnet run --project LibraryManager/LibraryManager.csproj
```

## Использование

### Поиск библиотек

```bash
# Поиск по названию
dotnet run --project LibraryManager/LibraryManager.csproj search "название_библиотеки"

# Поиск с фильтром по языку программирования
dotnet run --project LibraryManager/LibraryManager.csproj search "название_библиотеки" -l python

# Поиск с фильтром по версии
dotnet run --project LibraryManager/LibraryManager.csproj search "название_библиотеки" -v 1.0
```

### Установка библиотек

```bash
# Установка последней версии
dotnet run --project LibraryManager/LibraryManager.csproj install "название_библиотеки"

# Установка конкретной версии
dotnet run --project LibraryManager/LibraryManager.csproj install "название_библиотеки" -v 1.0.0
```

### Обновление библиотек

```bash
# Обновление конкретной библиотеки до последней версии
dotnet run --project LibraryManager/LibraryManager.csproj update "название_библиотеки"

# Обновление всех установленных библиотек
dotnet run --project LibraryManager/LibraryManager.csproj update
```

### Удаление библиотек

```bash
dotnet run --project LibraryManager/LibraryManager.csproj uninstall "название_библиотеки"
```

### Список установленных библиотек

```bash
dotnet run --project LibraryManager/LibraryManager.csproj list
```

## Конфигурация

Настройки приложения хранятся в файле конфигурации, расположенном в `%APPDATA%\LibraryManager\config.json`. Вы можете изменить следующие параметры:

- `api_url`: URL API сервера (по умолчанию: `http://localhost:8000/api`)
- `local_repository_path`: Путь к локальному хранилищу библиотек (по умолчанию: `%USERPROFILE%\.library-manager`)
- `timeout_seconds`: Таймаут HTTP-запросов в секундах (по умолчанию: 30) 