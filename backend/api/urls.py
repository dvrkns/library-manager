from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import ProgrammingLanguageViewSet, LibraryViewSet

router = DefaultRouter()
router.register('languages', ProgrammingLanguageViewSet)
router.register('libraries', LibraryViewSet)

urlpatterns = [
    path('', include(router.urls)),
] 