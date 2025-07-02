<template>
  <div class="add-library-view">
    <div class="page-header">
      <h1>Добавление новой библиотеки</h1>
      <router-link to="/" class="btn secondary">
        Вернуться к списку
      </router-link>
    </div>
    
    <AddLibraryForm 
      @added="onLibraryAdded" 
      @cancel="goToHome" 
    />
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router';
import AddLibraryForm from '@/components/AddLibraryForm.vue';

const router = useRouter();

const onLibraryAdded = () => {
  router.push({ 
    path: '/',
    query: { 
      message: 'Библиотека успешно добавлена',
      type: 'success'
    }
  });
};

const goToHome = () => {
  router.push('/');
};
</script>

<style scoped>
.add-library-view {
  max-width: 800px;
  margin: 0 auto;
}

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

.btn.secondary {
  background-color: #e9ecef;
  color: var(--text-color);
}

.btn.secondary:hover {
  background-color: #dee2e6;
}
</style> 