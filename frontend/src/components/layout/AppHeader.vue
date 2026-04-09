<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import PixelButton from '../ui/PixelButton.vue'

const auth = useAuthStore()
const router = useRouter()

async function logout() {
  await auth.logout()
  router.push({ name: 'landing' })
}
</script>

<template>
  <header class="bg-murder-surface border-b-2 border-murder-border px-4 py-3 sticky top-0 z-40">
    <div class="max-w-2xl mx-auto flex items-center justify-between gap-4">
      <RouterLink to="/" class="font-pixel text-murder-accent text-[11px] hover:text-murder-accent/80 transition-colors no-underline">
        MURDER
      </RouterLink>

      <div v-if="auth.isAuthenticated" class="flex items-center gap-3">
        <RouterLink
          v-if="auth.isUser"
          to="/settings"
          class="font-pixel text-[7px] text-murder-dim hover:text-murder-text transition-colors no-underline"
        >
          {{ auth.player?.name }}
        </RouterLink>
        <span v-else class="font-pixel text-[7px] text-murder-dim">
          GUEST
        </span>
        <PixelButton variant="ghost" size="sm" @click="logout">
          QUIT
        </PixelButton>
      </div>
    </div>
  </header>
</template>
