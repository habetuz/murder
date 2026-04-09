<script setup lang="ts">
import { computed } from 'vue'
import { useGameStore } from '../stores/game'
import { useAuthStore } from '../stores/auth'
import LeaderboardTable from '../components/game/LeaderboardTable.vue'
import PixelCard from '../components/ui/PixelCard.vue'
import PixelButton from '../components/ui/PixelButton.vue'

const gameStore = useGameStore()
const auth = useAuthStore()

// Log out guests immediately so their cookie is cleared,
// but keep the component mounted so they can still see the results.
if (auth.isGuest) {
  auth.logout()
}

const winners = computed(() => {
  if (!gameStore.currentLeaderboard.length) return null
  const sorted = [...gameStore.currentLeaderboard].sort((a, b) => b.kills - a.kills)
  const topKills = sorted[0].kills
  const tied = sorted.filter(e => e.kills === topKills)
  return {
    players: tied.map(e => ({
      name: gameStore.participantMap.get(e.playerId) ?? e.playerId,
      isYou: e.playerId === auth.player?.id,
    })),
    kills: topKills,
    anyIsYou: tied.some(e => e.playerId === auth.player?.id),
  }
})
</script>

<template>
  <div v-if="gameStore.currentGame" class="flex-1 flex flex-col items-center px-4 py-6 max-w-2xl mx-auto w-full gap-6">

    <div class="text-center flex flex-col items-center gap-3">
      <p class="font-pixel text-[8px] text-murder-dim">GAME OVER</p>
      <h2 class="font-pixel text-murder-text text-sm">{{ gameStore.currentGame.name }}</h2>
    </div>

    <!-- Winner banner -->
    <div v-if="winners" class="w-full bg-murder-surface border-2 border-murder-warn shadow-pixel text-center py-6 px-4">
      <p class="font-pixel text-murder-warn text-[8px] mb-3">{{ winners.players.length > 1 ? 'WINNERS' : 'WINNER' }}</p>
      <p v-for="(p, i) in winners.players" :key="i" class="font-pixel text-murder-text text-xl break-all" :class="{ 'mt-2': i > 0 }">{{ p.name }}</p>
      <p v-if="winners.anyIsYou" class="font-pixel text-murder-accent text-[8px] mt-2">THAT'S YOU!</p>
      <p class="font-body text-murder-dim text-sm mt-2">{{ winners.kills }} kill{{ winners.kills !== 1 ? 's' : '' }}</p>
    </div>
    <div v-else class="w-full bg-murder-surface border-2 border-murder-border shadow-pixel text-center py-6 px-4">
      <p class="font-pixel text-murder-dim text-[9px]">GAME ENDED</p>
    </div>

    <!-- Final leaderboard -->
    <PixelCard title="FINAL STANDINGS" class="w-full">
      <LeaderboardTable
        :entries="gameStore.currentLeaderboard"
        :participant-map="gameStore.participantMap"
        :username-map="gameStore.usernameMap"
        :current-player-id="auth.player?.id ?? null"
      />
    </PixelCard>

    <div class="flex gap-3">
      <RouterLink to="/">
        <PixelButton variant="ghost" size="md">HOME</PixelButton>
      </RouterLink>
    </div>
  </div>
</template>
