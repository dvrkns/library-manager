import os
import django

# Настройка Django
os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'config.settings')
django.setup()

# Импортируем модель после настройки Django
from libraries.models import ProgrammingLanguage

# Удаляем языки с конфликтующими slug
ProgrammingLanguage.objects.all().delete()

print("Все языки программирования удалены. Запустите команду populate_languages для добавления языков с правильными slug.") 