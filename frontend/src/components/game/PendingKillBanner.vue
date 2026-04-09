<script setup lang="ts">
import PixelCard from '../ui/PixelCard.vue'
import PixelButton from '../ui/PixelButton.vue'
import ErrorBanner from '../ui/ErrorBanner.vue'

defineProps<{
  loading?: boolean
  error?: string | null
}>()

const emit = defineEmits<{
  (e: 'respond', accepted: boolean): void
}>()
</script>

<template>
  <PixelCard variant="danger" class="w-full text-center py-4">
    <p class="font-pixel text-[10px] text-murder-warn mb-2">KILL CLAIM</p>
    <p class="font-body text-murder-text text-sm mb-4">
      Someone claims to have killed you. Did it happen?
    </p>
    <ErrorBanner v-if="error" :message="error" />
    <div class="flex justify-center gap-3">
      <PixelButton variant="danger" size="md" :loading="loading" @click="emit('respond', true)">
        CONFIRM
      </PixelButton>
      <PixelButton variant="ghost" size="md" :loading="loading" @click="emit('respond', false)">
        DENY
      </PixelButton>
    </div>
  </PixelCard>
</template>
