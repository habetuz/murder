<script setup lang="ts">
import { computed } from 'vue'
import { useQrCode } from '../../composables/useQrCode'

const props = defineProps<{
  gameId: string
}>()

const joinUrl = computed(() => `${window.location.origin}/?join=${props.gameId}`)
const { dataUrl } = useQrCode(joinUrl)
</script>

<template>
  <div class="hidden md:flex flex-col items-center gap-3">
    <p class="font-pixel text-[8px] text-murder-dim uppercase tracking-widest">Scan to Join</p>
    <div class="bg-murder-surface border-2 border-murder-border shadow-pixel p-3">
      <img
        v-if="dataUrl"
        :src="dataUrl"
        alt="QR code to join game"
        class="w-[220px] h-[220px] block"
        style="image-rendering: pixelated;"
      />
      <div v-else class="w-[220px] h-[220px] flex items-center justify-center">
        <span class="font-pixel text-[7px] text-murder-dim">GENERATING...</span>
      </div>
    </div>
    <p class="font-pixel text-[7px] text-murder-dim">{{ joinUrl }}</p>
  </div>
</template>
