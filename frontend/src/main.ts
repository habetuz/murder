import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import { registerUnauthorizedHandler } from './api/client'
import { useAuthStore } from './stores/auth'
import './style.css'

const app = createApp(App)
const pinia = createPinia()
app.use(pinia)
app.use(router)

// Global 401 handler: clear auth state, show flash, redirect to landing
registerUnauthorizedHandler((message: string) => {
  const auth = useAuthStore()
  auth.clearAuthState()
  auth.setFlash(message)
  router.push({ name: 'landing' })
})

app.mount('#app')
