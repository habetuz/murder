<script setup lang="ts">
defineProps<{
  title?: string
  variant?: 'default' | 'accent' | 'danger'
}>()
</script>

<template>
  <div
    :class="[
      'bg-murder-surface border-2 shadow-pixel p-4',
      variant === 'accent' && 'border-murder-accent',
      variant === 'danger' && 'border-murder-danger',
      !variant || variant === 'default' ? 'border-murder-border' : '',
    ]"
  >
    <h3 v-if="title" class="font-pixel text-[10px] text-murder-text mb-4 pb-3 border-b border-murder-border">
      {{ title }}
    </h3>
    <slot />
  </div>
</template>
