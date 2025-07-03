<template>
  <div class="library-list-item">
    <div class="library-info">
      <div class="library-header">
        <router-link :to="{ name: 'library-detail', params: { id: library.id } }" class="library-name">
          {{ library.name }}
        </router-link>
        <div class="library-version-tag">
          <span class="version-label">{{ library.version }}</span>
        </div>
      </div>
      
      <div class="library-description" v-if="library.description">
        {{ truncateDescription(library.description) }}
      </div>
      
      <div class="library-details">
        <div class="detail-item">
          <span class="detail-label">Язык:</span>
          <span class="detail-value">{{ library.language_name }}</span>
        </div>
        <div class="detail-item">
          <span class="detail-label">Размер:</span>
          <span class="detail-value">{{ formatFileSize(library.file_size) }}</span>
        </div>
        <div class="detail-item" v-if="library.author">
          <span class="detail-label">Автор:</span>
          <span class="detail-value">{{ library.author }}</span>
        </div>
        <div class="detail-item">
          <span class="detail-label">Опубликовано:</span>
          <span class="detail-value">{{ formatDate(library.published_date) }}</span>
        </div>
      </div>
    </div>
    
    <div class="library-actions">
      <div class="version-selector">
        <label for="version-select" class="version-label">Версия:</label>
        <select id="version-select" v-model="selectedVersion" @change="onVersionChange">
          <option :value="library.id">{{ library.version }}</option>
          <option v-for="ver in otherVersions" :key="ver.id" :value="ver.id">
            {{ ver.version }}
          </option>
        </select>
      </div>
      
      <div class="action-buttons">
        <a 
          v-if="selectedLibrary && (selectedLibrary.file || selectedLibrary.download_url)" 
          :href="selectedLibrary.file || selectedLibrary.download_url" 
          target="_blank" 
          class="btn-action btn-primary"
          download
        >
          Скачать
        </a>
        <router-link 
          v-if="showEdit"
          :to="{ name: 'edit-library', params: { id: library.id } }" 
          class="btn-action btn-warning"
        >
          Редактировать
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { defineProps, ref, computed, watch, onMounted } from 'vue';
import { useLibrariesStore } from '@/stores/libraries';

const props = defineProps({
  library: {
    type: Object,
    required: true
  },
  // Другие версии той же библиотеки
  versions: {
    type: Array,
    default: () => []
  },
  showEdit: {
    type: Boolean,
    default: false
  }
});

const store = useLibrariesStore();
const selectedVersion = ref(props.library.id);
const selectedLibrary = ref(props.library);

// Другие версии библиотеки (исключая текущую)
const otherVersions = computed(() => {
  return props.versions.filter(ver => ver.id !== props.library.id);
});

watch(selectedVersion, async (newValue) => {
  if (newValue === props.library.id) {
    selectedLibrary.value = props.library;
  } else {
    // Загружаем данные для выбранной версии
    try {
      await store.fetchLibraryById(newValue);
      selectedLibrary.value = store.library;
    } catch (error) {
      console.error('Ошибка при загрузке версии библиотеки', error);
    }
  }
});

const onVersionChange = () => {
  // Этот метод нужен для обработки изменения версии, если потребуется дополнительная логика
};

const truncateDescription = (text, maxLength = 150) => {
  if (!text || text.length <= maxLength) return text;
  return text.slice(0, maxLength) + '...';
};

const formatDate = (dateString) => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('ru-RU', { 
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  }).format(date);
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
.library-list-item {
  display: flex;
  justify-content: space-between;
  padding: 1.25rem;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 1rem;
  transition: box-shadow 0.3s, transform 0.2s;
}

.library-list-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transform: translateY(-2px);
}

.library-info {
  flex: 1;
  margin-right: 1.5rem;
}

.library-header {
  display: flex;
  align-items: center;
  margin-bottom: 0.75rem;
  gap: 1rem;
}

.library-name {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--primary-color);
  text-decoration: none;
}

.library-name:hover {
  text-decoration: underline;
}

.library-version-tag {
  padding: 0.25rem 0.5rem;
  background-color: #e6effd;
  border-radius: 4px;
  font-size: 0.875rem;
  color: var(--primary-color);
}

.library-description {
  margin-bottom: 1rem;
  color: #555;
  line-height: 1.5;
}

.library-details {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem 2rem;
}

.detail-item {
  display: flex;
  gap: 0.5rem;
}

.detail-label {
  color: #666;
  font-weight: 500;
}

.detail-value {
  color: #333;
}

.library-actions {
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 1rem;
  min-width: 200px;
}

.version-selector {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.version-label {
  font-size: 0.875rem;
  color: #666;
  font-weight: 500;
}

select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 0.95rem;
  background-color: white;
}

.action-buttons {
  display: flex;
  gap: 0.75rem;
}

.btn-action {
  flex: 1;
  text-align: center;
  text-decoration: none;
}

@media (max-width: 768px) {
  .library-list-item {
    flex-direction: column;
  }
  
  .library-info {
    margin-right: 0;
    margin-bottom: 1.5rem;
  }
  
  .library-actions {
    min-width: auto;
    width: 100%;
  }
}
</style> 