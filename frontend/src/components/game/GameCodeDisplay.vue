<script setup lang="ts">
import { ref } from 'vue'

const props = defineProps<{
  code: string
}>()

const copied = ref(false)

async function copy() {
  try {
    await navigator.clipboard.writeText(props.code)
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
  } catch {
    // clipboard not available
  }
}
</script>

<template>
  <div class="flex flex-col items-center gap-3">
    <p class="font-pixel text-[8px] text-murder-dim uppercase tracking-widest">Game Code</p>
    <div
      class="flex items-center gap-1 bg-murder-bg border-2 border-murder-code shadow-pixel px-4 py-3"
    >
      <span
        v-for="char in code"
        :key="char"
        class="font-pixel text-murder-code text-2xl w-8 text-center"
      >
        {{ char }}
      </span>
    </div>
    <button
      class="font-pixel text-[7px] text-murder-dim hover:text-murder-code transition-colors"
      @click="copy"
    >
      {{ copied ? 'COPIED!' : 'COPY CODE' }}
    </button>
  </div>
</template>
