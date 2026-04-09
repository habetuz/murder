<script setup lang="ts">
import { ref } from 'vue'
import PixelButton from '../ui/PixelButton.vue'

const props = defineProps<{
  victimName: string | null
  victimId: string | null
  disabled?: boolean
}>()

const emit = defineEmits<{
  (e: 'confirm', victimId: string): void
}>()

const confirming = ref(false)
const loading = ref(false)
let confirmTimeout: ReturnType<typeof setTimeout> | null = null

function startConfirm() {
  confirming.value = true
  confirmTimeout = setTimeout(() => {
    confirming.value = false
  }, 5000)
}

async function doKill() {
  if (!props.victimId) return
  if (confirmTimeout) clearTimeout(confirmTimeout)
  confirming.value = false
  loading.value = true
  try {
    emit('confirm', props.victimId)
  } finally {
    loading.value = false
  }
}

function cancel() {
  if (confirmTimeout) clearTimeout(confirmTimeout)
  confirming.value = false
}
</script>

<template>
  <div class="flex flex-col items-center gap-2">
    <template v-if="!confirming">
      <PixelButton
        variant="danger"
        size="lg"
        :disabled="disabled || !victimId"
        @click="startConfirm"
      >
        KILL
      </PixelButton>
    </template>
    <template v-else>
      <p class="font-pixel text-[8px] text-murder-warn text-center">
        KILL {{ victimName ?? '???' }}?
      </p>
      <div class="flex gap-3">
        <PixelButton variant="danger" size="md" :loading="loading" @click="doKill">
          CONFIRM
        </PixelButton>
        <PixelButton variant="ghost" size="md" @click="cancel">
          CANCEL
        </PixelButton>
      </div>
    </template>
  </div>
</template>
