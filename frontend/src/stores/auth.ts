import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { api } from '../api/client';
import { ApiError, type PlayerProfile, type SessionPayload } from '../types/api';

export const useAuthStore = defineStore('auth', () => {
  const initialized = ref(false);
  const loading = ref(false);
  const player = ref<PlayerProfile | null>(null);
  const session = ref<SessionPayload['session'] | null>(null);

  const isAuthenticated = computed(() => !!player.value);
  const isUser = computed(() => player.value?.kind === 'user');

  async function bootstrap() {
    if (initialized.value) {
      return;
    }

    loading.value = true;
    try {
      const payload = await api.session();
      const me = await api.getMe();
      player.value = me.player;
      session.value = payload.session;
    } catch (error) {
      if (!(error instanceof ApiError) || error.status !== 401) {
        console.error(error);
      }
      player.value = null;
      session.value = null;
    } finally {
      initialized.value = true;
      loading.value = false;
    }
  }

  async function refreshMe() {
    const me = await api.getMe();
    player.value = me.player;
  }

  async function login(username: string, password: string) {
    await api.login({ type: 'password', username, password });
    await bootstrapAfterAuth();
  }

  async function register(name: string) {
    await api.createUser({ name });
    await bootstrapAfterAuth();
  }

  async function createGuest(name: string, gameId: string) {
    await api.createGuest({ name, gameId });
    await bootstrapAfterAuth();
  }

  async function logout() {
    await api.logout();
    clearAuthState();
  }

  function clearAuthState() {
    player.value = null;
    session.value = null;
    initialized.value = true;
  }

  async function bootstrapAfterAuth() {
    const payload = await api.session();
    const me = await api.getMe();
    player.value = me.player;
    session.value = payload.session;
    initialized.value = true;
  }

  return {
    initialized,
    loading,
    player,
    session,
    isAuthenticated,
    isUser,
    bootstrap,
    refreshMe,
    login,
    register,
    createGuest,
    logout,
    clearAuthState,
  };
});
