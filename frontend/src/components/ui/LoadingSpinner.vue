<script setup lang="ts">
defineProps<{
  size?: 'sm' | 'md'
}>()
</script>

<template>
  <div
    :class="[
      'flex items-center justify-center gap-1',
      size === 'sm' ? 'h-4' : 'h-8',
    ]"
  >
    <span
      v-for="i in 3"
      :key="i"
      :class="[
        'bg-murder-accent inline-block animate-bounce',
        size === 'sm' ? 'w-1.5 h-1.5' : 'w-2.5 h-2.5',
      ]"
      :style="{ animationDelay: `${(i - 1) * 0.15}s` }"
    />
  </div>
</template>
