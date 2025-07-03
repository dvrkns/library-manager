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
    
    <div v-else-if="error" class="error-container">
      <p class="error-message">{{ error }}</p>
      <button class="btn-action btn-primary" @click="fetchLibrary">Повторить попытку</button>
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
          <label for="edit-language">Язык программирования:</label>
          <select id="edit-language" v-model="editForm.language">
            <option v-for="lang in languages" :key="lang.id" :value="lang.id">
              {{ lang.name }}
            </option>
          </select>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="edit-author">Автор:</label>
            <input 
              type="text" 
              id="edit-author" 
              v-model="editForm.author" 
              placeholder="Автор библиотеки"
            />
          </div>
        </div>
        
        <div class="form-row">
          <div class="form-group">
            <label for="edit-homepage">Домашняя страница:</label>
            <input 
              type="url" 
              id="edit-homepage" 
              v-model="editForm.homepage" 
              placeholder="https://example.com"
            />
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
        </div>
        
        <div class="form-actions">
          <button class="btn-action btn-success" @click="updateLibrary" :disabled="updating">
            {{ updating ? 'Сохранение...' : 'Сохранить изменения' }}
          </button>
          <button class="btn-action btn-danger" @click="confirmDelete">
            Удалить библиотеку
          </button>
        </div>
      </div>
      
      <!-- Управление версиями библиотеки -->
      <div v-if="activeTab === 'versions'" class="versions-panel card">
        <h2 class="form-title">Управление версиями</h2>
        
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
            <label for="new-version">Номер версии: <span class="required">*</span></label>
            <input 
              type="text" 
              id="new-version" 
              v-model="newVersionForm.version" 
              placeholder="Например: 1.0.1"
              required
            />
          </div>
          
          <div class="form-group">
            <label for="new-published-date">Дата публикации: <span class="required">*</span></label>
            <input 
              type="date" 
              id="new-published-date" 
              v-model="newVersionForm.published_date" 
              required
            />
          </div>
          
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
                required
              />
              <div class="file-info" v-if="newVersionForm.file">
                <span>Выбран файл: {{ newVersionForm.file.name }}</span>
                <span>({{ formatFileSize(newVersionForm.file.size) }})</span>
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
  author: '',
  language: null,
  homepage: '',
  repository: ''
});

// Форма добавления новой версии
const newVersionForm = reactive({
  version: '',
  published_date: new Date().toISOString().slice(0, 10),
  file: null,
  download_url: ''
});

// Проверка валидности формы для новой версии
const isNewVersionFormValid = computed(() => {
  return (
    newVersionForm.version &&
    newVersionForm.published_date &&
    (newVersionForm.file || newVersionForm.download_url)
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
      editForm.author = library.value.author || '';
      editForm.language = library.value.language;
      editForm.homepage = library.value.homepage || '';
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
    versions.value = response;
  } catch (err) {
    console.error('Ошибка при загрузке версий библиотеки:', err);
  } finally {
    loadingVersions.value = false;
  }
};

// Обработчики файлов
const handleNewFileChange = (event) => {
  const file = event.target.files[0];
  if (file) {
    newVersionForm.file = file;
  }
};

// Сохранение изменений основной информации
const updateLibrary = async () => {
  if (!library.value) return;
  
  updating.value = true;
  error.value = null;
  
  try {
    // Создание данных для обновления
    const updateData = {
      name: editForm.name,
      description: editForm.description,
      author: editForm.author,
      language: editForm.language,
      homepage: editForm.homepage,
      repository: editForm.repository,
      // Сохраняем текущую версию и файл
      version: library.value.version,
      file: library.value.file,
      download_url: library.value.download_url,
      published_date: library.value.published_date
    };
    
    // Обновляем библиотеку
    await store.updateLibrary(library.value.id, updateData);
    
    // Обновляем все версии с новым названием, если оно изменилось
    if (editForm.name !== library.value.name) {
      // Здесь можно добавить код для обновления всех версий,
      // но это потребует дополнительной поддержки на бэкенде
    }
    
    router.push({
      path: '/libraries',
      query: {
        message: 'Библиотека успешно обновлена',
        type: 'success'
      }
    });
  } catch (err) {
    error.value = 'Не удалось обновить библиотеку. ' + (err.response?.data?.detail || '');
    console.error('Ошибка при обновлении библиотеки:', err);
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
    formData.append('author', library.value.author || '');
    formData.append('language', library.value.language);
    formData.append('homepage', library.value.homepage || '');
    formData.append('repository', library.value.repository || '');
    formData.append('published_date', newVersionForm.published_date);
    
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
    error.value = 'Не удалось добавить новую версию. ' + (err.response?.data?.detail || '');
    console.error('Ошибка при добавлении новой версии:', err);
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
    alert('Нельзя удалить единственную версию библиотеки. Удалите всю библиотеку.');
    return;
  }
  
  if (confirm(`Вы действительно хотите удалить версию ${version.version}?`)) {
    try {
      await store.deleteLibrary(version.id);
      
      // Обновляем список версий
      await fetchVersions(library.value.name);
      
      // Показываем уведомление
      error.value = { type: 'success', message: `Версия ${version.version} успешно удалена` };
    } catch (err) {
      error.value = 'Не удалось удалить версию библиотеки.';
      console.error('Ошибка при удалении версии:', err);
    }
  }
};

// Сброс формы новой версии
const resetNewVersionForm = () => {
  newVersionForm.version = '';
  newVersionForm.published_date = new Date().toISOString().slice(0, 10);
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
  color: #333;
  border-bottom: 1px solid var(--border-color);
  padding-bottom: 0.5rem;
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
  color: var(--danger-color);
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

.form-help {
  display: block;
  font-size: 0.85rem;
  color: #777;
  margin-top: 0.35rem;
}

.file-upload {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.file-label {
  display: inline-block;
  padding: 0.5rem 1rem;
  background-color: #f1f5ff;
  border: 1px solid #e1e7ff;
  color: var(--primary-color);
  border-radius: 6px;
  cursor: pointer;
  transition: background-color 0.2s;
}

.file-label:hover {
  background-color: #e9efff;
}

.file-input {
  position: absolute;
  width: 0.1px;
  height: 0.1px;
  opacity: 0;
  overflow: hidden;
  z-index: -1;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-top: 0.5rem;
  color: #666;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.85rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
  margin-top: 2rem;
}

.loading-container,
.error-container {
  padding: 2rem;
  text-align: center;
  background: white;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.error-message {
  color: var(--danger-color);
  margin-bottom: 1rem;
}

/* Стили для управления версиями */
.versions-list {
  margin-bottom: 2rem;
}

.loading-message,
.empty-message {
  text-align: center;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 6px;
  color: #666;
}

.version-items {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.version-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 1rem;
  background-color: #f8f9fa;
  border-radius: 6px;
  border-left: 3px solid var(--primary-color);
}

.version-info {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.version-number {
  font-weight: 600;
  color: var(--primary-color);
}

.version-date,
.version-size {
  color: #666;
  font-size: 0.9rem;
}

.version-actions {
  display: flex;
  gap: 0.5rem;
}

.add-version-form {
  border-top: 1px solid var(--border-color);
  padding-top: 1.5rem;
}

@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
    gap: 1.25rem;
  }
  
  .form-actions {
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .version-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
  
  .version-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.25rem;
  }
}
</style> 