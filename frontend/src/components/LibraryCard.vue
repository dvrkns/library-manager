<template>
  <div class="library-card card">
    <div class="card-header d-flex justify-between align-center mb-2">
      <h3 class="library-name">{{ library.name }}</h3>
      <span class="library-version badge">{{ library.version }}</span>
    </div>
    
    <div class="card-body">
      <div class="library-language mb-2">
        <span class="text-muted">Язык: </span>
        <span class="badge">{{ library.language_name }}</span>
      </div>
      
      <div class="library-date mb-2">
        <span class="text-muted">Опубликовано: </span>
        <span>{{ formatDate(library.published_date) }}</span>
      </div>
      
      <div v-if="library.description" class="library-description mb-2">
        <p>{{ truncateDescription(library.description) }}</p>
      </div>
      
      <div v-if="library.author" class="library-author mb-2">
        <span class="text-muted">Автор: </span>
        <span>{{ library.author }}</span>
      </div>
    </div>
    
    <div class="card-footer mt-2">
      <router-link :to="{ name: 'library-detail', params: { id: library.id } }" class="btn">
        Подробнее
      </router-link>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue';

const props = defineProps({
  library: {
    type: Object,
    required: true
  }
});

const formatDate = (dateString) => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ru-RU', { 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  }).format(date);
};

const truncateDescription = (text, maxLength = 100) => {
  if (!text || text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
};
</script>

<style scoped>
.library-card {
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: transform 0.2s, box-shadow 0.2s;
}

.library-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.card-body {
  flex: 1;
}

.library-name {
  font-size: 1.25rem;
  margin: 0;
  color: var(--primary-color);
}

.library-version {
  background-color: #e6effd;
  color: var(--primary-color);
}

.library-description {
  color: #666;
  line-height: 1.4;
}

.card-footer {
  margin-top: auto;
}
</style> 