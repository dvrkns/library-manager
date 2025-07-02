from django.contrib import admin
from .models import ProgrammingLanguage, Library, Dependency


@admin.register(ProgrammingLanguage)
class ProgrammingLanguageAdmin(admin.ModelAdmin):
    list_display = ('name', 'slug')
    prepopulated_fields = {'slug': ('name',)}
    search_fields = ('name',)


class DependencyInline(admin.TabularInline):
    model = Dependency
    fk_name = 'library'
    extra = 1
    autocomplete_fields = ('depends_on',)


@admin.register(Library)
class LibraryAdmin(admin.ModelAdmin):
    list_display = ('name', 'version', 'language', 'published_date', 'file_size')
    list_filter = ('language', 'published_date')
    search_fields = ('name', 'description', 'author')
    date_hierarchy = 'published_date'
    readonly_fields = ('file_size', 'created_at', 'updated_at')
    autocomplete_fields = ('language',)
    inlines = [DependencyInline]


@admin.register(Dependency)
class DependencyAdmin(admin.ModelAdmin):
    list_display = ('library', 'depends_on', 'version_constraint')
    list_filter = ('library__language',)
    search_fields = ('library__name', 'depends_on__name', 'version_constraint')
    autocomplete_fields = ('library', 'depends_on') 