<template>
  <div class="edit-library-view">
    <div class="page-header">
      <h1 class="page-title" v-if="!loading && library">
        Редактирование библиотеки: {{ library.name }}
      </h1>
      <h1 class="page-title" v-else>Редактирование библиотеки</h1>
      <div class="header-actions">
        <router-link :to="{ name: 'all-libraries' }" class="btn-action btn-primary">
          Вернуться к списку
        </router-link>
      </div>
    </div>
    
    <div v-if="loading" class="loading-container">
      <p>Загрузка данных библиотеки...</p>
    </div>
    
    <div v-else-if="error && typeof error === 'string'" class="notification error">
      {{ error }}
      <button class="close-btn" @click="error = null">&times;</button>
    </div>
    
    <div v-else-if="error && error.type" class="notification" :class="error.type">
      {{ error.message }}
      <button class="close-btn" @click="error = null">&times;</button>
    </div>
    
    <template v-else-if="library">
      <div class="tabs">
        <button 
          class="tab-button" 
          :class="{ 'active': activeTab === 'edit' }"
          @click="activeTab = 'edit'"
        >
          Основная информация
        </button>
        <button 
          class="tab-button" 
          :class="{ 'active': activeTab === 'versions' }"
          @click="activeTab = 'versions'"
        >
          Управление версиями
        </button>
      </div>
      
      <!-- Форма редактирования основной информации библиотеки -->
      <div v-if="activeTab === 'edit'" class="edit-form card">
        <h2 class="form-title">Редактирование библиотеки</h2>
        
        <div class="form-group">
          <label for="edit-name">Название:</label>
          <input 
            type="text" 
            id="edit-name" 
            v-model="editForm.name" 
            placeholder="Название библиотеки"
          />
        </div>
        
        <div class="form-group">
          <label for="edit-description">Описание:</label>
          <textarea 
            id="edit-description" 
            v-model="editForm.description" 
            placeholder="Описание библиотеки"
            rows="5"
          ></textarea>
        </div>
        
        <div class="form-group">
          <label for="edit-repository">Репозиторий:</label>
          <input 
            type="url" 
            id="edit-repository" 
            v-model="editForm.repository" 
            placeholder="https://github.com/example/repo"
          />
        </div>
        
        <div class="current-version-info">
          <h3 class="section-title">Текущая версия</h3>
          <div class="version-badge">
            {{ library.version }}
          </div>
          <p class="version-hint">
            Вы можете <a href="#" @click.prevent="goToAddVersionForm">добавить новую версию</a> 
            или управлять существующими версиями на вкладке "Управление версиями".
          </p>
        </div>
        
        <div class="form-actions">
          <button class="btn-action btn-success" @click="updateLibrary" :disabled="updating">
            {{ updating ? 'Сохранение...' : 'Сохранить изменения' }}
          </button>
          <button class="btn-action btn-primary" @click="goToAddVersionForm">
            Управление версиями
          </button>
          <button class="btn-action btn-danger" @click="confirmDelete">
            Удалить библиотеку
          </button>
        </div>
      </div>
      
      <!-- Управление версиями библиотеки -->
      <div v-if="activeTab === 'versions'" class="versions-panel card">
        <h2 class="form-title">Управление версиями</h2>
        
        <div class="version-info-panel">
          <p>Здесь вы можете добавлять новые версии библиотеки и управлять существующими версиями.</p>
          <p>Каждая версия должна иметь уникальный номер. Текущее название библиотеки: <strong>{{ library.name }}</strong>.</p>
        </div>
        
        <!-- Список существующих версий -->
        <div class="versions-list">
          <h3 class="section-title">Существующие версии</h3>
          
          <div v-if="loadingVersions" class="loading-message">
            Загрузка версий...
          </div>
          
          <div v-else-if="versions.length === 0" class="empty-message">
            Нет доступных версий
          </div>
          
          <div v-else class="version-items">
            <div v-for="version in versions" :key="version.id" class="version-item">
              <div class="version-info">
                <span class="version-number">{{ version.version }}</span>
                <span class="version-date">{{ formatDate(version.published_date) }}</span>
                <span class="version-size">{{ formatFileSize(version.file_size) }}</span>
              </div>
              <div class="version-actions">
                <a 
                  v-if="version.file || version.download_url" 
                  :href="version.file || version.download_url" 
                  target="_blank" 
                  class="btn-action btn-primary btn-sm"
                  download
                >
                  Скачать
                </a>
                <button 
                  class="btn-action btn-danger btn-sm" 
                  @click="confirmDeleteVersion(version)"
                  :disabled="versions.length <= 1"
                >
                  Удалить
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Форма добавления новой версии -->
        <div class="add-version-form">
          <h3 class="section-title">Добавить новую версию</h3>
          <div class="form-group">
            <label>Файл библиотеки: <span class="required">*</span></label>
            <div class="file-upload">
              <label for="new-file" class="file-label">
                {{ newVersionForm.file ? 'Изменить файл' : 'Загрузить файл' }}
              </label>
              <input 
                type="file" 
                id="new-file" 
                @change="handleNewFileChange"
                class="file-input"
              />
              <div class="file-info" v-if="newVersionForm.file">
                <span>Выбран файл: {{ newVersionForm.file.name }}</span>
                <span>({{ formatFileSize(newVersionForm.file.size) }})</span>
                <div class="auto-fields">
                  <div><b>Версия:</b> {{ newVersionForm.version }}</div>
                </div>
              </div>
            </div>
          </div>
          <div class="form-group" v-if="!newVersionForm.file">
            <label for="new-download-url">Ссылка для скачивания: <span class="required">*</span></label>
            <input 
              type="url" 
              id="new-download-url" 
              v-model="newVersionForm.download_url" 
              placeholder="https://example.com/library.zip"
            />
            <small class="form-help">Требуется загрузить файл или указать ссылку для скачивания</small>
          </div>
          <div class="form-actions">
            <button 
              class="btn-action btn-success" 
              @click="addNewVersion" 
              :disabled="adding || !isNewVersionFormValid"
            >
              {{ adding ? 'Добавление...' : 'Добавить версию' }}
            </button>
            <button class="btn-action btn-secondary" @click="activeTab = 'edit'">
              Вернуться к редактированию
            </button>
            <button class="btn-action btn-danger" @click="resetNewVersionForm">
              Очистить форму
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useLibrariesStore } from '@/stores/libraries';

