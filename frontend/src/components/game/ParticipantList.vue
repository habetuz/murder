<script setup lang="ts">
import { ref } from 'vue'
import type { ParticipantDto } from '../../types/api'
import { ApiError } from '../../api/client'

const props = defineProps<{
  participants: ParticipantDto[]
  adminPlayerId: string | null
  currentPlayerId: string | null
  gameState?: 'pending' | 'running' | 'ended'
  canKick?: boolean
  onKick?: (playerId: string) => Promise<void>
}>()

const kickingId = ref<string | null>(null)
const kickError = ref<string | null>(null)

function canKickPlayer(p: ParticipantDto): boolean {
  return !!(
    props.canKick &&
    props.gameState === 'pending' &&
    p.id !== props.adminPlayerId &&
    p.id !== props.currentPlayerId
  )
}

async function kick(playerId: string) {
  if (!props.onKick) return
  kickingId.value = playerId
  kickError.value = null
  try {
    await props.onKick(playerId)
  } catch (e) {
    kickError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to kick'
  } finally {
    kickingId.value = null
  }
}
</script>

<template>
  <div class="flex flex-col gap-2">
    <p v-if="kickError" class="font-body text-murder-danger text-xs">{{ kickError }}</p>
    <div
      v-for="p in participants"
      :key="p.id"
      :class="[
        'flex items-center justify-between px-3 py-2 border-2',
        p.id === currentPlayerId
          ? 'border-murder-accent bg-murder-accent/10'
          : 'border-murder-border bg-murder-surface',
      ]"
    >
      <div class="flex items-center gap-2 min-w-0">
        <span class="font-body text-murder-text text-sm truncate">{{ p.name }}</span>
        <span
          v-if="p.username"
          class="font-body text-murder-dim text-xs truncate"
        >@{{ p.username }}</span>
        <span
          v-if="p.id === adminPlayerId"
          class="font-pixel text-[6px] text-murder-warn border border-murder-warn px-1 shrink-0"
        >ADMIN</span>
        <span
          v-if="p.id === currentPlayerId"
          class="font-pixel text-[6px] text-murder-accent border border-murder-accent px-1 shrink-0"
        >YOU</span>
        <span
          v-if="p.kind === 'guest'"
          class="font-pixel text-[6px] text-murder-dim border border-murder-dim px-1 shrink-0"
        >GUEST</span>
      </div>
      <button
        v-if="canKickPlayer(p)"
        :disabled="kickingId === p.id"
        class="font-pixel text-[7px] text-murder-danger border border-murder-danger px-1.5 py-0.5 hover:bg-murder-danger/20 transition-colors disabled:opacity-50 shrink-0 ml-2"
        @click="kick(p.id)"
      >
        {{ kickingId === p.id ? '...' : 'KICK' }}
      </button>
    </div>
    <p v-if="!participants.length" class="font-pixel text-[8px] text-murder-dim text-center py-4">
      NO PLAYERS YET
    </p>
  </div>
</template>
