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
    radial-gradient(45% 35% at 0% 0%, rgba(14, 165, 164, 0.2), transparent 60%),
    radial-gradient(40% 30% at 100% 0%, rgba(37, 99, 235, 0.18), transparent 65%),
    radial-gradient(60% 50% at 50% 100%, rgba(245, 158, 11, 0.12), transparent 65%),
    linear-gradient(180deg, #f8fbff 0%, #eef4ff 100%);
}

.page-shell {
  width: min(1120px, 100% - 1.5rem);
  margin: 0 auto;
  padding: 1rem 0 2.2rem;
}

@media (max-width: 700px) {
  .page-shell {
    width: min(1120px, 100% - 1rem);
    padding-top: 0.8rem;
  }
}
</style>
