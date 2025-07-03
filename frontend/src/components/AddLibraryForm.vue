<template>
  <div class="add-library-form">
    <h2>Добавить новую библиотеку</h2>
    
    <div v-if="error" class="error-message">
      {{ error }}
    </div>
    
    <form @submit.prevent="submitForm" enctype="multipart/form-data">
      <div class="form-group">
        <label for="name">Название библиотеки*</label>
        <input 
          id="name" 
          v-model="form.name" 
          type="text" 
          required 
          placeholder="Например: React, Vue, Django"
        />
      </div>
      
      <div class="form-group">
        <label for="version">Версия*</label>
        <input 
          id="version" 
          v-model="form.version" 
          type="text" 
          required 
          placeholder="Например: 1.0.0, 2.3.1"
        />
      </div>
      
      <div class="form-group">
        <label for="description">Описание</label>
        <textarea 
          id="description" 
          v-model="form.description" 
          rows="4" 
          placeholder="Краткое описание библиотеки"
        ></textarea>
      </div>
      
      <div class="form-group">
        <label for="language">Язык программирования*</label>
        <select 
          id="language" 
          v-model="form.language" 
          required
        >
          <option v-for="lang in languages" :key="lang.id" :value="lang.id">
            {{ lang.name }}
          </option>
        </select>
      </div>
      
      <div class="form-group">
        <label for="author">Автор</label>
        <input 
          id="author" 
          v-model="form.author" 
          type="text" 
          placeholder="Имя автора или организации"
        />
      </div>
      
      <div class="form-group">
        <label for="homepage">Домашняя страница</label>
        <input 
          id="homepage" 
          v-model="form.homepage" 
          type="url" 
          placeholder="https://example.com"
        />
      </div>
      
      <div class="form-group">
        <label for="repository">Репозиторий</label>
        <input 
          id="repository" 
          v-model="form.repository" 
          type="url" 
          placeholder="https://github.com/example/repo"
        />
      </div>
      
      <div class="form-group">
        <label>Файл библиотеки*</label>
        <div class="file-upload-container">
          <div class="upload-tabs">
            <button 
              type="button" 
              :class="['tab-btn', { active: uploadMethod === 'file' }]" 
              @click="uploadMethod = 'file'"
            >
              Загрузить файл
            </button>
            <button 
              type="button" 
              :class="['tab-btn', { active: uploadMethod === 'url' }]" 
              @click="uploadMethod = 'url'"
            >
              Указать URL
            </button>
          </div>
          
          <div v-if="uploadMethod === 'file'" class="upload-area">
            <div 
              class="dropzone" 
              @dragover.prevent 
              @drop.prevent="onFileDrop"
              @click="triggerFileInput"
            >
              <div v-if="!selectedFile">
                <i class="fas fa-cloud-upload-alt"></i>
                <p>Перетащите файл сюда или кликните для выбора</p>
              </div>
              <div v-else class="selected-file">
                <p>{{ selectedFile.name }} ({{ formatFileSize(selectedFile.size) }})</p>
                <button type="button" class="remove-file" @click.stop="removeFile">×</button>
              </div>
            </div>
            <input 
              ref="fileInput"
              type="file" 
              style="display: none"
              @change="onFileChange"
            />
          </div>
          
          <div v-else class="url-input">
            <input 
              id="download_url" 
              v-model="form.download_url" 
              type="url" 
              placeholder="https://example.com/library.tar.gz"
            />
          </div>
        </div>
      </div>
      
      <div class="form-group">
        <label for="published_date">Дата публикации</label>
        <input 
          id="published_date" 
          v-model="form.published_date" 
          type="date"
        />
      </div>
      
      <div class="form-actions">
        <button 
          type="submit" 
          class="btn primary" 
          :disabled="loading"
        >
          {{ loading ? 'Добавление...' : 'Добавить библиотеку' }}
        </button>
        <button 
          type="button" 
          class="btn secondary" 
          @click="$emit('cancel')"
          :disabled="loading"
        >
          Отмена
        </button>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useLibrariesStore } from '@/stores/libraries';

const store = useLibrariesStore();
const emit = defineEmits(['added', 'cancel']);

// Состояние формы
const form = reactive({
  name: '',
  version: '',
  description: '',
  language: null,
  author: '',
  homepage: '',
  repository: '',
  download_url: '',
  published_date: new Date().toISOString().split('T')[0] // Текущая дата в формате YYYY-MM-DD
});

