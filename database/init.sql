-- Создание базы данных
CREATE DATABASE librarymanager;

-- Подключение к созданной базе данных
\c librarymanager;

-- Создание необходимых расширений
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Инициализация данных языков программирования
INSERT INTO libraries_programminglanguage (name, slug) VALUES
('Python', 'python'),
('JavaScript', 'javascript'),
('TypeScript', 'typescript'),
('C#', 'csharp'),
('Java', 'java'),
('Go', 'go'),
('Rust', 'rust'),
('PHP', 'php'),
('Ruby', 'ruby'),
('Swift', 'swift');

-- Пользователь для админки
INSERT INTO auth_user (
    password, 
    is_superuser, 
    username, 
    first_name, 
    last_name, 
    email, 
    is_staff, 
    is_active, 
    date_joined
) VALUES (
    'pbkdf2_sha256$600000$J6h3mK2NMmCxnJtzosBiJV$1X/PgqJ7h33EfbVWJ+YXzZtQpJ6gYs6wsM0RuV4+YWA=', -- 'admin' хешированный
    TRUE,
    'admin',
    '',
    '',
    'admin@example.com',
    TRUE,
    TRUE,
    NOW()
); 