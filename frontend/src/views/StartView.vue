<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import Card from 'primevue/card';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Message from 'primevue/message';
import Dialog from 'primevue/dialog';
import { useAuthStore } from '../stores/auth';
import { useGamesStore } from '../stores/games';
import { ApiError } from '../types/api';

const auth = useAuthStore();
const games = useGamesStore();
const route = useRoute();
const router = useRouter();

const busy = ref(false);
const errorText = ref('');
const showCreate = ref(false);

const loginForm = reactive({ username: '', password: '' });
const registerForm = reactive({ username: '', password: '', passwordConfirm: '' });
const guestForm = reactive({ name: '', gameId: '' });
const joinForm = reactive({ gameId: '' });
const createForm = reactive({ name: '', description: '', endTime: '' });

const explanation = computed(
  () =>
    'Every player receives one target. Eliminate your assigned victim, inherit their target, and stay alive. Quiet strategy wins.'
);

onMounted(async () => {
  const joinedCode = typeof route.query.join === 'string' ? route.query.join : '';
  if (joinedCode) {
    guestForm.gameId = joinedCode;
    joinForm.gameId = joinedCode;
  }

  await auth.bootstrap();
  if (auth.isAuthenticated) {
    await games.fetchMyGames();
  }
});

async function runAction(action: () => Promise<void>) {
  busy.value = true;
  errorText.value = '';
  try {
    await action();
  } catch (error) {
    if (error instanceof ApiError) {
      errorText.value = error.problem?.detail ?? error.message;
    } else if (error instanceof Error) {
      errorText.value = error.message;
    } else {
      errorText.value = 'Unexpected error.';
    }
  } finally {
    busy.value = false;
  }
}

function normalizeEndTime(value: string) {
  return value ? new Date(value).toISOString() : undefined;
}

async function submitLogin() {
  await runAction(async () => {
    await auth.login(loginForm.username, loginForm.password);
    await games.fetchMyGames();
    loginForm.password = '';
  });
}

async function submitRegister() {
  await runAction(async () => {
    if (registerForm.password !== registerForm.passwordConfirm) {
      throw new Error('Password confirmation does not match.');
    }
    await auth.register(registerForm.username);
    await games.api.setPassword(registerForm.password);
    registerForm.password = '';
    registerForm.passwordConfirm = '';
    await games.fetchMyGames();
  });
}

async function submitGuestJoin() {
  await runAction(async () => {
    await auth.createGuest(guestForm.name, guestForm.gameId);
    await games.fetchMyGames();
    await router.push({ name: 'game', params: { gameId: guestForm.gameId } });
  });
}

async function submitJoinAsUser() {
  await runAction(async () => {
    await games.joinGame(joinForm.gameId);
    await router.push({ name: 'game', params: { gameId: joinForm.gameId } });
  });
}

async function submitCreateGame() {
  await runAction(async () => {
    const game = await games.createGame({
      name: createForm.name,
      description: createForm.description || undefined,
      endTime: normalizeEndTime(createForm.endTime),
    });
    showCreate.value = false;
    await router.push({ name: 'game', params: { gameId: game.id } });
  });
}

function openGame(gameId: string) {
  router.push({ name: 'game', params: { gameId } });
}
</script>

