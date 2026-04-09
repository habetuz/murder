<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
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
      <button v-if="auth.isUser" class="game-button secondary nav-btn" @click="router.push({ name: 'settings' })">
        Settings
      </button>
      <button v-if="auth.isAuthenticated" class="game-button ghost nav-btn" @click="onLogout">
        Logout
      </button>
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
  padding: 0.75rem 1rem;
  border-bottom: 1px solid rgba(77, 119, 219, 0.22);
  background: linear-gradient(120deg, rgba(18, 28, 60, 0.96), rgba(25, 52, 110, 0.94));
  position: sticky;
  top: 0;
  z-index: 20;
}

.brand {
  border: none;
  background: transparent;
  color: #eff6ff;
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
  color: #bae6fd;
}

.brand-mark {
  width: 2.1rem;
  height: 2.1rem;
  border-radius: 0.55rem;
  display: grid;
  place-items: center;
  font-weight: 700;
  font-size: 1.1rem;
  background: linear-gradient(160deg, #fde68a, #f59e0b);
  color: #2a1800;
}

.actions {
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.nav-btn {
  min-height: 2.2rem;
  padding: 0.48rem 0.9rem;
  font-size: 0.84rem;
}

.avatar {
  width: 2rem;
  height: 2rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: #dbeafe;
  color: #0f2e73;
  font-size: 0.85rem;
  font-weight: 700;
}

@media (max-width: 700px) {
  .brand small {
    display: none;
  }
}
</style>
