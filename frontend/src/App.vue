<template>
  <header>
    <nav class="navbar">
      <div class="container">
        <router-link to="/" class="logo">Library Manager</router-link>
        <div class="nav-links">
          <router-link to="/">Главная</router-link>
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
}

.nav-links a:hover,
.nav-links a.router-link-active {
  color: var(--primary-color);
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
</style> 