<template>
  <main class="start-view">
    <section class="hero">
      <p class="overline">Private Party Game</p>
      <h1>Hunt Quietly. Survive Longer.</h1>
      <p class="lead">{{ explanation }}</p>
    </section>

    <Message v-if="errorText" severity="error" :closable="false">{{ errorText }}</Message>

    <section v-if="!auth.isAuthenticated" class="grid anonymous-grid">
      <Card>
        <template #title>Login</template>
        <template #content>
          <form class="form" @submit.prevent="submitLogin">
            <label>
              Username
              <InputText v-model="loginForm.username" class="full" required />
            </label>
            <label>
              Password
              <Password v-model="loginForm.password" :feedback="false" toggleMask class="full" required />
            </label>
            <Button type="submit" label="Login" :loading="busy" />
          </form>
        </template>
      </Card>

      <Card>
        <template #title>Register</template>
        <template #content>
          <form class="form" @submit.prevent="submitRegister">
            <label>
              Username
              <InputText v-model="registerForm.username" class="full" required />
            </label>
            <label>
              Password
              <Password v-model="registerForm.password" :feedback="false" toggleMask class="full" required />
            </label>
            <label>
              Confirm password
              <Password v-model="registerForm.passwordConfirm" :feedback="false" toggleMask class="full" required />
            </label>
            <Button type="submit" label="Create user" severity="contrast" :loading="busy" />
          </form>
        </template>
      </Card>

      <Card>
        <template #title>Join as Guest</template>
        <template #content>
          <form class="form" @submit.prevent="submitGuestJoin">
            <label>
              Alias
              <InputText v-model="guestForm.name" class="full" required />
            </label>
            <label>
              Game code
              <InputText v-model="guestForm.gameId" class="full" required />
            </label>
            <Button type="submit" label="Join game" severity="warn" :loading="busy" />
          </form>
        </template>
      </Card>
    </section>

    <section v-else class="grid auth-grid">
      <Card>
        <template #title>Active Games</template>
        <template #content>
          <div class="list">
            <button
              v-for="game in games.activeGames"
              :key="game.id"
              type="button"
              class="game-item"
              @click="openGame(game.id)"
            >
              <strong>{{ game.name }}</strong>
              <small>{{ game.id }} · {{ game.state }}</small>
            </button>
            <p v-if="!games.activeGames.length" class="muted">No active games yet.</p>
          </div>
        </template>
      </Card>

      <Card>
        <template #title>History</template>
        <template #content>
          <div class="list">
            <button
              v-for="game in games.historyGames"
              :key="game.id"
              type="button"
              class="game-item"
              @click="openGame(game.id)"
            >
              <strong>{{ game.name }}</strong>
              <small>{{ game.id }} · ended</small>
            </button>
            <p v-if="!games.historyGames.length" class="muted">No finished games.</p>
          </div>
        </template>
      </Card>

      <Card>
        <template #title>Join or Create</template>
        <template #content>
          <form class="form" @submit.prevent="submitJoinAsUser">
            <label>
              Game code
              <InputText v-model="joinForm.gameId" class="full" required />
            </label>
            <Button type="submit" label="Join with code" :loading="busy" />
          </form>
          <Button label="Create game" severity="contrast" outlined @click="showCreate = true" />
        </template>
      </Card>
    </section>

    <Dialog v-model:visible="showCreate" modal header="Create game" class="create-dialog">
      <form class="form" @submit.prevent="submitCreateGame">
        <label>
          Name
          <InputText v-model="createForm.name" class="full" required />
        </label>
        <label>
          Description
          <InputText v-model="createForm.description" class="full" />
        </label>
        <label>
          End time
          <input v-model="createForm.endTime" class="native-input" type="datetime-local" />
        </label>
        <Button type="submit" label="Create" :loading="busy" />
      </form>
    </Dialog>
  </main>
</template>

<style scoped>
.start-view {
  display: grid;
  gap: 1rem;
}

.hero {
  padding: 0.75rem 0;
}

.overline {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-size: 0.75rem;
  color: #7e5f45;
}

h1 {
  margin: 0.35rem 0;
  font-size: clamp(1.8rem, 3vw, 2.6rem);
  line-height: 1.1;
}

.lead {
  margin: 0;
  max-width: 56ch;
  color: #4f3427;
}

.grid {
  display: grid;
  gap: 1rem;
}

.anonymous-grid,
.auth-grid {
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.form {
  display: grid;
  gap: 0.75rem;
}

label {
  display: grid;
  gap: 0.35rem;
  font-size: 0.92rem;
}

.full,
:deep(.full input) {
  width: 100%;
}

.list {
  display: grid;
  gap: 0.55rem;
}

.game-item {
  border: 1px solid rgba(62, 41, 28, 0.2);
  background: rgba(252, 246, 236, 0.7);
  border-radius: 0.7rem;
  text-align: left;
  padding: 0.6rem;
  display: grid;
  gap: 0.15rem;
}

.game-item small,
.muted {
  color: #6b4d3a;
}

.native-input {
  width: 100%;
  border: 1px solid #cbb59c;
  border-radius: 0.6rem;
  padding: 0.6rem;
  font: inherit;
}

@media (max-width: 700px) {
  .lead {
    font-size: 0.95rem;
  }
}
</style>
