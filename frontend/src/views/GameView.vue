<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import QRCode from 'qrcode';
import Card from 'primevue/card';
import Button from 'primevue/button';
import InputText from 'primevue/inputtext';
import Message from 'primevue/message';
import Tag from 'primevue/tag';
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
    <section class="game-head">
      <div>
        <p class="overline">Game Code {{ game.id }}</p>
        <h1>{{ game.name }}</h1>
        <p class="description">{{ game.description || 'No description' }}</p>
      </div>
      <div class="head-right">
        <Tag :value="game.state" rounded />
        <CountdownBadge :end-time="game.endTime" />
      </div>
    </section>

    <Message v-if="errorText" severity="error" :closable="false">{{ errorText }}</Message>

    <section class="layout-grid">
      <Card>
        <template #title>Participants</template>
        <template #content>
          <ul class="participants">
            <li v-for="player in participants" :key="player.id">
              <span>{{ player.name }}</span>
              <small>{{ player.kind }}</small>
            </li>
          </ul>
        </template>
      </Card>

      <Card v-if="game.state === 'pending'">
        <template #title>Pregame</template>
        <template #content>
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
            <input v-if="canEditEndTime" v-model="endTimeInput" type="datetime-local" class="native-input" />
            <p v-else class="hint">Only the admin can edit the game end time.</p>
            <div class="action-row" v-if="canEditEndTime">
              <Button label="Save end time" severity="secondary" @click="patchEndTime" :loading="loading" />
              <Button label="Start game" @click="startGame" :loading="loading" />
            </div>
            <Button label="Leave game" severity="contrast" outlined @click="leaveGame" :loading="loading" />
          </div>
        </template>
      </Card>

      <Card v-else-if="game.state === 'running'">
        <template #title>Your Target</template>
        <template #content>
          <div class="stack">
            <VictimRevealPanel :victim-player-id="victimPlayerId" :victim-label="victimName" :loading="loading" />
            <label>
              Confirm kill target
              <InputText v-model="killTarget" class="full" />
            </label>
            <p class="hint" v-if="killTarget">{{ participantLookup.get(killTarget) || 'Unknown player' }}</p>
            <div class="action-row">
              <Button label="Submit kill" severity="warn" :loading="loading" @click="submitKill" />
              <Button v-if="isAdmin" label="End game" severity="danger" :loading="loading" @click="endGame" />
            </div>
          </div>
        </template>
      </Card>

      <Card v-else>
        <template #title>Game Ended</template>
        <template #content>
          <p>The game is complete. Final leaderboard shown below.</p>
          <Button label="Back to start" severity="contrast" outlined @click="$router.push('/')" />
        </template>
      </Card>

      <Card>
        <template #title>Leaderboard</template>
        <template #content>
          <ul class="leaderboard">
            <li v-for="entry in leaderboardRows" :key="entry.playerId">
              <strong>{{ entry.playerName }}</strong>
              <span>{{ entry.kills }} kills</span>
            </li>
            <li v-if="!leaderboardRows.length" class="muted">Leaderboard appears once game starts.</li>
          </ul>
        </template>
      </Card>
    </section>
  </main>

  <main v-else class="game-view">
    <p>Loading game...</p>
  </main>
</template>

<style scoped>
.game-view {
  display: grid;
  gap: 1rem;
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
  color: #7e5f45;
  font-size: 0.76rem;
}

h1 {
  margin: 0.2rem 0;
  font-size: clamp(1.4rem, 2.8vw, 2.2rem);
}

.description {
  margin: 0;
  color: #5a3a2a;
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
  border: 1px solid rgba(70, 46, 31, 0.2);
  border-radius: 0.65rem;
  padding: 0.55rem 0.65rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(252, 246, 236, 0.6);
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

.native-input {
  width: 100%;
  border: 1px solid #cbb59c;
  border-radius: 0.6rem;
  padding: 0.6rem;
  font: inherit;
}

.join-code-block {
  border: 1px dashed rgba(62, 41, 28, 0.45);
  border-radius: 0.8rem;
  padding: 0.7rem;
  background: rgba(248, 236, 213, 0.8);
}

.join-code-title {
  margin: 0;
  text-transform: uppercase;
  font-size: 0.76rem;
  letter-spacing: 0.08em;
  color: #6f4c38;
}

.join-code {
  margin: 0.1rem 0;
  font-size: clamp(1.4rem, 3vw, 2rem);
  font-weight: 700;
  letter-spacing: 0.07em;
  color: #2d160f;
}

.join-code-caption {
  margin: 0;
  color: #6f4c38;
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
    border: 1px solid rgba(62, 41, 28, 0.45);
    border-radius: 0.65rem;
    background: #f6efdf;
    padding: 0.35rem;
    width: 160px;
    height: 160px;
  }
}

.full,
:deep(.full input) {
  width: 100%;
}

.hint,
.muted {
  color: #6f4c38;
}
</style>
