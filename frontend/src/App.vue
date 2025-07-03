<template>
  <header>
    <nav class="navbar">
      <div class="container">
        <router-link to="/" class="logo">Library Manager</router-link>
        <div class="nav-links">
          <router-link to="/">Главная</router-link>
          <router-link to="/libraries">Все библиотеки</router-link>
          <router-link to="/add-library">Добавить библиотеку</router-link>
        </div>
      </div>
    </nav>
  </header>

  <main class="container">
    <router-view />
  </main>

  <footer class="footer">
    <div class="container">
      <p>&copy; {{ new Date().getFullYear() }} Library Manager</p>
    </div>
  </footer>
</template>

<script setup>
// Component setup
</script>

<style>
/* Base styles */
:root {
  --primary-color: #4361ee;
  --text-color: #333;
  --background-color: #f8f9fa;
  --border-color: #e9ecef;
  --success-color: #2e7d32;
  --danger-color: #c62828;
  --warning-color: #f57c00;
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  line-height: 1.6;
  color: var(--text-color);
  background-color: var(--background-color);
}

.container {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 15px;
}

/* Header styles */
.navbar {
  display: flex;
  align-items: center;
  background-color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 1rem 0;
}

.navbar .container {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--primary-color);
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: 1.5rem;
}

.nav-links a {
  color: var(--text-color);
  text-decoration: none;
  transition: color 0.3s;
  padding: 0.5rem 0;
  position: relative;
}

.nav-links a:hover,
.nav-links a.router-link-active {
  color: var(--primary-color);
}

.nav-links a.router-link-active:after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background-color: var(--primary-color);
}

/* Main content */
main {
  min-height: calc(100vh - 150px);
  padding: 2rem 0;
}

/* Footer styles */
.footer {
  background-color: white;
  border-top: 1px solid var(--border-color);
  padding: 1.5rem 0;
  text-align: center;
}

/* Action buttons */
.btn-action {
  padding: 0.5rem 0.75rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  border: none;
  color: white;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;
}

.btn-primary {
  background-color: var(--primary-color);
}

.btn-primary:hover {
  background-color: #3050d8;
}

.btn-success {
  background-color: var(--success-color);
}

.btn-success:hover {
  background-color: #1b5e20;
}

.btn-danger {
  background-color: var(--danger-color);
}

.btn-danger:hover {
  background-color: #b71c1c;
}

.btn-warning {
  background-color: var(--warning-color);
}

.btn-warning:hover {
  background-color: #ef6c00;
}
</style> 