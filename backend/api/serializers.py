from rest_framework import serializers
from libraries.models import ProgrammingLanguage, Library, Dependency


class ProgrammingLanguageSerializer(serializers.ModelSerializer):
    class Meta:
        model = ProgrammingLanguage
        fields = ('id', 'name', 'slug')


class DependencySerializer(serializers.ModelSerializer):
    depends_on_name = serializers.StringRelatedField(source='depends_on', read_only=True)
    
    class Meta:
        model = Dependency
        fields = ('id', 'depends_on', 'depends_on_name', 'version_constraint')


class LibrarySerializer(serializers.ModelSerializer):
    language_name = serializers.StringRelatedField(source='language', read_only=True)
    dependencies = DependencySerializer(many=True, read_only=True)
    
    class Meta:
        model = Library
        fields = (
            'id', 'name', 'version', 'description', 'language', 'language_name',
            'author', 'homepage', 'repository', 'file', 'file_size',
            'published_date', 'created_at', 'updated_at', 'dependencies'
        )


class LibraryDetailSerializer(LibrarySerializer):
    dependents = serializers.SerializerMethodField()
    
    class Meta(LibrarySerializer.Meta):
        fields = LibrarySerializer.Meta.fields + ('dependents',)
        
    def get_dependents(self, obj):
        return [
            {
                'id': dep.library.id,
                'name': str(dep.library),
                'version_constraint': dep.version_constraint
            }
            for dep in obj.dependents.all()
        ] 