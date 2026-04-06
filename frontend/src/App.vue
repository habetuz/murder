<script setup lang="ts">
import { onMounted } from 'vue';
import AppHeader from './components/layout/AppHeader.vue';
import { useAuthStore } from './stores/auth';

const auth = useAuthStore();

onMounted(async () => {
  await auth.bootstrap();
});
</script>

<template>
  <div class="app-shell">
    <AppHeader />
    <section class="page-shell">
      <RouterView />
    </section>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100dvh;
  background:
    radial-gradient(120% 90% at 95% 5%, rgba(170, 132, 90, 0.25), transparent 45%),
    radial-gradient(100% 120% at 0% 0%, rgba(94, 56, 38, 0.2), transparent 50%),
    linear-gradient(180deg, #f6efdf 0%, #f2e6d1 100%);
}

.page-shell {
  width: min(1120px, 100% - 1.5rem);
  margin: 0 auto;
  padding: 1rem 0 2rem;
}

@media (max-width: 700px) {
  .page-shell {
    width: min(1120px, 100% - 1rem);
    padding-top: 0.8rem;
  }
}
</style>
