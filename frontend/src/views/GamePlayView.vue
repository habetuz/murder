<script setup lang="ts">
import { ref } from 'vue'
import { useGameStore } from '../stores/game'
import { useAuthStore } from '../stores/auth'
import { ApiError } from '../api/client'
import VictimReveal from '../components/game/VictimReveal.vue'
import KillConfirmButton from '../components/game/KillConfirmButton.vue'
import PendingKillBanner from '../components/game/PendingKillBanner.vue'
import LeaderboardTable from '../components/game/LeaderboardTable.vue'
import CountdownTimer from '../components/game/CountdownTimer.vue'
import AdminControls from '../components/game/AdminControls.vue'
import PixelCard from '../components/ui/PixelCard.vue'
import ErrorBanner from '../components/ui/ErrorBanner.vue'

const gameStore = useGameStore()
const auth = useAuthStore()

const killError = ref<string | null>(null)
const killLoading = ref(false)
const respondLoading = ref(false)
const respondError = ref<string | null>(null)

async function handleKill(victimId: string) {
  killLoading.value = true
  killError.value = null
  try {
    await gameStore.submitKill(gameStore.currentGame!.id, victimId)
  } catch (e) {
    killError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Kill failed'
  } finally {
    killLoading.value = false
  }
}

async function handleRespondToKill(accepted: boolean) {
  respondLoading.value = true
  respondError.value = null
  try {
    await gameStore.respondToKill(gameStore.currentGame!.id, accepted)
  } catch (e) {
    respondError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Response failed'
  } finally {
    respondLoading.value = false
  }
}
</script>

<template>
  <div v-if="gameStore.currentGame" class="flex-1 flex flex-col items-center px-4 py-6 max-w-2xl mx-auto w-full gap-6">

    <div class="text-center">
      <h2 class="font-pixel text-murder-text text-sm mb-1">{{ gameStore.currentGame.name }}</h2>
      <CountdownTimer :end-time="gameStore.currentGame.endTime" />
    </div>

    <!-- Dead state -->
    <template v-if="gameStore.isDead">
      <PixelCard variant="danger" class="w-full text-center py-6">
        <p class="font-pixel text-murder-danger text-lg mb-2">&#9760;</p>
        <p class="font-pixel text-[10px] text-murder-danger">YOU HAVE BEEN</p>
        <p class="font-pixel text-[10px] text-murder-danger">ELIMINATED</p>
        <p class="font-body text-murder-dim text-xs mt-3">Sit back and watch the carnage.</p>
      </PixelCard>
    </template>

    <!-- Alive state -->
    <template v-else>
      <!-- Pending kill banner for victim -->
      <PendingKillBanner
        v-if="gameStore.pendingKill"
        :loading="respondLoading"
        :error="respondError"
        @respond="handleRespondToKill"
      />

      <PixelCard variant="accent" class="w-full">
        <VictimReveal
          :victim-name="gameStore.currentVictimName"
          :victim-username="gameStore.currentVictimId ? gameStore.usernameMap.get(gameStore.currentVictimId) ?? null : null"
          :loading="!gameStore.currentVictimName && !gameStore.isDead"
        />
      </PixelCard>

      <ErrorBanner :message="killError" />

      <!-- Killer: waiting for victim confirmation -->
      <PixelCard v-if="gameStore.pendingKillSent" variant="default" class="w-full text-center py-4">
        <p class="font-pixel text-[9px] text-murder-warn mb-1">KILL PENDING</p>
        <p class="font-body text-murder-dim text-xs">Waiting for your victim to confirm the kill...</p>
      </PixelCard>

      <!-- Normal kill button (hidden while a kill is pending) -->
      <KillConfirmButton
        v-if="!gameStore.pendingKillSent"
        :victim-name="gameStore.currentVictimName"
        :victim-id="gameStore.currentVictimId"
        :disabled="killLoading || !gameStore.currentVictimId"
        @confirm="handleKill"
      />
    </template>

    <!-- Leaderboard -->
    <PixelCard title="LEADERBOARD" class="w-full">
      <LeaderboardTable
        :entries="gameStore.currentLeaderboard"
        :participant-map="gameStore.participantMap"
        :username-map="gameStore.usernameMap"
        :current-player-id="auth.player?.id ?? null"
      />
    </PixelCard>

    <!-- Admin controls -->
    <PixelCard v-if="gameStore.isAdmin" class="w-full">
      <AdminControls :game="gameStore.currentGame" />
    </PixelCard>
  </div>
</template>
