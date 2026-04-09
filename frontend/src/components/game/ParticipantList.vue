<script setup lang="ts">
import type { ParticipantDto } from '../../types/api'

defineProps<{
  participants: ParticipantDto[]
  adminPlayerId: string | null
  currentPlayerId: string | null
  gameState?: 'pending' | 'running' | 'ended'
}>()
</script>

<template>
  <div class="flex flex-col gap-2">
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
    </div>
    <p v-if="!participants.length" class="font-pixel text-[8px] text-murder-dim text-center py-4">
      NO PLAYERS YET
    </p>
  </div>
</template>
