<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppHeader from './components/layout/AppHeader.vue'
</script>

<template>
  <div class="flex flex-col min-h-svh bg-murder-bg">
    <AppHeader />
    <main class="flex-1 flex flex-col">
      <RouterView />
    </main>
  </div>
</template>
