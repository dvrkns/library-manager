import { defineStore } from 'pinia'
import api from '@/api'

export const useLibrariesStore = defineStore('libraries', {
  state: () => ({
    libraries: [],
    library: null,
    languages: [],
    loading: false,
    error: null,
    searchQuery: '',
    selectedLanguage: null,
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
        this.languages = response.data
      } catch (error) {
        this.error = error.message || 'Failed to fetch languages'
        console.error(error)
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
          this.selectedLanguage, 
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
    
    setSearchQuery(query) {
      this.searchQuery = query
    },
    
    setSelectedLanguage(language) {
      this.selectedLanguage = language
    },
    
    setSortBy(sort) {
      this.sortBy = sort
    }
  }
}) 