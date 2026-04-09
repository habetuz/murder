<script setup lang="ts">
defineProps<{
  state: 'pending' | 'running' | 'ended'
}>()

const labels: Record<string, string> = {
  pending: 'PENDING',
  running: 'RUNNING',
  ended: 'ENDED',
}
</script>

<template>
  <span
    :class="[
      'font-pixel text-[7px] px-2 py-1 border-2 uppercase inline-block',
      state === 'pending' && 'bg-murder-warn/20 border-murder-warn text-murder-warn',
      state === 'running' && 'bg-murder-accent/20 border-murder-accent text-murder-accent animate-pulse',
      state === 'ended' && 'bg-murder-dim/20 border-murder-dim text-murder-dim',
    ]"
  >
    {{ labels[state] }}
  </span>
</template>
