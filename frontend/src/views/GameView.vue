<script setup lang="ts">
import { onMounted, onUnmounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useGameStore } from '../stores/game'
import LoadingSpinner from '../components/ui/LoadingSpinner.vue'
import ErrorBanner from '../components/ui/ErrorBanner.vue'
import GameLobbyView from './GameLobbyView.vue'
import GamePlayView from './GamePlayView.vue'
import GameEndView from './GameEndView.vue'

const route = useRoute()
const gameStore = useGameStore()

const gameId = computed(() => route.params.gameId as string)

onMounted(() => {
  gameStore.startWatch(gameId.value)
})

onUnmounted(() => {
  gameStore.clearCurrentGame()
})
</script>

<template>
  <div class="flex-1 flex flex-col">
    <div v-if="gameStore.gameDeleted" class="flex-1 flex flex-col items-center justify-center px-4 gap-4">
      <ErrorBanner message="This game has been deleted." />
      <RouterLink to="/" class="font-pixel text-[8px] text-murder-dim hover:text-murder-accent">
        ← BACK HOME
      </RouterLink>
    </div>

    <div v-else-if="!gameStore.currentGame && !gameStore.gameError" class="flex-1 flex items-center justify-center">
      <LoadingSpinner />
    </div>

    <div v-else-if="gameStore.gameError && !gameStore.currentGame" class="flex-1 flex flex-col items-center justify-center px-4 gap-4">
      <ErrorBanner :message="gameStore.gameError" />
      <RouterLink to="/" class="font-pixel text-[8px] text-murder-dim hover:text-murder-accent">
        ← BACK HOME
      </RouterLink>
    </div>

    <template v-else-if="gameStore.currentGame">
      <GameLobbyView v-if="gameStore.currentGame.state === 'pending'" />
      <GamePlayView v-else-if="gameStore.currentGame.state === 'running'" />
      <GameEndView v-else-if="gameStore.currentGame.state === 'ended'" />
    </template>
  </div>
</template>