const store = useLibrariesStore();
const route = useRoute();
const router = useRouter();

const library = ref(null);
const loading = ref(false);
const error = ref(null);
const updating = ref(false);
const adding = ref(false);
const activeTab = ref('edit');
const versions = ref([]);
const loadingVersions = ref(false);

const languages = computed(() => store.languages);

// Форма редактирования основной информации библиотеки
const editForm = reactive({
  name: '',
  description: '',
  repository: ''
});

// Форма добавления новой версии
const newVersionForm = reactive({
  version: '',
  file: null,
  download_url: ''
});

// Проверка валидности формы для новой версии
const isNewVersionFormValid = computed(() => {
  return (
    (newVersionForm.file && newVersionForm.version) || newVersionForm.download_url
  );
});

// Загрузка данных библиотеки
const fetchLibrary = async () => {
  const id = route.params.id;
  if (!id) return;
  
  loading.value = true;
  error.value = null;
  
  try {
    // Загрузка языков, если они еще не загружены
    if (store.languages.length === 0) {
      await store.fetchLanguages();
    }
    
    // Загрузка данных библиотеки
    await store.fetchLibraryById(id);
    library.value = store.library;
    
    // Заполнение формы редактирования
    if (library.value) {
      editForm.name = library.value.name;
      editForm.description = library.value.description || '';
      editForm.repository = library.value.repository || '';
      
      // Загрузка версий библиотеки
      fetchVersions(library.value.name);
    }
  } catch (err) {
    error.value = 'Не удалось загрузить данные библиотеки.';
    console.error('Ошибка при загрузке библиотеки:', err);
  } finally {
    loading.value = false;
  }
};

