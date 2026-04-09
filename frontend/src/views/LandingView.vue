<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useGameStore } from '../stores/game'
import { ApiError } from '../api/client'
import PixelButton from '../components/ui/PixelButton.vue'
import PixelInput from '../components/ui/PixelInput.vue'
import PixelCard from '../components/ui/PixelCard.vue'
import PixelBadge from '../components/ui/PixelBadge.vue'
import PixelModal from '../components/ui/PixelModal.vue'
import ErrorBanner from '../components/ui/ErrorBanner.vue'
import LoadingSpinner from '../components/ui/LoadingSpinner.vue'

const auth = useAuthStore()
const gameStore = useGameStore()
const router = useRouter()
const route = useRoute()

// ── Guest join form ──
const guestName = ref('')
const guestCode = ref((route.query.join as string) ?? '')
const guestLoading = ref(false)
const guestError = ref<string | null>(null)

// ── Login form ──
const showLogin = ref(false)
const loginUsername = ref('')
const loginPassword = ref('')
const loginLoading = ref(false)
const loginError = ref<string | null>(null)

// ── Register form ──
const showRegister = ref(false)
const registerName = ref('')
const registerPassword = ref('')
const registerConfirm = ref('')
const registerLoading = ref(false)
const registerError = ref<string | null>(null)

// ── Create game modal ──
const showCreate = ref(false)
const createName = ref('')
const createDisplayName = ref('')
const createDesc = ref('')
const createEndTime = ref('')
const createLoading = ref(false)
const createError = ref<string | null>(null)

// ── Join as user ──
const joinCode = ref((route.query.join as string) ?? '')
const joinDisplayName = ref('')
const joinLoading = ref(false)
const joinError = ref<string | null>(null)

onMounted(async () => {
  if (auth.isAuthenticated) {
    await gameStore.fetchMyGames()
  }
})

async function joinAsGuest() {
  if (!guestName.value.trim() || !guestCode.value.trim()) return
  guestLoading.value = true
  guestError.value = null
  try {
    const gameId = await auth.createGuest(guestName.value.trim(), guestCode.value.trim().toUpperCase())
    await gameStore.fetchMyGames()
    router.push({ name: 'game', params: { gameId } })
  } catch (e) {
    guestError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to join game'
  } finally {
    guestLoading.value = false
  }
}

async function login() {
  if (!loginUsername.value || !loginPassword.value) return
  loginLoading.value = true
  loginError.value = null
  try {
    await auth.login(loginUsername.value, loginPassword.value)
    showLogin.value = false
    await gameStore.fetchMyGames()
  } catch (e) {
    loginError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Login failed'
  } finally {
    loginLoading.value = false
  }
}

async function register() {
  if (!registerName.value.trim()) return
  if (registerPassword.value && registerPassword.value !== registerConfirm.value) return
  registerLoading.value = true
  registerError.value = null
  try {
    await auth.register(registerName.value.trim(), registerPassword.value || undefined)
    showRegister.value = false
    await gameStore.fetchMyGames()
  } catch (e) {
    registerError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Registration failed'
  } finally {
    registerLoading.value = false
  }
}

async function createGame() {
  if (!createName.value.trim() || !createDisplayName.value.trim()) return
  createLoading.value = true
  createError.value = null
  try {
    const game = await gameStore.createGame({
      name: createName.value.trim(),
      displayName: createDisplayName.value.trim(),
      description: createDesc.value.trim() || undefined,
      endTime: createEndTime.value ? new Date(createEndTime.value).toISOString() : undefined,
    })
    showCreate.value = false
    createName.value = ''
    createDisplayName.value = ''
    createDesc.value = ''
    createEndTime.value = ''
    router.push({ name: 'game', params: { gameId: game.id } })
  } catch (e) {
    createError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to create game'
  } finally {
    createLoading.value = false
  }
}

async function joinAsUser() {
  if (!joinCode.value.trim() || !joinDisplayName.value.trim()) return
  joinLoading.value = true
  joinError.value = null
  try {
    await gameStore.joinGame(joinCode.value.trim().toUpperCase(), joinDisplayName.value.trim())
    router.push({ name: 'game', params: { gameId: joinCode.value.trim().toUpperCase() } })
  } catch (e) {
    joinError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to join game'
  } finally {
    joinLoading.value = false
  }
}
</script>

