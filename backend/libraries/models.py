from django.db import models
from django.utils import timezone


class ProgrammingLanguage(models.Model):
    """Модель для языков программирования"""
    name = models.CharField(max_length=100, unique=True, verbose_name='Название')
    slug = models.SlugField(max_length=100, unique=True, verbose_name='Slug')
    
    class Meta:
        verbose_name = 'Язык программирования'
        verbose_name_plural = 'Языки программирования'
        ordering = ['name']
    
    def __str__(self):
        return self.name


class Library(models.Model):
    """Модель для программных библиотек"""
    name = models.CharField(max_length=255, verbose_name='Название')
    version = models.CharField(max_length=50, verbose_name='Версия')
    description = models.TextField(blank=True, verbose_name='Описание')
    language = models.ForeignKey(
        ProgrammingLanguage,
        on_delete=models.CASCADE,
        related_name='libraries',
        verbose_name='Язык программирования'
    )
    author = models.CharField(max_length=255, blank=True, verbose_name='Автор')
    homepage = models.URLField(blank=True, verbose_name='Домашняя страница')
    repository = models.URLField(blank=True, verbose_name='Репозиторий')
    file = models.FileField(upload_to='libraries/', blank=True, null=True, verbose_name='Файл библиотеки')
    file_size = models.PositiveIntegerField(default=0, verbose_name='Размер файла (байт)')
    published_date = models.DateTimeField(default=timezone.now, verbose_name='Дата публикации')
    created_at = models.DateTimeField(auto_now_add=True, verbose_name='Дата создания')
    updated_at = models.DateTimeField(auto_now=True, verbose_name='Дата обновления')
    
    class Meta:
        verbose_name = 'Библиотека'
        verbose_name_plural = 'Библиотеки'
        ordering = ['-published_date']
        unique_together = ('name', 'version')
    
    def __str__(self):
        return f"{self.name} {self.version}"
    
    def save(self, *args, **kwargs):
        # Вычисление размера файла при сохранении
        if self.file and hasattr(self.file, 'size'):
            self.file_size = self.file.size
        super().save(*args, **kwargs)


class Dependency(models.Model):
    """Модель для зависимостей между библиотеками"""
    library = models.ForeignKey(
        Library,
        on_delete=models.CASCADE,
        related_name='dependencies',
        verbose_name='Библиотека'
    )
    depends_on = models.ForeignKey(
        Library,
        on_delete=models.CASCADE,
        related_name='dependents',
        verbose_name='Зависит от'
    )
    version_constraint = models.CharField(max_length=100, blank=True, verbose_name='Ограничение версии')
    
    class Meta:
        verbose_name = 'Зависимость'
        verbose_name_plural = 'Зависимости'
        unique_together = ('library', 'depends_on')
    
    def __str__(self):
        return f"{self.library} -> {self.depends_on} {self.version_constraint}" 