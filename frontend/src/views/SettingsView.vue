<script setup lang="ts">
import { onMounted, ref } from 'vue';
import Card from 'primevue/card';
import Button from 'primevue/button';
import Password from 'primevue/password';
import Message from 'primevue/message';
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

    <Message v-if="errorText" severity="error" :closable="false">{{ errorText }}</Message>
    <Message v-if="successText" severity="success" :closable="false">{{ successText }}</Message>

    <section class="grid">
      <Card>
        <template #title>Password</template>
        <template #content>
          <div class="stack">
            <Password v-model="password" :feedback="false" toggleMask class="full" placeholder="New password" />
            <Button label="Save password" :loading="loading" @click="savePassword" />
          </div>
        </template>
      </Card>

      <Card>
        <template #title>Credentials</template>
        <template #content>
          <ul class="credentials">
            <li v-for="credential in credentials" :key="credential.id">
              <div>
                <strong>{{ credential.type }}</strong>
                <p>{{ credential.id }}</p>
              </div>
              <Button label="Revoke" text severity="danger" @click="revoke(credential.id)" :loading="loading" />
            </li>
            <li v-if="!credentials.length" class="muted">No credentials found.</li>
          </ul>
          <Button label="Revoke all sessions" severity="contrast" outlined @click="revokeSessions" :loading="loading" />
        </template>
      </Card>
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
  color: #7e5f45;
}

h1 {
  margin: 0.2rem 0;
}

.lead {
  margin: 0;
  color: #553826;
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
  border: 1px solid rgba(70, 46, 31, 0.2);
  border-radius: 0.65rem;
  padding: 0.55rem 0.65rem;
  background: rgba(252, 246, 236, 0.6);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
}

.credentials p {
  margin: 0;
  color: #6f4c38;
  font-size: 0.84rem;
}

.full,
:deep(.full input) {
  width: 100%;
}

.muted {
  color: #6f4c38;
}
</style>
