# Library Manager

Система для хранения и управления библиотеками программного обеспечения с двухкомпонентной архитектурой.

## Компоненты системы

1. **Веб-приложение** (Django + Vue.js)
   - Бэкенд: Django, PostgreSQL
   - Фронтенд: Vue.js
   - Функции: хранение, поиск, фильтрация программных библиотек

2. **Консольная утилита** (C#/.NET Core)
   - Функции: поиск, установка, обновление и удаление библиотек через CLI

## Структура проекта

```
library-manager/
├── backend/                # Django backend
│   ├── config/             # Django project settings
│   ├── api/                # API endpoints
│   ├── libraries/          # App for library management
│   ├── requirements.txt    # Python dependencies
│   └── manage.py           # Django management script
├── frontend/               # Vue.js frontend
│   ├── src/                # Vue source code
│   ├── public/             # Static assets
│   ├── package.json        # Node.js dependencies
│   └── vite.config.js      # Vue configuration
├── client/                 # C# console client
│   ├── LibraryManager/     # C# project
│   ├── LibraryManager.sln  # Solution file
│   └── README.md           # Client documentation
├── database/               # Database scripts
│   └── init.sql            # Initial schema
└── README.md               # Project documentation
```

## Требования

- Python 3.8+
- Node.js 16+
- .NET Core 6.0+
- PostgreSQL 13+

## Установка и запуск

### Бэкенд (Django)

```bash
cd backend
python -m venv venv
source venv/bin/activate  # На Windows: venv\Scripts\activate
pip install -r requirements.txt
python manage.py migrate
python manage.py runserver
```

### Фронтенд (Vue.js)

```bash
cd frontend
npm install
npm run dev
```

### Консольная утилита (C#)

```bash
cd client
dotnet build
dotnet run --project LibraryManager
```

## Основные функции

### Веб-приложение

- Отображение списка библиотек
- Поиск по названию, версии, дате публикации
- Фильтрация по языку
- Сортировка по дате публикации
- Подробное отображение метаданных библиотеки

### Консольная утилита

- `search`: поиск пакетов по ключевым словам и фильтрам (`-lang`, `-ver`)
- `install`: установка указанной или последней версии вместе с зависимостями
- `update`: обновление одной или всех локальных библиотек
- `uninstall`: удаление пакета из локального хранилища
- `list`: вывод списка установленных библиотек с версиями