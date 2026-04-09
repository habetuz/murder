<script setup lang="ts">
import { computed } from 'vue'
import type { LeaderboardEntry } from '../../types/api'

const props = defineProps<{
  entries: LeaderboardEntry[]
  participantMap: Map<string, string>
  usernameMap: Map<string, string | null>
  currentPlayerId: string | null
}>()

const sorted = computed(() =>
  [...props.entries].sort((a, b) => b.kills - a.kills),
)

const medals = ['🥇', '🥈', '🥉']
</script>

<template>
  <div class="flex flex-col gap-1">
    <div
      v-for="(entry, i) in sorted"
      :key="entry.playerId"
      :class="[
        'flex items-center gap-3 px-3 py-2 border-2',
        entry.playerId === currentPlayerId
          ? 'border-murder-accent bg-murder-accent/10'
          : 'border-murder-border bg-murder-surface',
      ]"
    >
      <span class="font-pixel text-[9px] w-6 text-center shrink-0">
        {{ i < 3 ? medals[i] : `#${i + 1}` }}
      </span>
      <span class="font-body text-murder-text text-sm flex-1 truncate">
        {{ participantMap.get(entry.playerId) ?? entry.playerId }}
        <span
          v-if="usernameMap.get(entry.playerId)"
          class="text-murder-dim text-xs"
        >@{{ usernameMap.get(entry.playerId) }}</span>
      </span>
      <span class="font-pixel text-[9px] text-murder-accent shrink-0">
        {{ entry.kills }}✦
      </span>
    </div>
    <p v-if="!sorted.length" class="font-pixel text-[8px] text-murder-dim text-center py-4">
      NO KILLS YET
    </p>
  </div>
</template>
