<template>
  <div class="search-container card mb-3">
    <div class="search-form">
      <div class="search-input-container mb-2">
        <input
          id="search-input"
          type="text"
          v-model="query"
          placeholder="Поиск библиотек по названию, версии, дате публикации..."
          @keyup.enter="search"
        />
      </div>
      
      <div class="search-options d-flex justify-between flex-wrap">
        <div class="filter-group d-flex align-center">
          <div class="language-filter mr-1">
            <select v-model="selectedLanguage">
              <option value="">Все языки</option>
              <option v-for="lang in languages" :key="lang.id" :value="lang.slug">
                {{ lang.name }}
              </option>
            </select>
          </div>
          
          <div class="sort-filter">
            <select v-model="sortOption">
              <option value="-published_date">Сначала новые</option>
              <option value="published_date">Сначала старые</option>
              <option value="name">По названию (А-Я)</option>
              <option value="-name">По названию (Я-А)</option>
            </select>
          </div>
        </div>
        
        <button id="search-button" class="btn" @click="search">Поиск</button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue';
import { useLibrariesStore } from '@/stores/libraries';

const store = useLibrariesStore();

const query = ref('');
const selectedLanguage = ref('');
const sortOption = ref('-published_date');
const languages = ref([]);

// Initialize data
onMounted(async () => {
  await store.fetchLanguages();
  languages.value = store.languages;
});

// Watch for changes in the store
watch(() => store.searchQuery, (newValue) => {
  query.value = newValue;
});

watch(() => store.selectedLanguage, (newValue) => {
  selectedLanguage.value = newValue;
});

watch(() => store.sortBy, (newValue) => {
  sortOption.value = newValue;
});

// Methods
const search = () => {
  // Update store values
  store.setSearchQuery(query.value);
  store.setSelectedLanguage(selectedLanguage.value);
  store.setSortBy(sortOption.value);
  
  // Execute search
  store.searchLibraries();
};
</script>

<style scoped>
.search-container {
  background-color: white;
  padding: 1.5rem;
}

.search-input-container input {
  width: 100%;
  padding: 0.75rem;
  font-size: 1rem;
}

.filter-group {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.language-filter,
.sort-filter {
  width: 200px;
}

@media (max-width: 768px) {
  .search-options {
    flex-direction: column;
    gap: 1rem;
  }
  
  .filter-group {
    width: 100%;
    margin-bottom: 1rem;
  }
  
  .language-filter,
  .sort-filter {
    width: 100%;
  }
  
  button {
    width: 100%;
  }
}
</style> 