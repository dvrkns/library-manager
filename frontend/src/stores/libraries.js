import { defineStore } from 'pinia'
import api from '@/api'
import axios from 'axios'

export const useLibrariesStore = defineStore('libraries', {
  state: () => ({
    libraries: [],
    library: null,
    languages: [],
    loading: false,
    error: null,
    searchQuery: '',
    sortBy: '-published_date'
  }),
  
  getters: {
    getLibraryById: (state) => (id) => {
      return state.libraries.find(lib => lib.id === parseInt(id))
    }
  },
  
  actions: {
    async fetchLanguages() {
      this.loading = true
      try {
        const response = await api.getLanguages()
        // Проверяем, возвращает ли API результаты с пагинацией
        if (response.data.results) {
          this.languages = response.data.results
          
          // Если есть следующая страница, загружаем все языки
          if (response.data.next) {
            let nextPage = response.data.next
            while (nextPage) {
              const nextResponse = await axios.get(nextPage)
              this.languages = [...this.languages, ...nextResponse.data.results]
              nextPage = nextResponse.data.next
            }
          }
        } else {
          this.languages = response.data
        }
        
        console.log('Загруженные языки:', this.languages)
      } catch (error) {
        this.error = error.message || 'Failed to fetch languages'
        console.error('Ошибка при загрузке языков:', error)
      } finally {
        this.loading = false
      }
    },
    
    async fetchLibraries() {
      this.loading = true
      try {
        const response = await api.getLibraries()
        this.libraries = response.data.results || response.data
      } catch (error) {
        this.error = error.message || 'Failed to fetch libraries'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    
    async fetchLibraryById(id) {
      this.loading = true
      try {
        const response = await api.getLibrary(id)
        this.library = response.data
      } catch (error) {
        this.error = error.message || `Failed to fetch library with ID ${id}`
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    
    async searchLibraries() {
      if (!this.searchQuery.trim()) return
      this.loading = true
      try {
        const response = await api.searchLibraries(
          this.searchQuery, 
          undefined, 
          this.sortBy
        )
        this.libraries = response.data.results || response.data
      } catch (error) {
        this.error = error.message || 'Search failed'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    
    async createLibrary(libraryData) {
      this.loading = true
      try {
        const response = await api.createLibrary(libraryData)
        // Добавляем новую библиотеку в начало списка
        this.libraries = [response.data, ...this.libraries]
        return response.data
      } catch (error) {
        this.error = error.message || 'Failed to create library'
        console.error(error)
        throw error
      } finally {
        this.loading = false
      }
    },
    
    async createLibraryWithFormData(formData) {
      this.loading = true
      try {
        const response = await api.createLibraryWithFormData(formData)
        // Добавляем новую библиотеку в начало списка
        this.libraries = [response.data, ...this.libraries]
        return response.data
      } catch (error) {
        this.error = error.message || 'Failed to create library'
        console.error(error)
        throw error
      } finally {
        this.loading = false
      }
    },
    
    setSearchQuery(query) {
      this.searchQuery = query
    },
    
    setSortBy(sort) {
      this.sortBy = sort
    },
    
    async deleteLibrary(id) {
      this.loading = true
      try {
        await api.deleteLibrary(id)
        // После удаления обновляем список библиотек
        await this.fetchLibraries()
      } catch (error) {
        this.error = error.message || 'Ошибка при удалении библиотеки'
        throw error
      } finally {
        this.loading = false
      }
    }
  }
}) 