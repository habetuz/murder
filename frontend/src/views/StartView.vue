<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
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

    <p v-if="errorText" class="status-message error">{{ errorText }}</p>

    <section v-if="!auth.isAuthenticated" class="grid anonymous-grid">
      <section class="panel">
        <h2>Login</h2>
        <form class="form" @submit.prevent="submitLogin">
          <label>
            Username
            <input v-model="loginForm.username" class="text-input" required />
          </label>
          <label>
            Password
            <input v-model="loginForm.password" class="text-input" type="password" required />
          </label>
          <button type="submit" class="game-button primary" :disabled="busy">Login</button>
        </form>
      </section>

      <section class="panel">
        <h2>Register</h2>
        <form class="form" @submit.prevent="submitRegister">
          <label>
            Username
            <input v-model="registerForm.username" class="text-input" required />
          </label>
          <label>
            Password
            <input v-model="registerForm.password" class="text-input" type="password" required />
          </label>
          <label>
            Confirm password
            <input v-model="registerForm.passwordConfirm" class="text-input" type="password" required />
          </label>
          <button type="submit" class="game-button secondary" :disabled="busy">Create user</button>
        </form>
      </section>

      <section class="panel">
        <h2>Join as Guest</h2>
        <form class="form" @submit.prevent="submitGuestJoin">
          <label>
            Alias
            <input v-model="guestForm.name" class="text-input" required />
          </label>
          <label>
            Game code
            <input v-model="guestForm.gameId" class="text-input" required />
          </label>
          <button type="submit" class="game-button warn" :disabled="busy">Join game</button>
        </form>
      </section>
    </section>

    <section v-else class="grid auth-grid">
      <section class="panel">
        <h2>Active Games</h2>
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
      </section>

      <section class="panel">
        <h2>History</h2>
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
      </section>

      <section class="panel">
        <h2>Join or Create</h2>
        <form class="form" @submit.prevent="submitJoinAsUser">
          <label>
            Game code
            <input v-model="joinForm.gameId" class="text-input" required />
          </label>
          <button type="submit" class="game-button primary" :disabled="busy">Join with code</button>
        </form>
        <button class="game-button ghost create-btn" @click="showCreate = true">Create game</button>
      </section>
    </section>

    <div v-if="showCreate" class="overlay" @click.self="showCreate = false">
      <section class="panel modal-panel">
        <h2>Create game</h2>
        <form class="form" @submit.prevent="submitCreateGame">
          <label>
            Name
            <input v-model="createForm.name" class="text-input" required />
          </label>
          <label>
            Description
            <input v-model="createForm.description" class="text-input" />
          </label>
          <label>
            End time
            <input v-model="createForm.endTime" class="text-input" type="datetime-local" />
          </label>
          <div class="modal-actions">
            <button type="button" class="game-button ghost" @click="showCreate = false">Cancel</button>
            <button type="submit" class="game-button primary" :disabled="busy">Create</button>
          </div>
        </form>
      </section>
    </div>
  </main>
</template>

<style scoped>
.start-view {
  display: grid;
  gap: 1.1rem;
}

.hero {
  padding: 0.6rem 0;
  display: grid;
  gap: 0.35rem;
}

.overline {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-size: 0.78rem;
  color: #3b82f6;
  font-weight: 800;
}

h1 {
  margin: 0;
  font-size: clamp(2rem, 3vw, 2.8rem);
  line-height: 1.1;
}

.lead {
  margin: 0;
  max-width: 56ch;
  color: #374151;
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
  font-size: 0.91rem;
  font-weight: 700;
}

.list {
  display: grid;
  gap: 0.55rem;
}

.game-item {
  border: 1px solid #d7e3ff;
  background: linear-gradient(135deg, #ffffff, #f5f9ff);
  border-radius: 0.85rem;
  text-align: left;
  padding: 0.65rem;
  display: grid;
  gap: 0.2rem;
  transition: transform 120ms ease, box-shadow 120ms ease;
}

.game-item:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 20px rgba(37, 99, 235, 0.12);
}

.game-item small,
.muted {
  color: #6b7280;
}

.create-btn {
  margin-top: 0.6rem;
}

.overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.5);
  display: grid;
  place-items: center;
  padding: 1rem;
  z-index: 40;
}

.modal-panel {
  width: min(540px, 100%);
}

.modal-actions {
  display: flex;
  justify-content: end;
  gap: 0.5rem;
}

.panel h2 {
  margin-bottom: 0.7rem;
}

@media (max-width: 700px) {
  .hero {
    gap: 0.2rem;
  }
}
</style>
