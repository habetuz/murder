<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import QRCode from 'qrcode';
import { useAuthStore } from '../stores/auth';
import { useGamesStore } from '../stores/games';
import { ApiError, type GameDto, type LeaderboardEntry, type ParticipantDto } from '../types/api';
import VictimRevealPanel from '../components/game/VictimRevealPanel.vue';
import CountdownBadge from '../components/game/CountdownBadge.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const games = useGamesStore();

const game = ref<GameDto | null>(null);
const participants = ref<ParticipantDto[]>([]);
const entries = ref<LeaderboardEntry[]>([]);
const victimPlayerId = ref<string | null>(null);

const loading = ref(false);
const errorText = ref('');
const endTimeInput = ref('');
const killTarget = ref('');
const qrCodeDataUrl = ref('');
const joinUrl = ref('');

let poller: number | null = null;

const gameId = computed(() => route.params.gameId as string);
const isAdmin = computed(() => !!auth.player && auth.player.id === game.value?.adminPlayerId);

const leaderboardRows = computed(() => {
  const names = new Map(participants.value.map((participant) => [participant.id, participant.name]));
  return entries.value.map((entry) => ({
    playerId: entry.playerId,
    playerName: names.get(entry.playerId) ?? entry.playerId,
    kills: entry.kills,
  }));
});

const participantLookup = computed(() => new Map(participants.value.map((participant) => [participant.id, participant.name])));
const victimName = computed(() => {
  if (!victimPlayerId.value) {
    return null;
  }

  return participantLookup.value.get(victimPlayerId.value) ?? victimPlayerId.value;
});
const canEditEndTime = computed(() => game.value?.state === 'pending' && isAdmin.value);

watch(
  () => game.value?.id,
  async (id) => {
    if (!id || typeof window === 'undefined') {
      qrCodeDataUrl.value = '';
      return;
    }

    joinUrl.value = `${window.location.origin}/?join=${encodeURIComponent(id)}`;
    qrCodeDataUrl.value = await QRCode.toDataURL(joinUrl.value, {
      width: 240,
      margin: 1,
      color: {
        dark: '#2a1a12',
        light: '#f6efdf',
      },
    });
  },
  { immediate: true }
);

onMounted(async () => {
  await refreshAll();
  startPolling();
});

onUnmounted(() => {
  if (poller !== null) {
    window.clearInterval(poller);
  }
});

