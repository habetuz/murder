<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useGamesStore } from '../stores/games';
import { ApiError, type CredentialDto } from '../types/api';

const auth = useAuthStore();
const games = useGamesStore();

const credentials = ref<CredentialDto[]>([]);
const password = ref('');
const loading = ref(false);
const errorText = ref('');
const successText = ref('');

onMounted(async () => {
  await refresh();
});

async function guarded(action: () => Promise<void>) {
  loading.value = true;
  errorText.value = '';
  successText.value = '';
  try {
    await action();
  } catch (error) {
    if (error instanceof ApiError) {
      errorText.value = error.problem?.detail ?? error.message;
    } else {
      errorText.value = 'Unexpected error.';
    }
  } finally {
    loading.value = false;
  }
}

async function refresh() {
  await guarded(async () => {
    await auth.refreshMe();
    credentials.value = await games.api.getCredentials();
  });
}

async function savePassword() {
  await guarded(async () => {
    await games.api.setPassword(password.value);
    password.value = '';
    successText.value = 'Password updated.';
    credentials.value = await games.api.getCredentials();
  });
}

async function revoke(id: string) {
  await guarded(async () => {
    await games.api.revokeCredential(id);
    credentials.value = await games.api.getCredentials();
  });
}

async function revokeSessions() {
  await guarded(async () => {
    await games.api.revokeSessions();
    successText.value = 'All sessions revoked. Please login again if needed.';
    credentials.value = [];
  });
}
</script>

<template>
  <main class="settings-view">
    <section class="hero">
      <p class="overline">User Settings</p>
      <h1>{{ auth.player?.name }}</h1>
      <p class="lead">Manage account credentials and sessions.</p>
    </section>

    <p v-if="errorText" class="status-message error">{{ errorText }}</p>
    <p v-if="successText" class="status-message success">{{ successText }}</p>

    <section class="grid">
      <section class="panel">
        <h2>Password</h2>
        <div class="stack">
          <input v-model="password" class="text-input" type="password" placeholder="New password" />
          <button class="game-button primary" :disabled="loading" @click="savePassword">Save password</button>
        </div>
      </section>

      <section class="panel">
        <h2>Credentials</h2>
        <ul class="credentials">
          <li v-for="credential in credentials" :key="credential.id">
            <div>
              <strong>{{ credential.type }}</strong>
              <p>{{ credential.id }}</p>
            </div>
            <button class="game-button danger compact" :disabled="loading" @click="revoke(credential.id)">Revoke</button>
          </li>
          <li v-if="!credentials.length" class="muted">No credentials found.</li>
        </ul>
        <button class="game-button ghost" :disabled="loading" @click="revokeSessions">Revoke all sessions</button>
      </section>
    </section>
  </main>
</template>

<style scoped>
.settings-view {
  display: grid;
  gap: 1rem;
}

.overline {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-size: 0.76rem;
  color: #3b82f6;
  font-weight: 800;
}

h1 {
  margin: 0.2rem 0;
}

.lead {
  margin: 0;
  color: #4b5563;
}

.grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.stack {
  display: grid;
  gap: 0.65rem;
}

.credentials {
  margin: 0 0 0.8rem;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.5rem;
}

.credentials li {
  border: 1px solid #dbeafe;
  border-radius: 0.75rem;
  padding: 0.55rem 0.65rem;
  background: linear-gradient(135deg, #ffffff, #f5f9ff);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
}

.credentials p {
  margin: 0;
  color: #475569;
  font-size: 0.84rem;
}

.muted {
  color: #64748b;
}

.compact {
  padding: 0.4rem 0.65rem;
  min-height: 0;
}
</style>
