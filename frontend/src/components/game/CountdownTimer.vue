<script setup lang="ts">
import { toRef } from 'vue'
import { useCountdown } from '../../composables/useCountdown'

const props = defineProps<{
  endTime: string | null
}>()

const { label, isUrgent, isExpired } = useCountdown(toRef(props, 'endTime'))
</script>

<template>
  <div v-if="endTime" class="flex flex-col items-center gap-1">
    <p class="font-pixel text-[7px] text-murder-dim uppercase tracking-widest">Time Remaining</p>
    <span
      :class="[
        'font-pixel text-lg tabular-nums',
        isExpired ? 'text-murder-dim' : isUrgent ? 'text-murder-danger animate-pulse' : 'text-murder-warn',
      ]"
    >
      {{ label }}
    </span>
  </div>
</template>
