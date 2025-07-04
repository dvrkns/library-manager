<template>
  <div class="all-libraries-view">
    <div class="page-header">
      <h1 class="page-title">Все библиотеки</h1>
      <div class="header-actions">
        <router-link to="/add-library" class="btn-action btn-primary">
          Добавить библиотеку
        </router-link>
      </div>
    </div>
    
    <div class="filters-panel card mb-3">
      <div class="filters-form">
        <div class="filters-row">
          <div class="filter-group">
            <label for="filter-query">Поиск:</label>
            <input 
              type="text" 
              id="filter-query" 
              placeholder="Введите название, версию или описание..." 
              v-model="filterQuery"
              @input="applyFilters"
            />
          </div>
          
          <div class="filter-group">
            <label for="filter-sort">Сортировка:</label>
            <select id="filter-sort" v-model="sortBy" @change="applyFilters">
              <option value="-published_date">Сначала новые</option>
              <option value="published_date">Сначала старые</option>
              <option value="name">По названию (А-Я)</option>
              <option value="-name">По названию (Я-А)</option>
              <option value="-file_size">По размеру (большие)</option>
              <option value="file_size">По размеру (малые)</option>
            </select>
          </div>
        </div>
      </div>
    </div>
    
    <div v-if="notification" class="notification" :class="notification.type">
      {{ notification.message }}
      <button class="close-btn" @click="clearNotification">&times;</button>
    </div>
    
    <div v-if="loading" class="loading-container">
      <p>Загрузка библиотек...</p>
    </div>
    
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <button class="btn-action btn-primary" @click="fetchLibraries">Повторить попытку</button>
    </div>
    
    <div v-else-if="libraryGroups.length === 0" class="empty-container">
      <p>Библиотеки не найдены.</p>
    </div>
    
    <div v-else class="libraries-list">
      <div v-for="group in libraryGroups" :key="group.name" class="library-group">
        <LibraryListItem 
          :library="group.latest" 
          :versions="group.versions"
          :showEdit="true"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useLibrariesStore } from '@/stores/libraries';
import LibraryListItem from '@/components/LibraryListItem.vue';

const store = useLibrariesStore();
const route = useRoute();

const loading = ref(false);
const error = ref(null);
const libraries = ref([]);
const filterQuery = ref('');
const sortBy = ref('-published_date');

// Группировка библиотек по имени для выбора версий
const libraryGroups = computed(() => {
  // Сначала фильтруем библиотеки
  const filtered = libraries.value.filter(lib => {
    // Фильтр по поисковой строке
    return !filterQuery.value || 
      lib.name.toLowerCase().includes(filterQuery.value.toLowerCase()) || 
      lib.version.toLowerCase().includes(filterQuery.value.toLowerCase());
  });
  
  // Группируем по имени библиотеки
  const groups = {};
  filtered.forEach(lib => {
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
  
  // Преобразуем в массив и сортируем
  return Object.values(groups).sort((a, b) => {
    // Применяем выбранную сортировку к последним версиям библиотек
    if (sortBy.value === 'name') {
      return a.name.localeCompare(b.name);
    } else if (sortBy.value === '-name') {
      return b.name.localeCompare(a.name);
    } else if (sortBy.value === 'published_date') {
      return new Date(a.latest.published_date) - new Date(b.latest.published_date);
    } else if (sortBy.value === '-published_date') {
      return new Date(b.latest.published_date) - new Date(a.latest.published_date);
    } else if (sortBy.value === 'file_size') {
      return a.latest.file_size - b.latest.file_size;
    } else if (sortBy.value === '-file_size') {
      return b.latest.file_size - a.latest.file_size;
    }
    return 0;
  });
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
});

const clearNotification = () => {
  notification.value = null;
};

const fetchLibraries = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    // Загрузка всех библиотек
    const response = await store.fetchAllLibraries();
    libraries.value = response;
    console.log('Библиотеки успешно загружены:', libraries.value);
  } catch (err) {
    error.value = `Не удалось загрузить библиотеки: ${err.message || err}. Пожалуйста, попробуйте позже.`;
    console.error('Ошибка при загрузке библиотек:', err);
  } finally {
    loading.value = false;
  }
};

const applyFilters = () => {
  // Функция вызывается при изменении фильтров
  // Фактическая фильтрация происходит в computed свойстве libraryGroups
};

// Загружаем данные при монтировании компонента
onMounted(() => {
  fetchLibraries();
});
</script>

<style scoped>
.all-libraries-view {
  max-width: 1000px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.page-title {
  font-size: 2rem;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.filters-panel {
  padding: 1.25rem;
  margin-bottom: 2rem;
}

.filters-row {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  min-width: 200px;
  flex: 1;
}

.filter-group label {
  font-weight: 500;
  font-size: 0.95rem;
}

.filter-group input,
.filter-group select {
  padding: 0.625rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.95rem;
}

.filter-group input:focus,
.filter-group select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(67, 97, 238, 0.1);
}

.libraries-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
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
  color: var(--success-color);
}

.notification.error {
  background-color: #ffebee;
  color: var(--danger-color);
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
  color: var(--danger-color);
  margin-bottom: 1rem;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 768px) {
  .filters-row {
    flex-direction: column;
    gap: 1rem;
  }
  
  .filter-group {
    width: 100%;
  }
}
</style> 