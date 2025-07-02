import axios from 'axios'

const apiClient = axios.create({
  baseURL: '/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
})

export default {
  // Languages
  getLanguages() {
    return apiClient.get('/languages/')
  },

  // Libraries
  getLibraries(params = {}) {
    return apiClient.get('/libraries/', { params })
  },

  getLibrary(id) {
    return apiClient.get(`/libraries/${id}/`)
  },

  searchLibraries(query, language = null, sort = '-published_date') {
    const params = { q: query }
    
    if (language) {
      params.lang = language
    }
    
    if (sort) {
      params.sort = sort
    }
    
    return apiClient.get('/libraries/search/', { params })
  },

  createLibrary(data) {
    return apiClient.post('/libraries/', data)
  },

  updateLibrary(id, data) {
    return apiClient.put(`/libraries/${id}/`, data)
  },

  deleteLibrary(id) {
    return apiClient.delete(`/libraries/${id}/`)
  }
} 