// Загрузка всех версий библиотеки
const fetchVersions = async (name) => {
  if (!name) return;
  
  loadingVersions.value = true;
  
  try {
    const response = await store.getLibraryVersions(name);
    
    // Убедимся, что получили массив версий
    if (Array.isArray(response)) {
      versions.value = response;
    } else {
      console.error('Некорректный формат данных версий:', response);
      versions.value = [];
    }
  } catch (err) {
    console.error('Ошибка при загрузке версий библиотеки:', err);
    error.value = {
      type: 'error',
      message: 'Не удалось загрузить версии библиотеки'
    };
    versions.value = [];
  } finally {
    loadingVersions.value = false;
  }
};

// Обработчики файлов
const handleNewFileChange = (event) => {
  const file = event.target.files[0];
  if (file) {
    newVersionForm.file = file;
    autofillVersionFromFilename(file.name);
  }
};

function autofillVersionFromFilename(filename) {
  // Удаляем расширение
  const nameWithoutExt = filename.replace(/\.[^.]+$/, '');
  // Ищем последний дефис
  const lastDash = nameWithoutExt.lastIndexOf('-');
  if (lastDash > 0) {
    newVersionForm.version = nameWithoutExt.slice(lastDash + 1);
  } else {
    newVersionForm.version = '';
  }
}

// Сохранение изменений основной информации
const updateLibrary = async () => {
  if (!library.value) return;
  
  updating.value = true;
  error.value = null;
  
  try {
    // Создание данных для обновления
    const formData = new FormData();
    formData.append('name', editForm.name);
    formData.append('description', editForm.description);
    formData.append('repository', editForm.repository);
    
    // Добавляем необходимые поля из текущей библиотеки
    formData.append('language', library.value.language);
    formData.append('version', library.value.version);
    
    // Сохраняем текущую дату публикации
    formData.append('published_date', library.value.published_date);
    
    // Если есть существующий файл
    if (library.value.file) {
      formData.append('file_url', library.value.file);
    }
    
    // Если есть URL для скачивания
    if (library.value.download_url) {
      formData.append('download_url', library.value.download_url);
    }
    
    // Обновляем библиотеку
    await store.updateLibraryWithFormData(library.value.id, formData);
    
    // Показываем уведомление об успехе
    error.value = {
      type: 'success',
      message: 'Библиотека успешно обновлена'
    };
    
    // Перезагружаем данные библиотеки
    await fetchLibrary();
  } catch (err) {
    console.error('Ошибка при обновлении библиотеки:', err);
    error.value = { 
      type: 'error',
      message: 'Не удалось обновить библиотеку: ' + (err.response?.data?.detail || err.message || '')
    };
  } finally {
    updating.value = false;
  }
};

// Добавление новой версии
const addNewVersion = async () => {
  if (!library.value || !isNewVersionFormValid.value) return;
  
  adding.value = true;
  error.value = null;
  
  try {
    // Создание FormData для отправки файла
    const formData = new FormData();
    formData.append('name', library.value.name);
    formData.append('version', newVersionForm.version);
    formData.append('description', library.value.description || '');
    // Используем существующий язык Python
    formData.append('language', library.value.language);
    formData.append('repository', library.value.repository || '');
    
    // Используем текущую дату как дату публикации
    const today = new Date().toISOString().split('T')[0];
    formData.append('published_date', today);
    
    if (newVersionForm.file) {
      formData.append('file', newVersionForm.file);
    }
    
    if (newVersionForm.download_url) {
      formData.append('download_url', newVersionForm.download_url);
    }
    
    // Создаем новую версию
    await store.createLibraryWithFormData(formData);
    
    // Обновляем список версий
    await fetchVersions(library.value.name);
    
    // Сбрасываем форму
    resetNewVersionForm();
    
    // Показываем уведомление
    error.value = { type: 'success', message: 'Новая версия успешно добавлена' };
  } catch (err) {
    console.error('Ошибка при добавлении новой версии:', err);
    error.value = { type: 'error', message: 'Не удалось добавить новую версию: ' + (err.response?.data?.detail || err.message || '') };
  } finally {
    adding.value = false;
  }
};

