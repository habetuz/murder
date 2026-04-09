<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { api, ApiError } from '../api/client'
import type { CredentialDto } from '../types/api'
import PixelCard from '../components/ui/PixelCard.vue'
import PixelButton from '../components/ui/PixelButton.vue'
import PixelInput from '../components/ui/PixelInput.vue'
import PixelModal from '../components/ui/PixelModal.vue'
import ErrorBanner from '../components/ui/ErrorBanner.vue'
import LoadingSpinner from '../components/ui/LoadingSpinner.vue'

const auth = useAuthStore()
const router = useRouter()

const credentials = ref<CredentialDto[]>([])
const loadingCreds = ref(false)

const showPassword = ref(false)
const newPassword = ref('')
const confirmPassword = ref('')
const passwordLoading = ref(false)
const passwordError = ref<string | null>(null)
const passwordSuccess = ref(false)

const showRevokeAll = ref(false)
const revokeLoading = ref(false)
const revokeError = ref<string | null>(null)

const deleteLoading = ref(false)
const deleteError = ref<string | null>(null)

onMounted(async () => {
  loadingCreds.value = true
  try {
    credentials.value = await api.getCredentials()
  } catch {
    // ignore
  } finally {
    loadingCreds.value = false
  }
})

async function savePassword() {
  if (!newPassword.value || newPassword.value !== confirmPassword.value) return
  passwordLoading.value = true
  passwordError.value = null
  passwordSuccess.value = false
  try {
    await api.createPassword(newPassword.value)
    passwordSuccess.value = true
    newPassword.value = ''
    confirmPassword.value = ''
    credentials.value = await api.getCredentials()
    setTimeout(() => { showPassword.value = false }, 1000)
  } catch (e) {
    passwordError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error saving password'
  } finally {
    passwordLoading.value = false
  }
}

async function deleteCredential(id: string) {
  deleteLoading.value = true
  deleteError.value = null
  try {
    await api.deleteCredential(id)
    credentials.value = credentials.value.filter(c => c.id !== id)
  } catch (e) {
    deleteError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error revoking'
  } finally {
    deleteLoading.value = false
  }
}

async function revokeAllSessions() {
  revokeLoading.value = true
  revokeError.value = null
  try {
    await api.revokeAllSessions()
    auth.clearAuthState()
    router.push({ name: 'landing' })
  } catch (e) {
    revokeError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error revoking sessions'
  } finally {
    revokeLoading.value = false
  }
}
</script>

<template>
  <div class="flex-1 flex flex-col items-center px-4 py-6 max-w-2xl mx-auto w-full gap-6">
    <div class="w-full">
      <h2 class="font-pixel text-murder-text text-sm mb-1">SETTINGS</h2>
      <p class="font-body text-murder-dim text-xs">Manage your account for {{ auth.player?.name ?? 'Guest' }}</p>
    </div>

    <!-- Credentials -->
    <PixelCard title="CREDENTIALS" class="w-full">
      <div class="flex flex-col gap-3">
        <ErrorBanner :message="deleteError" />
        <LoadingSpinner v-if="loadingCreds" size="sm" />
        <div
          v-for="cred in credentials"
          :key="cred.id"
          class="flex items-center justify-between gap-2 py-2 border-b border-murder-border last:border-0"
        >
          <div>
            <p class="font-pixel text-[8px] text-murder-text uppercase">{{ cred.type }}</p>
            <p v-if="cred.expiresAt" class="font-body text-murder-dim text-xs">
              Expires: {{ new Date(cred.expiresAt).toLocaleString() }}
            </p>
          </div>
          <PixelButton variant="danger" size="sm" :loading="deleteLoading" @click="deleteCredential(cred.id)">
            REVOKE
          </PixelButton>
        </div>
        <PixelButton variant="ghost" size="sm" @click="showPassword = true">
          SET PASSWORD
        </PixelButton>
      </div>
    </PixelCard>

    <!-- Sessions -->
    <PixelCard title="SESSIONS" class="w-full">
      <div class="flex flex-col gap-3">
        <p class="font-body text-murder-dim text-xs">Revoke all active sessions and log out everywhere.</p>
        <ErrorBanner :message="revokeError" />
        <PixelButton variant="danger" size="sm" @click="showRevokeAll = true">
          REVOKE ALL SESSIONS
        </PixelButton>
      </div>
    </PixelCard>

    <!-- Back -->
    <RouterLink to="/">
      <PixelButton variant="ghost" size="sm">← BACK</PixelButton>
    </RouterLink>

    <!-- Set password modal -->
    <PixelModal :open="showPassword" title="SET PASSWORD" @close="showPassword = false">
      <div class="flex flex-col gap-4">
        <ErrorBanner :message="passwordError" />
        <p v-if="passwordSuccess" class="font-pixel text-[7px] text-murder-accent">PASSWORD SAVED!</p>
        <PixelInput v-model="newPassword" label="New Password" type="password" placeholder="••••••••" autocomplete="new-password" />
        <PixelInput v-model="confirmPassword" label="Confirm Password" type="password" placeholder="••••••••" autocomplete="new-password" />
        <PixelButton
          variant="primary"
          size="md"
          class="w-full"
          :loading="passwordLoading"
          :disabled="!newPassword || newPassword !== confirmPassword"
          @click="savePassword"
        >
          SAVE
        </PixelButton>
      </div>
    </PixelModal>

    <!-- Revoke all confirm modal -->
    <PixelModal :open="showRevokeAll" title="REVOKE ALL SESSIONS?" @close="showRevokeAll = false">
      <div class="flex flex-col gap-4">
        <p class="font-body text-murder-text text-sm">You'll be logged out everywhere.</p>
        <ErrorBanner :message="revokeError" />
        <div class="flex gap-2">
          <PixelButton variant="danger" size="sm" class="flex-1" :loading="revokeLoading" @click="revokeAllSessions">
            CONFIRM
          </PixelButton>
          <PixelButton variant="ghost" size="sm" @click="showRevokeAll = false">
            CANCEL
          </PixelButton>
        </div>
      </div>
    </PixelModal>
  </div>
</template>
