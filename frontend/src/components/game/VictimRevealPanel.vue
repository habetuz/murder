<script setup lang="ts">
import { computed, ref } from 'vue';

const props = defineProps<{
  victimPlayerId: string | null;
  victimLabel?: string | null;
  loading?: boolean;
}>();

const holdVisible = ref(false);
let holdTimer: number | null = null;

const text = computed(() => {
  if (!props.victimPlayerId && !props.victimLabel) {
    return 'No victim assigned';
  }
  return props.victimLabel ?? props.victimPlayerId;
});

function startHold() {
  if (props.loading || !props.victimPlayerId) {
    return;
  }

  clearHold();
  holdTimer = window.setTimeout(() => {
    holdVisible.value = true;
  }, 350);
}

function stopHold() {
  clearHold();
  holdVisible.value = false;
}

function clearHold() {
  if (holdTimer !== null) {
    window.clearTimeout(holdTimer);
    holdTimer = null;
  }
}
</script>

<template>
  <div class="victim-panel">
    <p class="helper">Hold the button to reveal. Release to hide.</p>

    <div class="victim-value" :class="{ hidden: !holdVisible }">
      <span v-if="!holdVisible">Hidden</span>
      <span v-else>{{ text }}</span>
    </div>

    <button
      class="reveal-button"
      :disabled="loading || !victimPlayerId"
      @pointerdown="startHold"
      @pointerup="stopHold"
      @pointercancel="stopHold"
      @pointerleave="stopHold"
      @touchend="stopHold"
      @mouseup="stopHold"
      @keyup.space="stopHold"
      @keydown.space.prevent="startHold"
    >
      Hold to reveal victim
    </button>
  </div>
</template>

<style scoped>
.victim-panel {
  display: grid;
  gap: 0.75rem;
}

.helper {
  margin: 0;
  color: #5a3a2a;
  font-size: 0.9rem;
}

.victim-value {
  min-height: 3rem;
  border-radius: 0.8rem;
  border: 1px dashed #6f4b37;
  display: grid;
  place-items: center;
  font-size: 1.2rem;
  letter-spacing: 0.05em;
  background: rgba(250, 242, 220, 0.8);
  color: #26140d;
}

.victim-value.hidden {
  color: transparent;
  text-shadow: 0 0 8px rgba(38, 20, 13, 0.7);
}

.reveal-button {
  border: none;
  border-radius: 999px;
  background: #2a1711;
  color: #fff4e8;
  padding: 0.8rem 1rem;
  font-size: 1rem;
}

.reveal-button:disabled {
  opacity: 0.6;
}
</style>
