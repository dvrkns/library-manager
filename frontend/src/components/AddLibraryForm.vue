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
        <label>Файл библиотеки</label>
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
  language: null, // Будет установлен автоматически на Python
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
let pythonLanguageId = null;

// Загружаем языки при монтировании компонента и находим ID для Python
onMounted(async () => {
  if (!languages.value.length) {
    await store.fetchLanguages();
  }
  
  // Находим ID языка Python
  const python = languages.value.find(lang => lang.name.toLowerCase() === 'python');
  if (python) {
    pythonLanguageId = python.id;
    form.language = pythonLanguageId;
  } else {
    error.value = 'Язык Python не найден в базе данных';
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
    // Проверяем, что язык Python найден
    if (!form.language) {
      throw new Error('Язык Python не найден в базе данных');
    }
    
    const formData = new FormData();
    
    // Добавляем все поля формы
    Object.keys(form).forEach(key => {
      if (form[key] !== null && form[key] !== '') {
        formData.append(key, form[key]);
      }
    });
    
    // Если выбран метод загрузки файла и файл выбран
    if (uploadMethod.value === 'file' && selectedFile.value) {
      formData.append('file', selectedFile.value);
    }
    
    await store.createLibraryWithFormData(formData);
    emit('added');
    resetForm();
  } catch (err) {
    error.value = err.message || 'Произошла ошибка при добавлении библиотеки';
    console.error(err);
  } finally {
    loading.value = false;
  }
};

// Сброс формы
const resetForm = () => {
  Object.keys(form).forEach(key => {
    if (key === 'published_date') {
      form[key] = new Date().toISOString().split('T')[0];
    } else if (key === 'language') {
      form[key] = pythonLanguageId;
    } else {
      form[key] = '';
    }
  });
  selectedFile.value = null;
  uploadMethod.value = 'file';
  if (fileInput.value) {
    fileInput.value.value = '';
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

.form-group {
  margin-bottom: 1.5rem;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

input, select, textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--border-color);
  border-radius: 4px;
  font-size: 1rem;
}

textarea {
  resize: vertical;
}

.error-message {
  background-color: #ffebee;
  color: #d32f2f;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-radius: 4px;
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

.btn.primary:hover {
  background-color: #3050d8;
}

.btn.secondary {
  background-color: #e9ecef;
  color: var(--text-color);
}

.btn.secondary:hover {
  background-color: #dee2e6;
}

/* Стили для загрузки файлов */
.file-upload-container {
  border: 1px solid var(--border-color);
  border-radius: 4px;
  overflow: hidden;
}

.upload-tabs {
  display: flex;
  border-bottom: 1px solid var(--border-color);
}

.tab-btn {
  flex: 1;
  padding: 0.75rem;
  background: none;
  border: none;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background-color 0.2s;
}

.tab-btn.active {
  background-color: var(--primary-color);
  color: white;
}

.upload-area {
  padding: 1rem;
}

.dropzone {
  border: 2px dashed var(--border-color);
  border-radius: 4px;
  padding: 2rem;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.3s;
}

.dropzone:hover {
  border-color: var(--primary-color);
}

.dropzone i {
  font-size: 2rem;
  color: var(--text-light);
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
  color: #d32f2f;
  font-size: 1.5rem;
  cursor: pointer;
}

.url-input {
  padding: 1rem;
}
</style> 