// Удаление библиотеки
const confirmDelete = async () => {
  if (!library.value) return;
  
  if (confirm('Вы действительно хотите удалить эту библиотеку и все ее версии?')) {
    try {
      // Удаляем все версии библиотеки
      for (const version of versions.value) {
        await store.deleteLibrary(version.id);
      }
      
      router.push({
        path: '/libraries',
        query: {
          message: 'Библиотека и все ее версии успешно удалены',
          type: 'success'
        }
      });
    } catch (err) {
      error.value = 'Не удалось удалить библиотеку.';
      console.error('Ошибка при удалении библиотеки:', err);
    }
  }
};

// Удаление версии
const confirmDeleteVersion = async (version) => {
  if (versions.value.length <= 1) {
    error.value = {
      type: 'error',
      message: 'Нельзя удалить единственную версию библиотеки. Удалите всю библиотеку.'
    };
    return;
  }
  
  if (confirm(`Вы действительно хотите удалить версию ${version.version}?`)) {
    try {
      await store.deleteLibrary(version.id);
      
      // Обновляем список версий
      await fetchVersions(library.value.name);
      
      // Показываем уведомление
      error.value = { 
        type: 'success', 
        message: `Версия ${version.version} успешно удалена` 
      };
    } catch (err) {
      console.error('Ошибка при удалении версии:', err);
      error.value = { 
        type: 'error', 
        message: 'Не удалось удалить версию библиотеки: ' + (err.response?.data?.detail || err.message || '')
      };
    }
  }
};

// Сброс формы новой версии
const resetNewVersionForm = () => {
  newVersionForm.version = '';
  newVersionForm.file = null;
  newVersionForm.download_url = '';
};

// Вспомогательные функции
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

// Функция для переключения на форму добавления новой версии
const goToAddVersionForm = () => {
  activeTab.value = 'versions';
  // Прокрутка к форме добавления версии
  setTimeout(() => {
    const addVersionForm = document.querySelector('.add-version-form');
    if (addVersionForm) {
      addVersionForm.scrollIntoView({ behavior: 'smooth' });
    }
  }, 100);
};

// Загрузка данных при монтировании компонента
onMounted(() => {
  fetchLibrary();
});
</script>

<style scoped>
.edit-library-view {
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.page-title {
  font-size: 1.75rem;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 0.75rem;
}

.tabs {
  display: flex;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid var(--border-color);
}

.tab-button {
  padding: 0.75rem 1.25rem;
  border: none;
  background: none;
  font-size: 1rem;
  cursor: pointer;
  border-bottom: 3px solid transparent;
  transition: all 0.2s;
}

.tab-button.active {
  border-bottom-color: var(--primary-color);
  color: var(--primary-color);
  font-weight: 500;
}

.tab-button:hover:not(.active) {
  background-color: #f5f5f5;
}

.card {
  padding: 1.5rem;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 1.5rem;
}

.form-title {
  font-size: 1.25rem;
  margin-top: 0;
  margin-bottom: 1.5rem;
  color: #333;
}

.section-title {
  font-size: 1.1rem;
  margin-top: 1.5rem;
  margin-bottom: 1rem;
  color: #444;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-row {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 1.25rem;
}

.form-row .form-group {
  flex: 1;
  margin-bottom: 0;
}

label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.required {
  color: #dc3545;
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

input:focus,
textarea:focus,
select:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(67, 97, 238, 0.1);
}

.form-help {
  display: block;
  margin-top: 0.25rem;
  font-size: 0.875rem;
  color: #666;
}

/* Styles for the file upload */
.file-upload {
  margin-bottom: 1rem;
}

.file-label {
  display: inline-block;
  padding: 0.5rem 1rem;
  background-color: #f1f5ff;
  color: var(--primary-color);
  border-radius: 4px;
  border: 1px solid #d8e0fa;
  cursor: pointer;
  font-weight: normal;
  transition: all 0.2s;
}

.file-label:hover {
  background-color: #e6ecff;
}

.file-input {
  position: absolute;
  left: -9999px;
  opacity: 0;
}

.file-info {
  display: flex;
  flex-direction: column;
  margin-top: 0.5rem;
  font-size: 0.9rem;
  color: #555;
}

/* Styles for version list */
.version-items {
  margin-top: 1rem;
  border: 1px solid #eee;
  border-radius: 4px;
  overflow: hidden;
}

.version-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  border-bottom: 1px solid #eee;
}

