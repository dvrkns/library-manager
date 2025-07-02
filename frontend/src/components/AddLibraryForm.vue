<template>
  <div class="add-library-form">
    <h2>Добавить новую библиотеку</h2>
    
    <div v-if="error" class="error-message">
      {{ error }}
    </div>
    
    <form @submit.prevent="submitForm">
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
        <label for="language">Язык программирования*</label>
        <select 
          id="language" 
          v-model="form.language" 
          required
        >
          <option value="" disabled>Выберите язык</option>
          <option 
            v-for="language in languages" 
            :key="language.id" 
            :value="language.id"
          >
            {{ language.name }}
          </option>
        </select>
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
  language: '',
  author: '',
  homepage: '',
  repository: '',
  published_date: new Date().toISOString().split('T')[0] // Текущая дата в формате YYYY-MM-DD
});

const loading = ref(false);
const error = ref('');

// Получаем список языков из хранилища
const languages = computed(() => store.languages);

// Загружаем языки при монтировании компонента
onMounted(async () => {
  if (!languages.value.length) {
    await store.fetchLanguages();
  }
});

// Отправка формы
const submitForm = async () => {
  loading.value = true;
  error.value = '';
  
  try {
    await store.createLibrary(form);
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
    } else {
      form[key] = '';
    }
  });
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

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
</style> 