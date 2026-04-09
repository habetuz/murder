<script setup lang="ts">
defineProps<{
  open: boolean
  title?: string
}>()

const emit = defineEmits<{
  (e: 'close'): void
}>()
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="open"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70"
        @click.self="emit('close')"
      >
        <div class="bg-murder-surface border-2 border-murder-border shadow-pixel w-full max-w-sm">
          <div v-if="title" class="flex items-center justify-between px-4 py-3 border-b border-murder-border">
            <span class="font-pixel text-[9px] text-murder-text">{{ title }}</span>
            <button
              class="font-pixel text-[10px] text-murder-dim hover:text-murder-danger leading-none"
              @click="emit('close')"
            >
              ✕
            </button>
          </div>
          <div class="p-4">
            <slot />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.15s;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