<template>
  <div class="flex-1 flex flex-col items-center px-4 py-8 max-w-2xl mx-auto w-full gap-8">

    <!-- Flash message (session expired, kicked, etc.) -->
    <div
      v-if="auth.flashMessage"
      class="w-full bg-murder-warn/20 border-2 border-murder-warn text-murder-warn font-pixel text-[8px] px-4 py-3 flex items-center justify-between gap-3"
    >
      <span>{{ auth.flashMessage }}</span>
      <button class="text-murder-warn hover:text-murder-text leading-none shrink-0" @click="auth.clearFlash()">✕</button>
    </div>

    <!-- Hero -->
    <div class="text-center flex flex-col items-center gap-4">
      <h1 class="font-pixel text-murder-danger text-3xl md:text-4xl leading-tight">
        MURDER
      </h1>
      <p class="font-body text-murder-dim text-sm max-w-sm">
        Eliminate your target. Inherit theirs. Last one standing wins.
      </p>
      <div class="border-2 border-murder-border bg-murder-surface shadow-pixel p-4 text-left max-w-sm w-full">
        <p class="font-pixel text-[7px] text-murder-warn mb-3">HOW TO PLAY</p>
        <ol class="font-body text-murder-text text-xs space-y-2 list-decimal list-inside">
          <li>Join a game and get your secret target.</li>
          <li>Find your target IRL and pass them an object.</li>
          <li>They're eliminated — you inherit their target.</li>
          <li>Last player standing wins.</li>
        </ol>
      </div>
    </div>

    <!-- Unauthenticated: Guest Join -->
    <template v-if="!auth.isAuthenticated">
      <PixelCard title="JOIN A GAME" variant="accent" class="w-full">
        <div class="flex flex-col gap-4">
          <ErrorBanner :message="guestError" />
          <PixelInput v-model="guestName" label="Your Name" placeholder="Agent Smith" autocomplete="nickname" />
          <PixelInput
            v-model="guestCode"
            label="Game Code"
            placeholder="ABCDE"
            :maxlength="5"
            autocomplete="off"
          />
          <PixelButton
            variant="primary"
            size="lg"
            class="w-full"
            :loading="guestLoading"
            :disabled="!guestName.trim() || !guestCode.trim()"
            @click="joinAsGuest"
          >
            JOIN AS GUEST
          </PixelButton>
        </div>
      </PixelCard>

      <div class="flex gap-4 items-center w-full">
        <div class="flex-1 h-px bg-murder-border" />
        <span class="font-pixel text-[7px] text-murder-dim">OR</span>
        <div class="flex-1 h-px bg-murder-border" />
      </div>

      <div class="flex gap-3 w-full">
        <PixelButton variant="ghost" size="md" class="flex-1" @click="showLogin = true">
          LOG IN
        </PixelButton>
        <PixelButton variant="ghost" size="md" class="flex-1" @click="showRegister = true">
          REGISTER
        </PixelButton>
      </div>
    </template>

    <!-- Authenticated: My Games + Create/Join -->
    <template v-else>
      <PixelCard class="w-full">
        <div class="flex flex-col gap-4">
          <ErrorBanner :message="joinError" />
          <PixelInput
            v-model="joinDisplayName"
            label="Display Name"
            placeholder="Agent Smith"
            autocomplete="nickname"
          />
          <div class="flex gap-2">
            <div class="flex-1">
              <PixelInput
                v-model="joinCode"
                label="Game Code"
                placeholder="ABCDE"
                :maxlength="5"
                autocomplete="off"
              />
            </div>
            <div class="flex flex-col justify-end">
              <PixelButton
                variant="code"
                size="md"
                :loading="joinLoading"
                :disabled="!joinCode.trim() || !joinDisplayName.trim()"
                @click="joinAsUser"
              >
                JOIN
              </PixelButton>
            </div>
          </div>
          <PixelButton variant="primary" size="md" class="w-full" @click="showCreate = true">
            CREATE GAME
          </PixelButton>
        </div>
      </PixelCard>

      <!-- Active games -->
      <div v-if="gameStore.loadingMyGames" class="py-4">
        <LoadingSpinner />
      </div>
      <template v-else>
        <div v-if="gameStore.activeGames.length" class="w-full flex flex-col gap-2">
          <p class="font-pixel text-[8px] text-murder-dim uppercase">Active Games</p>
          <RouterLink
            v-for="g in gameStore.activeGames"
            :key="g.id"
            :to="{ name: 'game', params: { gameId: g.id } }"
            class="no-underline"
          >
            <div class="bg-murder-surface border-2 border-murder-border hover:border-murder-accent shadow-pixel px-4 py-3 flex items-center justify-between transition-colors cursor-pointer">
              <div>
                <p class="font-body text-murder-text text-sm font-semibold">{{ g.name }}</p>
                <p class="font-pixel text-[7px] text-murder-code">{{ g.id }}</p>
              </div>
              <PixelBadge :state="g.state" />
            </div>
          </RouterLink>
        </div>

        <div v-if="gameStore.historyGames.length" class="w-full flex flex-col gap-2">
          <p class="font-pixel text-[8px] text-murder-dim uppercase">History</p>
          <RouterLink
            v-for="g in gameStore.historyGames"
            :key="g.id"
            :to="{ name: 'game', params: { gameId: g.id } }"
            class="no-underline"
          >
            <div class="bg-murder-surface border-2 border-murder-border hover:border-murder-dim shadow-pixel px-4 py-3 flex items-center justify-between transition-colors cursor-pointer opacity-70">
              <div>
                <p class="font-body text-murder-text text-sm font-semibold">{{ g.name }}</p>
                <p class="font-pixel text-[7px] text-murder-code">{{ g.id }}</p>
              </div>
              <PixelBadge :state="g.state" />
            </div>
          </RouterLink>
        </div>
      </template>
    </template>

    <!-- Login Modal -->
    <PixelModal :open="showLogin" title="LOG IN" @close="showLogin = false">
      <div class="flex flex-col gap-4">
        <ErrorBanner :message="loginError" />
        <PixelInput v-model="loginUsername" label="Username" placeholder="agent007" autocomplete="username" />
        <PixelInput v-model="loginPassword" label="Password" type="password" placeholder="••••••••" autocomplete="current-password" />
        <PixelButton variant="primary" size="md" class="w-full" :loading="loginLoading" type="submit" @click="login">
          LOG IN
        </PixelButton>
      </div>
    </PixelModal>

    <!-- Register Modal -->
    <PixelModal :open="showRegister" title="CREATE ACCOUNT" @close="showRegister = false">
      <div class="flex flex-col gap-4">
        <ErrorBanner :message="registerError" />
        <PixelInput v-model="registerName" label="Username" placeholder="agent007" autocomplete="username" />
        <PixelInput v-model="registerPassword" label="Password" type="password" placeholder="••••••••" autocomplete="new-password" />
        <PixelInput v-model="registerConfirm" label="Confirm Password" type="password" placeholder="••••••••" autocomplete="new-password" />
        <p v-if="registerPassword && registerConfirm && registerPassword !== registerConfirm" class="font-pixel text-[7px] text-murder-danger">
          PASSWORDS DON'T MATCH
        </p>
        <PixelButton
          variant="primary"
          size="md"
          class="w-full"
          :loading="registerLoading"
          :disabled="!registerName.trim() || !registerPassword || registerPassword !== registerConfirm"
          @click="register"
        >
          CREATE ACCOUNT
        </PixelButton>
      </div>
    </PixelModal>

    <!-- Create Game Modal -->
    <PixelModal :open="showCreate" title="CREATE GAME" @close="showCreate = false">
      <div class="flex flex-col gap-4">
        <ErrorBanner :message="createError" />
        <PixelInput v-model="createName" label="Game Name" placeholder="Office Round" />
        <PixelInput v-model="createDisplayName" label="Your Display Name" placeholder="Agent Smith" autocomplete="nickname" />
        <PixelInput v-model="createDesc" label="Description (optional)" placeholder="Spring event" />
        <PixelInput v-model="createEndTime" label="End Time (optional)" type="datetime-local" />
        <PixelButton
          variant="primary"
          size="md"
          class="w-full"
          :loading="createLoading"
          :disabled="!createName.trim() || !createDisplayName.trim()"
          @click="createGame"
        >
          CREATE
        </PixelButton>
      </div>
    </PixelModal>
  </div>
</template>
