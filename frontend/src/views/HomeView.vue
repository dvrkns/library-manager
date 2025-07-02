<template>
  <div class="home">
    <h1 class="page-title">Библиотеки программного обеспечения</h1>
    
    <SearchBar />
    
    <div v-if="loading" class="loading-container">
      <p>Загрузка библиотек...</p>
    </div>
    
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
    </div>
    
    <div v-else-if="libraries.length === 0" class="empty-container">
      <p>Библиотеки не найдены. Попробуйте изменить параметры поиска.</p>
    </div>
    
    <div v-else class="libraries-grid grid">
      <LibraryCard 
        v-for="library in libraries" 
        :key="library.id" 
        :library="library" 
      />
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue';
import { useLibrariesStore } from '@/stores/libraries';
import SearchBar from '@/components/SearchBar.vue';
import LibraryCard from '@/components/LibraryCard.vue';

const store = useLibrariesStore();

// Computed properties
const libraries = computed(() => store.libraries);
const loading = computed(() => store.loading);
const error = computed(() => store.error);

// Load libraries when component mounts
onMounted(async () => {
  if (!libraries.value.length) {
    await store.fetchLibraries();
  }
});
</script>

<style scoped>
.loading-container,
.error-container,
.empty-container {
  text-align: center;
  padding: 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.error-message {
  color: #dc3545;
}
</style> 