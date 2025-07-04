<template>
  <div class="library-detail">
    <div v-if="loading" class="loading-container">
      <p>Загрузка информации о библиотеке...</p>
    </div>
    
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <router-link to="/" class="btn mt-2">Вернуться на главную</router-link>
    </div>
    
    <template v-else-if="library">
      <div class="card">
        <div class="d-flex justify-between align-center mb-3">
          <h1 class="library-title">{{ library.name }} <span class="version">{{ library.version }}</span></h1>
          <div>
            <router-link to="/" class="btn mr-1">Назад</router-link>
            <button class="btn danger" @click="onDeleteLibrary">Удалить</button>
          </div>
        </div>
        
        <div class="library-info">
          <div class="info-section mb-3">
            <h3 class="section-title mb-1">Основная информация</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Дата публикации:</span>
                <span class="value">{{ formatDate(library.published_date) }}</span>
              </div>
              
              <div class="info-item">
                <span class="label">Размер файла:</span>
                <span class="value">{{ formatFileSize(library.file_size) }}</span>
              </div>
            </div>
          </div>
          
          <div v-if="library.description" class="info-section mb-3">
            <h3 class="section-title mb-1">Описание</h3>
            <p class="description">{{ library.description }}</p>
          </div>
          
          <div class="info-section mb-3">
            <h3 class="section-title mb-1">Ссылки</h3>
            <div class="links">
              <a v-if="library.repository" :href="library.repository" target="_blank" class="link">Репозиторий</a>
              <a v-if="library.file" :href="library.file" target="_blank" class="link">Скачать файл</a>
            </div>
          </div>
          
          <div v-if="library.dependencies && library.dependencies.length > 0" class="info-section mb-3">
            <h3 class="section-title mb-1">Зависимости</h3>
            <ul class="dependencies-list">
              <li v-for="dep in library.dependencies" :key="dep.id" class="dependency-item">
                {{ dep.depends_on_name }} <span v-if="dep.version_constraint" class="version-constraint">({{ dep.version_constraint }})</span>
              </li>
            </ul>
          </div>
          
          <div v-if="library.dependents && library.dependents.length > 0" class="info-section">
            <h3 class="section-title mb-1">Используется в</h3>
            <ul class="dependencies-list">
              <li v-for="dep in library.dependents" :key="dep.id" class="dependency-item">
                {{ dep.name }} <span v-if="dep.version_constraint" class="version-constraint">({{ dep.version_constraint }})</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useLibrariesStore } from '@/stores/libraries';

const route = useRoute();
const router = useRouter();
const store = useLibrariesStore();

// Computed properties
const library = computed(() => store.library);
const loading = computed(() => store.loading);
const error = computed(() => store.error);

// Load library data when component mounts
onMounted(async () => {
  const id = route.params.id;
  await store.fetchLibraryById(id);
});

// Utility functions
const formatDate = (dateString) => {
  if (!dateString) return 'Не указано';
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ru-RU', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  }).format(date);
};

const formatFileSize = (bytes) => {
  if (!bytes) return 'Неизвестно';
  const units = ['байт', 'КБ', 'МБ', 'ГБ'];
  let size = bytes;
  let unitIndex = 0;
  
  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024;
    unitIndex++;
  }
  
  return `${size.toFixed(1)} ${units[unitIndex]}`;
};

// Delete library handler
const onDeleteLibrary = async () => {
  if (!confirm('Вы действительно хотите удалить эту библиотеку? Это действие нельзя отменить.')) {
    return;
  }
  
  try {
    await store.deleteLibrary(library.value.id);
    router.push('/');
  } catch (error) {
    console.error('Ошибка при удалении библиотеки:', error);
  }
};
</script>

<style scoped>
.library-detail {
  max-width: 800px;
  margin: 0 auto;
}

.library-title {
  font-size: 1.75rem;
  margin: 0;
  color: var(--primary-color);
}

.version {
  font-size: 1rem;
  background-color: #e6f2ff;
  color: var(--primary-color);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  margin-left: 0.5rem;
}

.library-info {
  margin-top: 1.5rem;
}

.section-title {
  font-size: 1.25rem;
  color: #333;
  margin-bottom: 0.5rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.label {
  font-size: 0.875rem;
  color: #666;
}

.value {
  font-weight: 500;
}

.badge {
  display: inline-block;
  padding: 0.25rem 0.5rem;
  background-color: #e6f2ff;
  color: var(--primary-color);
  border-radius: 4px;
  font-size: 0.875rem;
}

.description {
  line-height: 1.6;
  white-space: pre-line;
}

.links {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.link {
  display: inline-block;
  padding: 0.5rem 1rem;
  background-color: #f8f9fa;
  color: var(--primary-color);
  text-decoration: none;
  border-radius: 4px;
  border: 1px solid var(--border-color);
  transition: background-color 0.3s;
}

.link:hover {
  background-color: #e9ecef;
}

.dependencies-list {
  list-style: none;
  padding: 0;
}

.dependency-item {
  padding: 0.5rem 0;
  border-bottom: 1px solid var(--border-color);
}

.dependency-item:last-child {
  border-bottom: none;
}

.version-constraint {
  color: #6c757d;
  font-size: 0.9rem;
}

.loading-container,
.error-container {
  text-align: center;
  padding: 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.error-message {
  color: #dc3545;
}

.btn.danger {
  background-color: #dc3545;
  color: white;
}
.btn.danger:hover {
  background-color: #b52a37;
}
</style> 