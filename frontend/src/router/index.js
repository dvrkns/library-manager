import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
      meta: {
        showSearch: true // Only the home page will have search enabled
      }
    },
    {
      path: '/libraries',
      name: 'all-libraries',
      component: () => import('../views/AllLibrariesView.vue'),
      meta: {
        showSearch: false
      }
    },
    {
      path: '/libraries/:id',
      name: 'library-detail',
      component: () => import('../views/LibraryDetailView.vue'),
      meta: {
        showSearch: false
      }
    },
    {
      path: '/add-library',
      name: 'add-library',
      component: () => import('../views/AddLibraryView.vue'),
      meta: {
        showSearch: false
      }
    },
    {
      path: '/edit-library/:id',
      name: 'edit-library',
      component: () => import('../views/EditLibraryView.vue'),
      meta: {
        showSearch: false
      }
    }
  ]
})

// Add keyboard navigation support
document.addEventListener('keydown', (e) => {
  // Enter key for search submit
  if (e.key === 'Enter' && document.activeElement.id === 'search-input') {
    document.getElementById('search-button')?.click()
  }
  
  // Escape key to unfocus active element
  if (e.key === 'Escape' && document.activeElement) {
    document.activeElement.blur()
  }
})

export default router 