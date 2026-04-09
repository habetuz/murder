<script setup lang="ts">
defineProps<{
  message?: string | null
}>()
</script>

<template>
  <div
    v-if="message"
    class="bg-murder-danger/20 border-2 border-murder-danger text-murder-danger font-body text-sm px-4 py-3 leading-snug"
  >
    {{ message }}
  </div>
</template>