.version-item:last-child {
  border-bottom: none;
}

.version-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.version-number {
  font-weight: 600;
  color: var(--primary-color);
}

.version-date,
.version-size {
  color: #666;
  font-size: 0.875rem;
}

.version-actions {
  display: flex;
  gap: 0.5rem;
}

/* Button styles */
.btn-action {
  padding: 0.625rem 1rem;
  border: none;
  border-radius: 4px;
  font-size: 0.95rem;
  cursor: pointer;
  transition: background-color 0.2s;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn-action.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
}

.btn-primary {
  background-color: var(--primary-color);
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background-color: #3854c8;
}

.btn-success {
  background-color: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background-color: #218838;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background-color: #c82333;
}

.btn-warning {
  background-color: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background-color: #e0a800;
}

.btn-action:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 1.5rem;
}

/* Status containers */
.loading-container,
.error-container,
.empty-message,
.loading-message {
  padding: 1.5rem;
  text-align: center;
  border-radius: 8px;
  background-color: #f8f9fa;
}

.error-container {
  border-left: 4px solid var(--danger-color);
  background-color: #fff1f1;
}

.error-message {
  color: var(--danger-color);
  margin-bottom: 1rem;
}

.empty-message,
.loading-message {
  color: #666;
  font-style: italic;
}

/* Notification */
.notification {
  position: relative;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-radius: 4px;
  font-weight: 500;
}

.notification.success {
  background-color: #d4edda;
  color: #155724;
  border-left: 4px solid #28a745;
}

.notification.error {
  background-color: #f8d7da;
  color: #721c24;
  border-left: 4px solid #dc3545;
}

.close-btn {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  background: transparent;
  border: none;
  font-size: 1.25rem;
  cursor: pointer;
  color: inherit;
  opacity: 0.7;
}

.close-btn:hover {
  opacity: 1;
}

@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
    gap: 1.25rem;
  }
  
  .version-item {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .version-actions {
    margin-top: 0.75rem;
    width: 100%;
  }
  
  .btn-action {
    flex: 1;
  }
}

.current-version-info {
  margin-top: 2rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 8px;
  border-left: 4px solid var(--primary-color);
}

.version-badge {
  display: inline-block;
  background-color: var(--primary-color);
  color: white;
  padding: 0.3rem 0.8rem;
  border-radius: 50px;
  font-weight: 600;
  margin: 0.5rem 0;
}

.version-hint {
  margin-top: 0.5rem;
  color: #666;
}

.version-hint a {
  color: var(--primary-color);
  text-decoration: none;
  font-weight: 500;
}

.version-hint a:hover {
  text-decoration: underline;
}

.version-info-panel {
  background-color: #e6f7ff;
  border-left: 4px solid #1890ff;
  padding: 1rem;
  margin-bottom: 1.5rem;
  border-radius: 4px;
}

.version-info-panel p {
  margin: 0.5rem 0;
}

.version-info-panel p:last-child {
  margin-bottom: 0;
}

.version-info-panel strong {
  color: #0366d6;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background-color: #5a6268;
}
</style> 