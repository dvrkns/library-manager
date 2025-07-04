# Generated by Django 4.2.10 on 2025-07-02 09:50

from django.db import migrations, models
import django.db.models.deletion
import django.utils.timezone


class Migration(migrations.Migration):

    initial = True

    dependencies = [
    ]

    operations = [
        migrations.CreateModel(
            name='ProgrammingLanguage',
            fields=[
                ('id', models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('name', models.CharField(max_length=100, unique=True, verbose_name='Название')),
                ('slug', models.SlugField(max_length=100, unique=True, verbose_name='Slug')),
            ],
            options={
                'verbose_name': 'Язык программирования',
                'verbose_name_plural': 'Языки программирования',
                'ordering': ['name'],
            },
        ),
        migrations.CreateModel(
            name='Library',
            fields=[
                ('id', models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('name', models.CharField(max_length=255, verbose_name='Название')),
                ('version', models.CharField(max_length=50, verbose_name='Версия')),
                ('description', models.TextField(blank=True, verbose_name='Описание')),
                ('author', models.CharField(blank=True, max_length=255, verbose_name='Автор')),
                ('homepage', models.URLField(blank=True, verbose_name='Домашняя страница')),
                ('repository', models.URLField(blank=True, verbose_name='Репозиторий')),
                ('file', models.FileField(blank=True, null=True, upload_to='libraries/', verbose_name='Файл библиотеки')),
                ('file_size', models.PositiveIntegerField(default=0, verbose_name='Размер файла (байт)')),
                ('published_date', models.DateTimeField(default=django.utils.timezone.now, verbose_name='Дата публикации')),
                ('created_at', models.DateTimeField(auto_now_add=True, verbose_name='Дата создания')),
                ('updated_at', models.DateTimeField(auto_now=True, verbose_name='Дата обновления')),
                ('language', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, related_name='libraries', to='libraries.programminglanguage', verbose_name='Язык программирования')),
            ],
            options={
                'verbose_name': 'Библиотека',
                'verbose_name_plural': 'Библиотеки',
                'ordering': ['-published_date'],
                'unique_together': {('name', 'version')},
            },
        ),
        migrations.CreateModel(
            name='Dependency',
            fields=[
                ('id', models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name='ID')),
                ('version_constraint', models.CharField(blank=True, max_length=100, verbose_name='Ограничение версии')),
                ('depends_on', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, related_name='dependents', to='libraries.library', verbose_name='Зависит от')),
                ('library', models.ForeignKey(on_delete=django.db.models.deletion.CASCADE, related_name='dependencies', to='libraries.library', verbose_name='Библиотека')),
            ],
            options={
                'verbose_name': 'Зависимость',
                'verbose_name_plural': 'Зависимости',
                'unique_together': {('library', 'depends_on')},
            },
        ),
    ]
