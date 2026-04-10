<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useGameStore } from '../stores/game'
import { useAuthStore } from '../stores/auth'
import { ApiError } from '../api/client'
import { ref } from 'vue'
import GameCodeDisplay from '../components/game/GameCodeDisplay.vue'
import QrCodePanel from '../components/game/QrCodePanel.vue'
import CountdownTimer from '../components/game/CountdownTimer.vue'
import ParticipantList from '../components/game/ParticipantList.vue'
import AdminControls from '../components/game/AdminControls.vue'
import PixelCard from '../components/ui/PixelCard.vue'
import PixelButton from '../components/ui/PixelButton.vue'
import ErrorBanner from '../components/ui/ErrorBanner.vue'

const gameStore = useGameStore()
const auth = useAuthStore()
const router = useRouter()

const leaveLoading = ref(false)
const leaveError = ref<string | null>(null)

async function leave() {
  if (!gameStore.currentGame) return
  leaveLoading.value = true
  leaveError.value = null
  try {
    await gameStore.leaveGame(gameStore.currentGame.id)
    if (auth.isGuest) {
      auth.clearAuthState()
    }
    router.push({ name: 'landing' })
  } catch (e) {
    leaveError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to leave'
  } finally {
    leaveLoading.value = false
  }
}

async function handleKick(playerId: string) {
  if (!gameStore.currentGame) return
  await gameStore.kickPlayer(gameStore.currentGame.id, playerId)
}

async function handleRestore(playerId: string): Promise<string> {
  return gameStore.generateRestoreToken(gameStore.currentGame!.id, playerId)
}
</script>

<template>
  <div v-if="gameStore.currentGame" class="flex-1 flex flex-col items-center px-4 py-6 max-w-2xl mx-auto w-full gap-6">

    <!-- Game title + state -->
    <div class="text-center">
      <h2 class="font-pixel text-murder-text text-sm mb-2">{{ gameStore.currentGame.name }}</h2>
      <p v-if="gameStore.currentGame.description" class="font-body text-murder-dim text-xs">
        {{ gameStore.currentGame.description }}
      </p>
    </div>

    <!-- Code + QR side by side on desktop -->
    <div class="flex flex-col md:flex-row items-center gap-6 w-full justify-center">
      <GameCodeDisplay :code="gameStore.currentGame.id" />
      <QrCodePanel :game-id="gameStore.currentGame.id" />
    </div>

    <CountdownTimer :end-time="gameStore.currentGame.endTime" />

    <!-- Participants -->
    <PixelCard title="PLAYERS" class="w-full">
      <ParticipantList
        :participants="gameStore.currentParticipants"
        :admin-player-id="gameStore.currentGame.adminPlayerId"
        :current-player-id="auth.player?.id ?? null"
        game-state="pending"
        :can-kick="gameStore.isAdmin"
        :on-kick="handleKick"
        :can-restore="gameStore.isAdmin"
        :on-restore="handleRestore"
      />
    </PixelCard>

    <!-- Admin controls -->
    <PixelCard v-if="gameStore.isAdmin" class="w-full">
      <AdminControls :game="gameStore.currentGame" />
    </PixelCard>

    <!-- Leave (non-admin) -->
    <template v-if="!gameStore.isAdmin">
      <ErrorBanner :message="leaveError" />
      <PixelButton variant="ghost" size="sm" :loading="leaveLoading" @click="leave">
        LEAVE GAME
      </PixelButton>
    </template>

    <p v-if="!gameStore.isAdmin" class="font-pixel text-[7px] text-murder-dim text-center">
      WAITING FOR ADMIN TO START...
    </p>
  </div>
</template>
