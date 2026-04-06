<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue';

const props = defineProps<{
  endTime: string | null;
}>();

const now = ref(Date.now());
let timer: number | null = null;

const remaining = computed(() => {
  if (!props.endTime) {
    return null;
  }

  const ms = new Date(props.endTime).getTime() - now.value;
  return Math.max(0, ms);
});

const label = computed(() => {
  if (remaining.value === null) {
    return 'No end time';
  }

  if (remaining.value <= 0) {
    return 'Time is up';
  }

  const totalSeconds = Math.floor(remaining.value / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  const hh = hours.toString().padStart(2, '0');
  const mm = minutes.toString().padStart(2, '0');
  const ss = seconds.toString().padStart(2, '0');

  return `${hh}:${mm}:${ss}`;
});

const isUrgent = computed(() => remaining.value !== null && remaining.value > 0 && remaining.value < 10 * 60 * 1000);

onMounted(() => {
  timer = window.setInterval(() => {
    now.value = Date.now();
  }, 1000);
});

onUnmounted(() => {
  if (timer !== null) {
    window.clearInterval(timer);
  }
});
</script>

<template>
  <div class="countdown" :class="{ urgent: isUrgent }">
    <span class="caption">End Countdown</span>
    <strong>{{ label }}</strong>
  </div>
</template>

<style scoped>
.countdown {
  display: inline-flex;
  flex-direction: column;
  gap: 0.25rem;
  background: rgba(252, 247, 234, 0.88);
  border: 1px solid rgba(70, 46, 31, 0.2);
  border-radius: 0.75rem;
  padding: 0.55rem 0.8rem;
  color: #2f1d12;
}

.countdown strong {
  font-size: 1.25rem;
  letter-spacing: 0.05em;
}

.caption {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.countdown.urgent {
  background: #3f1d13;
  color: #fff8ef;
  border-color: #3f1d13;
}
</style>
