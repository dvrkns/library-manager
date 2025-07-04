import axios from 'axios'

const apiClient = axios.create({
  baseURL: 'http://109.194.11.15:8000/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
})

export default {
  // Languages
  getLanguages() {
    return apiClient.get('/languages/', { params: { limit: 100 } })
  },

  // Libraries
  getLibraries(params = {}) {
    return apiClient.get('/libraries/', { params })
  },
  
  getAllLibraries() {
    // Запрашиваем все библиотеки без пагинации
    return apiClient.get('/libraries/all/')
  },

  getLibrary(id) {
    return apiClient.get(`/libraries/${id}/`)
  },

  searchLibraries(query, language = null, sort = '-published_date') {
    const params = { q: query }
    if (sort) {
      params.sort = sort
    }
    return apiClient.get('/libraries/search/', { params })
  },

  createLibrary(data) {
    return apiClient.post('/libraries/', data)
  },
  
  createLibraryWithFormData(formData) {
    // Создаем новый экземпляр axios с правильными заголовками для FormData
    const formDataClient = axios.create({
      baseURL: 'http://109.194.11.15:8000/api',
      timeout: 30000,
      headers: {
        'Content-Type': 'multipart/form-data',
        'Accept': 'application/json'
      }
    })
    
    return formDataClient.post('/libraries/', formData)
  },

  updateLibrary(id, data) {
    return apiClient.put(`/libraries/${id}/`, data)
  },
  
  updateLibraryWithFormData(id, formData) {
    // Создаем новый экземпляр axios с правильными заголовками для FormData
    const formDataClient = axios.create({
      baseURL: 'http://109.194.11.15:8000/api',
      timeout: 30000,
      headers: {
        'Content-Type': 'multipart/form-data',
        'Accept': 'application/json'
      }
    })
    
    return formDataClient.put(`/libraries/${id}/`, formData)
  },

  deleteLibrary(id) {
    return apiClient.delete(`/libraries/${id}/`)
  },
  
  getLibraryVersions(name) {
    // Получаем все версии библиотеки по имени
    return apiClient.get('/libraries/versions/', { params: { name } })
  }
} 