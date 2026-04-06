<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import Button from 'primevue/button';
import { useAuthStore } from '../../stores/auth';

const auth = useAuthStore();
const router = useRouter();
const initials = computed(() => auth.player?.name.slice(0, 1).toUpperCase() ?? '?');

async function onLogout() {
  await auth.logout();
  await router.push({ name: 'start' });
}
</script>

<template>
  <header class="app-header">
    <button class="brand" type="button" @click="router.push({ name: 'start' })">
      <span class="brand-mark">M</span>
      <div>
        <p>Murder</p>
        <small>Detective Party Game</small>
      </div>
    </button>

    <nav class="actions">
      <Button
        v-if="auth.isUser"
        label="Settings"
        size="small"
        severity="contrast"
        outlined
        @click="router.push({ name: 'settings' })"
      />
      <Button
        v-if="auth.isAuthenticated"
        label="Logout"
        size="small"
        severity="danger"
        text
        @click="onLogout"
      />
      <div v-if="auth.isAuthenticated" class="avatar" :title="auth.player?.name">
        {{ initials }}
      </div>
    </nav>
  </header>
</template>

<style scoped>
.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.8rem 1rem;
  border-bottom: 1px solid rgba(80, 56, 42, 0.24);
  background: linear-gradient(120deg, rgba(26, 16, 14, 0.96), rgba(63, 34, 24, 0.96));
  position: sticky;
  top: 0;
  z-index: 20;
}

.brand {
  border: none;
  background: transparent;
  color: #f9eddd;
  display: flex;
  gap: 0.7rem;
  align-items: center;
  text-align: left;
}

.brand p {
  margin: 0;
  font-size: 1.15rem;
  letter-spacing: 0.04em;
}

.brand small {
  color: #dec7ae;
}

.brand-mark {
  width: 2.1rem;
  height: 2.1rem;
  border-radius: 0.55rem;
  display: grid;
  place-items: center;
  font-weight: 700;
  font-size: 1.1rem;
  background: linear-gradient(160deg, #f8e9cd, #a9805a);
  color: #23110c;
}

.actions {
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: #f6e8cf;
  color: #2d160f;
  font-size: 0.85rem;
  font-weight: 700;
}
</style>