async function guarded(action: () => Promise<void>) {
  loading.value = true;
  errorText.value = '';
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

async function refreshAll() {
  await guarded(async () => {
    const [gameData, participantData] = await Promise.all([
      games.getGame(gameId.value),
      games.getParticipants(gameId.value),
    ]);

    game.value = gameData;
    participants.value = participantData;

    if (game.value.endTime) {
      endTimeInput.value = game.value.endTime.slice(0, 16);
    }

    if (game.value.state !== 'pending') {
      entries.value = await games.getLeaderboard(gameId.value);
    } else {
      entries.value = [];
    }

    if (game.value.state === 'running') {
      victimPlayerId.value = await games.getVictim(gameId.value);
      if (!killTarget.value) {
        killTarget.value = victimPlayerId.value;
      }
    } else {
      victimPlayerId.value = null;
      killTarget.value = '';
    }
  });
}

function startPolling() {
  poller = window.setInterval(async () => {
    if (document.visibilityState === 'hidden') {
      return;
    }

    if (game.value?.state === 'running') {
      await refreshAll();
    }
  }, 4000);
}

function toIsoFromInput(value: string) {
  return value ? new Date(value).toISOString() : undefined;
}

async function patchEndTime() {
  await guarded(async () => {
    if (!endTimeInput.value) {
      return;
    }
    await games.api.changeEnd(gameId.value, toIsoFromInput(endTimeInput.value)!);
    await refreshAll();
  });
}

async function startGame() {
  await guarded(async () => {
    await games.api.startGame(gameId.value, { endTime: toIsoFromInput(endTimeInput.value) });
    await refreshAll();
  });
}

async function endGame() {
  await guarded(async () => {
    await games.api.endGame(gameId.value);
    await refreshAll();
  });
}

async function leaveGame() {
  await guarded(async () => {
    const isGuest = auth.player?.kind === 'guest';
    await games.leaveGame(gameId.value, !isGuest);
    if (isGuest) {
      auth.clearAuthState();
    }
    await router.push({ name: 'start' });
  });
}

async function submitKill() {
  await guarded(async () => {
    await games.api.kill(gameId.value, killTarget.value);
    await refreshAll();
  });
}
</script>

<template>
  <main v-if="game" class="game-view">
    <section class="panel game-head">
      <div>
        <p class="overline">Game Code {{ game.id }}</p>
        <h1>{{ game.name }}</h1>
        <p class="description">{{ game.description || 'No description' }}</p>
      </div>
      <div class="head-right">
        <span class="badge-chip">{{ game.state }}</span>
        <CountdownBadge :end-time="game.endTime" />
      </div>
    </section>

    <p v-if="errorText" class="status-message error">{{ errorText }}</p>

    <section class="layout-grid">
      <section class="panel">
        <h2>Participants</h2>
        <ul class="participants">
          <li v-for="player in participants" :key="player.id">
            <span>{{ player.name }}</span>
            <small>{{ player.kind }}</small>
          </li>
        </ul>
      </section>

      <section v-if="game.state === 'pending'" class="panel">
        <h2>Pregame</h2>
        <div class="stack">
          <div class="join-code-block">
            <p class="join-code-title">Share this game code</p>
            <p class="join-code">{{ game.id }}</p>
            <p class="join-code-caption">Players can also open this link: <a :href="joinUrl">{{ joinUrl }}</a></p>
          </div>
          <div class="join-qr" v-if="qrCodeDataUrl">
            <p>Scan to join directly</p>
            <img :src="qrCodeDataUrl" :alt="`QR code to join game ${game.id}`" />
          </div>
          <p v-if="canEditEndTime">Set an optional end time before starting.</p>
          <input v-if="canEditEndTime" v-model="endTimeInput" type="datetime-local" class="text-input" />
          <p v-else class="hint">Only the admin can edit the game end time.</p>
          <div class="action-row" v-if="canEditEndTime">
            <button class="game-button secondary" :disabled="loading" @click="patchEndTime">Save end time</button>
            <button class="game-button primary" :disabled="loading" @click="startGame">Start game</button>
          </div>
          <button class="game-button ghost" :disabled="loading" @click="leaveGame">Leave game</button>
        </div>
      </section>

      <section v-else-if="game.state === 'running'" class="panel">
        <h2>Your Target</h2>
        <div class="stack">
          <VictimRevealPanel :victim-player-id="victimPlayerId" :victim-label="victimName" :loading="loading" />
          <label>
            Confirm kill target
            <input v-model="killTarget" class="text-input" />
          </label>
          <p class="hint" v-if="killTarget">{{ participantLookup.get(killTarget) || 'Unknown player' }}</p>
          <div class="action-row">
            <button class="game-button warn" :disabled="loading" @click="submitKill">Submit kill</button>
            <button v-if="isAdmin" class="game-button danger" :disabled="loading" @click="endGame">End game</button>
          </div>
        </div>
      </section>

      <section v-else class="panel">
        <h2>Game Ended</h2>
        <p>The game is complete. Final leaderboard shown below.</p>
        <button class="game-button primary" @click="router.push({ name: 'start' })">Back to start</button>
      </section>

      <section class="panel">
        <h2>Leaderboard</h2>
        <ul class="leaderboard">
          <li v-for="entry in leaderboardRows" :key="entry.playerId">
            <strong>{{ entry.playerName }}</strong>
            <span>{{ entry.kills }} kills</span>
          </li>
          <li v-if="!leaderboardRows.length" class="muted">Leaderboard appears once game starts.</li>
        </ul>
      </section>
    </section>
  </main>

  <main v-else class="game-view">
    <p class="status-message">Loading game...</p>
  </main>
</template>

<style scoped>
.game-view {
  display: grid;
  gap: 1.1rem;
}

.game-head {
  display: flex;
  justify-content: space-between;
  align-items: start;
  gap: 0.8rem;
}

.head-right {
  display: grid;
  gap: 0.5rem;
  justify-items: end;
}

.overline {
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.09em;
  color: #3b82f6;
  font-size: 0.76rem;
  font-weight: 800;
}

h1 {
  margin: 0.2rem 0;
  font-size: clamp(1.4rem, 2.8vw, 2.2rem);
}

.description {
  margin: 0;
  color: #4b5563;
}

.layout-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
}

.participants,
.leaderboard {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  gap: 0.45rem;
}

.participants li,
.leaderboard li {
  border: 1px solid #dbeafe;
  border-radius: 0.75rem;
  padding: 0.55rem 0.65rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: linear-gradient(135deg, #ffffff, #f5f9ff);
}

.stack {
  display: grid;
  gap: 0.7rem;
}

.action-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.55rem;
}

.join-code-block {
  border: 1px dashed #93c5fd;
  border-radius: 0.8rem;
  padding: 0.7rem;
  background: #eff6ff;
}

.join-code-title {
  margin: 0;
  text-transform: uppercase;
  font-size: 0.76rem;
  letter-spacing: 0.08em;
  color: #1d4ed8;
}

.join-code {
  margin: 0.1rem 0;
  font-size: clamp(1.4rem, 3vw, 2rem);
  font-weight: 700;
  letter-spacing: 0.07em;
  color: #0f172a;
}

.join-code-caption {
  margin: 0;
  color: #334155;
  word-break: break-all;
}

.join-qr {
  display: none;
}

@media (min-width: 980px) {
  .join-qr {
    display: inline-grid;
    gap: 0.4rem;
    justify-items: start;
  }

  .join-qr img {
    border: 1px solid #93c5fd;
    border-radius: 0.65rem;
    background: #ffffff;
    padding: 0.35rem;
    width: 160px;
    height: 160px;
  }
}

.hint,
.muted {
  color: #64748b;
}

.panel h2 {
  margin-top: 0;
}
</style>
