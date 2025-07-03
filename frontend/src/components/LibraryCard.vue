<template>
  <div class="library-card card">
    <div class="card-header d-flex justify-between align-center mb-2">
      <h3 class="library-name">{{ library.name }}</h3>
      <span class="library-version badge">{{ library.version }}</span>
    </div>
    
    <div class="card-body">
      <div v-if="library.description" class="library-description mb-2">
        <p>{{ truncateDescription(library.description) }}</p>
      </div>
      
      <div class="library-size mb-2">
        <span class="text-muted">Размер: </span>
        <span>{{ formatFileSize(library.file_size) }}</span>
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

const truncateDescription = (text, maxLength = 100) => {
  if (!text || text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
};

const formatFileSize = (bytes) => {
  if (!bytes || bytes === 0) return 'Н/Д';
  
  const units = ['байт', 'КБ', 'МБ', 'ГБ'];
  let size = bytes;
  let unitIndex = 0;
  
  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024;
    unitIndex++;
  }
  
  return `${size.toFixed(1)} ${units[unitIndex]}`;
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