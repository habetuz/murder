import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api, ApiError } from '../api/client'
import type { PlayerRef } from '../types/api'

export const useAuthStore = defineStore('auth', () => {
  const initialized = ref(false)
  const loading = ref(false)
  const player = ref<PlayerRef | null>(null)
  const sessionExpiresAt = ref<string | null>(null)
  const flashMessage = ref<string | null>(null)

  const isAuthenticated = computed(() => player.value !== null)
  const isUser = computed(() => player.value?.kind === 'user')
  const isGuest = computed(() => player.value?.kind === 'guest')

  async function bootstrap() {
    if (initialized.value) return
    initialized.value = true
    try {
      const res = await api.getSession()
      player.value = res.player
      sessionExpiresAt.value = res.session.expiresAt
    } catch {
      player.value = null
      sessionExpiresAt.value = null
    }
  }

  async function login(username: string, password: string) {
    loading.value = true
    try {
      await api.login({ type: 'password', username, password })
      const res = await api.getSession()
      player.value = res.player
      sessionExpiresAt.value = res.session.expiresAt
    } finally {
      loading.value = false
    }
  }

  async function register(name: string, password?: string) {
    loading.value = true
    try {
      await api.createUser({ name })
      if (password) {
        await api.createPassword(password)
      }
      const res = await api.getSession()
      player.value = res.player
      sessionExpiresAt.value = res.session.expiresAt
    } finally {
      loading.value = false
    }
  }

  async function createGuest(name: string, gameId: string): Promise<string> {
    loading.value = true
    try {
      const res = await api.createGuest({ name, gameId })
      const session = await api.getSession()
      player.value = session.player
      sessionExpiresAt.value = session.session.expiresAt
      return res.joinedGameId
    } finally {
      loading.value = false
    }
  }

  async function logout() {
    try {
      await api.logout()
    } catch (e) {
      if (!(e instanceof ApiError && e.status === 401)) throw e
    } finally {
      clearAuthState()
    }
  }

  function clearAuthState() {
    player.value = null
    sessionExpiresAt.value = null
    initialized.value = false
  }

  function setFlash(message: string) {
    flashMessage.value = message
  }

  function clearFlash() {
    flashMessage.value = null
  }

  return {
    initialized,
    loading,
    player,
    sessionExpiresAt,
    flashMessage,
    isAuthenticated,
    isUser,
    isGuest,
    bootstrap,
    login,
    register,
    createGuest,
    logout,
    clearAuthState,
    setFlash,
    clearFlash,
  }
})