const loading = ref(false);
const error = ref('');
const uploadMethod = ref('file');
const selectedFile = ref(null);
const fileInput = ref(null);

// Получаем список языков из хранилища
const languages = computed(() => store.languages);

// Загружаем языки при монтировании компонента
onMounted(async () => {
  if (!languages.value.length) {
    await store.fetchLanguages();
  }
  
  // Если есть языки, выбираем первый по умолчанию
  if (languages.value.length > 0) {
    form.language = languages.value[0].id;
  }
});

// Функции для работы с файлами
const triggerFileInput = () => {
  fileInput.value.click();
};

const onFileChange = (event) => {
  const file = event.target.files[0];
  if (file) {
    selectedFile.value = file;
  }
};

const onFileDrop = (event) => {
  const file = event.dataTransfer.files[0];
  if (file) {
    selectedFile.value = file;
  }
};

const removeFile = () => {
  selectedFile.value = null;
  if (fileInput.value) {
    fileInput.value.value = '';
  }
};

const formatFileSize = (bytes) => {
  if (bytes < 1024) return bytes + ' B';
  else if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
  else return (bytes / 1048576).toFixed(1) + ' MB';
};

// Отправка формы
const submitForm = async () => {
  loading.value = true;
  error.value = '';
  
  try {
    // Проверяем, что язык выбран
    if (!form.language) {
      throw new Error('Необходимо выбрать язык программирования');
    }
    
    const formData = new FormData();
    
    // Добавляем все поля формы
    Object.keys(form).forEach(key => {
      if (form[key] !== null && form[key] !== '') {
        formData.append(key, form[key]);
      }
    });
    
    // Добавляем файл, если он выбран
    if (uploadMethod.value === 'file' && selectedFile.value) {
      formData.append('file', selectedFile.value);
    }
    
    // Проверяем, что есть файл или URL
    if (uploadMethod.value === 'file' && !selectedFile.value && !form.download_url) {
      throw new Error('Необходимо загрузить файл или указать URL для скачивания');
    }
    
    // Отправляем данные
    await store.createLibraryWithFormData(formData);
    
    // Сообщаем об успешном добавлении
    emit('added');
  } catch (err) {
    console.error('Ошибка при добавлении библиотеки:', err);
    error.value = err.message || 'Произошла ошибка при добавлении библиотеки';
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.add-library-form {
  background-color: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

h2 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  color: var(--primary-color);
}

.form-group {
  margin-bottom: 1.25rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

input[type="text"],
input[type="url"],
input[type="date"],
textarea,
select {
  width: 100%;
  padding: 0.625rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.95rem;
  font-family: inherit;
}

input[type="text"]:focus,
input[type="url"]:focus,
input[type="date"]:focus,
textarea:focus,
select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(67, 97, 238, 0.1);
}

.error-message {
  background-color: #ffebee;
  color: var(--danger-color);
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
}

.file-upload-container {
  border: 1px solid #ddd;
  border-radius: 6px;
  overflow: hidden;
}

.upload-tabs {
  display: flex;
  border-bottom: 1px solid #ddd;
}

.tab-btn {
  flex: 1;
  padding: 0.75rem;
  background: none;
  border: none;
  cursor: pointer;
  transition: background-color 0.2s;
}

.tab-btn.active {
  background-color: #f1f5ff;
  color: var(--primary-color);
  font-weight: 500;
}

.upload-area {
  padding: 1rem;
}

.dropzone {
  border: 2px dashed #ddd;
  border-radius: 4px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s;
}

.dropzone:hover {
  border-color: var(--primary-color);
}

.dropzone i {
  font-size: 2rem;
  color: #aaa;
  margin-bottom: 0.5rem;
}

.selected-file {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.remove-file {
  background: none;
  border: none;
  color: var(--danger-color);
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0.25rem 0.5rem;
}

.url-input {
  padding: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

.btn.primary {
  background-color: var(--primary-color);
  color: white;
}

.btn.primary:hover:not(:disabled) {
  background-color: #3050d8;
}

.btn.secondary {
  background-color: #e9ecef;
  color: var(--text-color);
}

.btn.secondary:hover:not(:disabled) {
  background-color: #dee2e6;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .add-library-form {
    padding: 1.5rem;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .btn {
    width: 100%;
  }
}
</style> 