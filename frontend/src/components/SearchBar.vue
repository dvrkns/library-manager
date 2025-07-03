<template>
  <div class="search-container card mb-3">
    <div class="search-form">
      <div class="search-input-container mb-2">
        <div class="search-icon">🔍</div>
        <input
          id="search-input"
          type="text"
          v-model="query"
          placeholder="Введите название библиотеки, версию или ключевое слово..."
          @keyup.enter="search"
          class="search-input"
        />
      </div>
      
      <div class="search-options d-flex justify-between flex-wrap">
        <!-- Блок сортировки слева от кнопки поиска -->
        <div v-if="showSortingOptions" class="sort-filter">
          <label for="sort-select">Сортировать по:</label>
          <select id="sort-select" v-model="sortOption" @change="updateSorting">
            <option value="-published_date">Сначала новые</option>
            <option value="published_date">Сначала старые</option>
            <option value="name">По названию (А-Я)</option>
            <option value="-name">По названию (Я-А)</option>
          </select>
        </div>
        <div v-else class="sort-filter-placeholder"></div>
        
        <button id="search-button" class="btn search-btn" @click="search">
          <span class="search-btn-text">Поиск</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, defineEmits, defineProps } from 'vue';
import { useLibrariesStore } from '@/stores/libraries';

const props = defineProps({
  // Флаг, который показывает когда нужно отображать блок сортировки
  showSortingOptions: {
    type: Boolean,
    default: false
  }
});

const store = useLibrariesStore();
const emit = defineEmits(['search-performed']);

const query = ref('');
const sortOption = ref('-published_date');

// Watch for changes in the store
watch(() => store.searchQuery, (newValue) => {
  query.value = newValue;
});

watch(() => store.sortBy, (newValue) => {
  sortOption.value = newValue;
});

// Initialize sort option from store
onMounted(() => {
  sortOption.value = store.sortBy || '-published_date';
});

// Methods
const search = () => {
  // Проверяем, что поисковый запрос не пустой
  if (!query.value.trim()) return;
  
  // Update store values
  store.setSearchQuery(query.value);
  store.setSortBy(sortOption.value);
  
  // Execute search
  store.searchLibraries();
  
  // Emit event to notify parent component that search was performed
  emit('search-performed');
};

const updateSorting = () => {
  store.setSortBy(sortOption.value);
};
</script>

<style scoped>
.search-container {
  background-color: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.search-input-container {
  position: relative;
  margin-bottom: 1.5rem;
}

.search-icon {
  position: absolute;
  left: 16px;
  top: 50%;
  transform: translateY(-50%);
  font-size: 1.2rem;
  color: #6c757d;
  z-index: 1;
}

.search-input {
  width: 100%;
  padding: 1rem 1rem 1rem 3rem;
  font-size: 1.1rem;
  border: 2px solid #e9ecef;
  border-radius: 8px;
  transition: border-color 0.3s, box-shadow 0.3s;
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(67, 97, 238, 0.15);
}

.search-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
}

.sort-filter {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.sort-filter-placeholder {
  flex: 1;
}

.sort-filter label {
  font-weight: 500;
  white-space: nowrap;
  color: #555;
}

.sort-filter select {
  padding: 0.5rem 1rem;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  font-size: 0.95rem;
  background-color: #f9f9f9;
  cursor: pointer;
  transition: border-color 0.2s;
}

.sort-filter select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(67, 97, 238, 0.1);
}

.search-btn {
  padding: 0.75rem 1.5rem;
  background-color: var(--primary-color);
  font-weight: 600;
  transition: all 0.3s;
}

.search-btn:hover {
  background-color: #304dda;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.search-btn-text {
  display: inline-block;
}

@media (max-width: 768px) {
  .search-options {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .sort-filter {
    width: 100%;
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }
  
  .sort-filter select {
    width: 100%;
  }
  
  .search-btn {
    width: 100%;
    margin-top: 0.5rem;
  }
}
</style> 