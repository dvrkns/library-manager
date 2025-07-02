from django.core.management.base import BaseCommand
from django.utils.text import slugify
from libraries.models import ProgrammingLanguage

class Command(BaseCommand):
    help = 'Заполняет базу данных языками программирования'

    def handle(self, *args, **options):
        languages = [
            'Python',
            'JavaScript',
            'TypeScript',
            'Java',
            'C#',
            'C++',
            'C',
            'PHP',
            'Ruby',
            'Go',
            'Rust',
            'Swift',
            'Kotlin',
            'Dart',
            'R'
        ]

        # Специальные slug для языков с конфликтами
        special_slugs = {
            'C#': 'csharp',
            'C++': 'cpp',
            'C': 'c-lang',
            'R': 'r-lang'
        }

        created_count = 0
        skipped_count = 0

        for lang_name in languages:
            # Используем специальный slug, если он определен
            if lang_name in special_slugs:
                slug = special_slugs[lang_name]
            else:
                slug = slugify(lang_name)
            
            # Проверяем, существует ли язык
            if not ProgrammingLanguage.objects.filter(slug=slug).exists():
                ProgrammingLanguage.objects.create(
                    name=lang_name,
                    slug=slug
                )
                created_count += 1
                self.stdout.write(self.style.SUCCESS(f'Создан язык: {lang_name} (slug: {slug})'))
            else:
                skipped_count += 1
                self.stdout.write(self.style.WARNING(f'Пропущен существующий язык: {lang_name} (slug: {slug})'))

        self.stdout.write(self.style.SUCCESS(
            f'Завершено! Создано: {created_count}, пропущено: {skipped_count}'
        )) 