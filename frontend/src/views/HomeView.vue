<template>
  <div class="home">
    <div class="hero-section card mb-3">
      <h1 class="hero-title">Менеджер библиотек программного обеспечения</h1>
      <p class="hero-description">
        Наша система помогает управлять программными библиотеками и пакетами. Храните, систематизируйте 
        и находите нужные библиотеки для своих проектов. Вы можете искать пакеты по названию, 
        версии и другим параметрам, просматривать детальную информацию и загружать библиотеки.
      </p>
      <div class="features-list">
        <div class="feature-item">
          <i class="feature-icon">🔍</i>
          <h3>Быстрый поиск</h3>
          <p>Мгновенно находите нужные библиотеки по названию, версии или дате публикации</p>
        </div>
        <div class="feature-item">
          <i class="feature-icon">📦</i>
          <h3>Управление пакетами</h3>
          <p>Удобное добавление, редактирование и каталогизация программных библиотек</p>
        </div>
        <div class="feature-item">
          <i class="feature-icon">⬇️</i>
          <h3>Загрузка библиотек</h3>
          <p>Скачивайте нужные пакеты прямо из каталога одним кликом</p>
        </div>
      </div>
    </div>
    
    <SearchBar 
      @search-performed="searchPerformed = true"
      :showSortingOptions="searchPerformed && !loading && libraryGroups.length > 0"
    />
    
    <div v-if="notification" class="notification" :class="notification.type">
      {{ notification.message }}
      <button class="close-btn" @click="clearNotification">&times;</button>
    </div>
    
    <!-- Показывать раздел библиотек только после выполнения поиска -->
    <div v-if="searchPerformed" class="libraries-section">
      <div class="page-header mt-4">
        <h2 class="section-title">Библиотеки</h2>
      </div>
      
      <div v-if="loading" class="loading-container">
        <p>Загрузка библиотек...</p>
      </div>
      
      <div v-else-if="error" class="error-container">
        <p class="error-message">{{ error }}</p>
      </div>
      
      <div v-else-if="libraryGroups.length === 0" class="empty-container">
        <p>Библиотеки не найдены. Попробуйте изменить параметры поиска.</p>
      </div>
      
      <div v-else class="libraries-list">
        <div v-for="group in libraryGroups" :key="group.name" class="library-group">
          <LibraryListItem 
            :library="group.latest" 
            :versions="group.versions"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useLibrariesStore } from '@/stores/libraries';
import SearchBar from '@/components/SearchBar.vue';
import LibraryListItem from '@/components/LibraryListItem.vue';

const store = useLibrariesStore();
const route = useRoute();

// Флаг, указывающий, был ли выполнен поиск
const searchPerformed = ref(false);
const libraries = ref([]);

// Computed properties
const loading = computed(() => store.loading);
const error = computed(() => store.error);

// Группировка библиотек по имени для отображения версий
const libraryGroups = computed(() => {
  // Группируем библиотеки по имени
  const groups = {};
  libraries.value.forEach(lib => {
    if (!groups[lib.name]) {
      groups[lib.name] = { 
        name: lib.name, 
        versions: [], 
        latest: lib 
      };
    }
    
    groups[lib.name].versions.push(lib);
    
    // Определяем последнюю версию
    if (new Date(lib.published_date) > new Date(groups[lib.name].latest.published_date)) {
      groups[lib.name].latest = lib;
    }
  });
  
  // Преобразуем в массив и возвращаем
  return Object.values(groups);
});

// Notification system
const notification = ref(null);

// Check for query parameters to show notification
onMounted(() => {
  if (route.query.message) {
    notification.value = {
      message: route.query.message,
      type: route.query.type || 'info'
    };
    
    // Auto-hide notification after 5 seconds
    setTimeout(() => {
      clearNotification();
    }, 5000);
  }
  
  // Если в URL есть параметр поиска, считаем что поиск был выполнен
  if (route.query.q) {
    searchPerformed.value = true;
  }
});

const clearNotification = () => {
  notification.value = null;
};

// Отслеживаем изменения в хранилище и обновляем локальный массив библиотек
watch(() => store.libraries, (newLibraries) => {
  libraries.value = newLibraries;
});

// Load libraries only if there's a search query
onMounted(async () => {
  if (route.query.q) {
    store.setSearchQuery(route.query.q);
    store.setSortBy(route.query.sort || '-published_date');
    await store.searchLibraries();
    libraries.value = store.libraries;
    searchPerformed.value = true;
  }
});
</script>

<style scoped>
.hero-section {
  text-align: center;
  padding: 2.5rem;
  background: linear-gradient(135deg, #f5f7ff 0%, #e3e8ff 100%);
  border-radius: 12px;
  margin-bottom: 2rem;
}

.hero-title {
  font-size: 2.5rem;
  margin-bottom: 1rem;
  color: var(--primary-color);
}

.hero-description {
  font-size: 1.1rem;
  margin-bottom: 2rem;
  max-width: 800px;
  margin-left: auto;
  margin-right: auto;
  line-height: 1.6;
}

.features-list {
  display: flex;
  justify-content: space-around;
  gap: 2rem;
  margin-top: 2rem;
}

.feature-item {
  flex: 1;
  padding: 1.5rem;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);
  transition: transform 0.3s, box-shadow 0.3s;
}

.feature-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.1);
}

.feature-icon {
  font-size: 2rem;
  display: block;
  margin-bottom: 1rem;
}

.feature-item h3 {
  margin-bottom: 0.5rem;
  color: var(--primary-color);
}

.section-title {
  font-size: 1.75rem;
  margin-bottom: 0;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.libraries-section {
  animation: fadeIn 0.5s ease-in-out;
}

.libraries-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.3s;
  text-decoration: none;
}

.btn.primary {
  background-color: var(--primary-color);
  color: white;
}

.btn.primary:hover {
  background-color: #3050d8;
}

.notification {
  position: relative;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-radius: 4px;
  animation: fadeIn 0.3s ease-in-out;
}

.notification.success {
  background-color: #e8f5e9;
  color: #2e7d32;
}

.notification.error {
  background-color: #ffebee;
  color: #c62828;
}

.notification.info {
  background-color: #e3f2fd;
  color: #1565c0;
}

.close-btn {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: none;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  opacity: 0.7;
}

.close-btn:hover {
  opacity: 1;
}

.loading-container,
.error-container,
.empty-container {
  padding: 2rem;
  text-align: center;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.error-message {
  color: #c62828;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 768px) {
  .features-list {
    flex-direction: column;
    gap: 1rem;
  }
}
</style> 