from django.db.models import Q
from rest_framework import viewsets, filters
from rest_framework.decorators import action
from rest_framework.response import Response
from django_filters.rest_framework import DjangoFilterBackend

from libraries.models import ProgrammingLanguage, Library
from .serializers import (
    ProgrammingLanguageSerializer,
    LibrarySerializer,
    LibraryDetailSerializer
)


class ProgrammingLanguageViewSet(viewsets.ReadOnlyModelViewSet):
    """
    API для просмотра языков программирования
    """
    queryset = ProgrammingLanguage.objects.all()
    serializer_class = ProgrammingLanguageSerializer
    lookup_field = 'slug'


class LibraryViewSet(viewsets.ModelViewSet):
    """
    API для работы с библиотеками
    """
    queryset = Library.objects.all()
    serializer_class = LibrarySerializer
    filter_backends = [
        DjangoFilterBackend,
        filters.SearchFilter,
        filters.OrderingFilter
    ]
    filterset_fields = ['language']
    search_fields = ['name', 'version', 'description', 'author']
    ordering_fields = ['name', 'version', 'published_date', 'file_size']
    ordering = ['-published_date']
    
    def get_serializer_class(self):
        if self.action == 'retrieve':
            return LibraryDetailSerializer
        return LibrarySerializer
    
    @action(detail=False, methods=['get'])
    def search(self, request):
        """
        Поиск библиотек по названию, версии и дате публикации
        """
        query = request.query_params.get('q', '')
        if not query:
            return Response({'error': 'Необходимо указать параметр q для поиска'}, status=400)
            
        language = request.query_params.get('lang', None)
        
        queryset = self.get_queryset().filter(
            Q(name__icontains=query) | 
            Q(version__icontains=query) |
            Q(description__icontains=query)
        )
        
        if language:
            queryset = queryset.filter(language__slug=language)
            
        # Сортировка
        sort_by = request.query_params.get('sort', '-published_date')
        if sort_by not in self.ordering_fields and sort_by not in [f'-{field}' for field in self.ordering_fields]:
            sort_by = '-published_date'
            
        queryset = queryset.order_by(sort_by)
        
        page = self.paginate_queryset(queryset)
        if page is not None:
            serializer = self.get_serializer(page, many=True)
            return self.get_paginated_response(serializer.data)
            
        serializer = self.get_serializer(queryset, many=True)
        return Response(serializer.data) 