<template>
  <div class="home">
    <div class="page-header">
      <h1 class="page-title">Библиотеки программного обеспечения</h1>
      <router-link to="/add-library" class="btn primary">
        Добавить библиотеку
      </router-link>
    </div>
    
    <div v-if="notification" class="notification" :class="notification.type">
      {{ notification.message }}
      <button class="close-btn" @click="clearNotification">&times;</button>
    </div>
    
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
import { onMounted, computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useLibrariesStore } from '@/stores/libraries';
import SearchBar from '@/components/SearchBar.vue';
import LibraryCard from '@/components/LibraryCard.vue';

const store = useLibrariesStore();
const route = useRoute();

// Computed properties
const libraries = computed(() => store.libraries);
const loading = computed(() => store.loading);
const error = computed(() => store.error);

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

// Load libraries when component mounts
onMounted(async () => {
  if (!libraries.value.length) {
    await store.fetchLibraries();
  }
});
</script>

<style scoped>
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
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
  text-align: center;
  padding: 2rem;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.error-message {
  color: #dc3545;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
</style> 