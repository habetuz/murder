<script setup lang="ts">
import { ref } from 'vue'
import type { ParticipantDto } from '../../types/api'
import { ApiError } from '../../api/client'

const props = defineProps<{
  participants: ParticipantDto[]
  adminPlayerId: string | null
  currentPlayerId: string | null
  gameState?: 'pending' | 'running' | 'ended'
  canKick?: boolean
  onKick?: (playerId: string) => Promise<void>
  canRestore?: boolean
  onRestore?: (playerId: string) => Promise<string>
}>()

const kickingId = ref<string | null>(null)
const kickError = ref<string | null>(null)
const restoringId = ref<string | null>(null)
const restoreError = ref<string | null>(null)
const restoreLink = ref<{ playerId: string; url: string } | null>(null)
const copied = ref(false)

function canKickPlayer(p: ParticipantDto): boolean {
  return !!(
    props.canKick &&
    props.gameState === 'pending' &&
    p.id !== props.adminPlayerId &&
    p.id !== props.currentPlayerId
  )
}

function canRestorePlayer(p: ParticipantDto): boolean {
  return !!(
    props.canRestore &&
    p.kind === 'guest' &&
    p.id !== props.currentPlayerId
  )
}

async function kick(playerId: string) {
  if (!props.onKick) return
  kickingId.value = playerId
  kickError.value = null
  try {
    await props.onKick(playerId)
  } catch (e) {
    kickError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to kick'
  } finally {
    kickingId.value = null
  }
}

async function restore(playerId: string) {
  if (!props.onRestore) return
  restoringId.value = playerId
  restoreError.value = null
  restoreLink.value = null
  copied.value = false
  try {
    const token = await props.onRestore(playerId)
    const url = `${window.location.origin}/api/auth/restore/${token}`
    restoreLink.value = { playerId, url }
  } catch (e) {
    restoreError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Failed to generate link'
  } finally {
    restoringId.value = null
  }
}

async function copyLink() {
  if (!restoreLink.value) return
  try {
    await navigator.clipboard.writeText(restoreLink.value.url)
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
  } catch {
    // Fallback: select the text
  }
}

function dismissLink() {
  restoreLink.value = null
  copied.value = false
}
</script>

<template>
  <div class="flex flex-col gap-2">
    <p v-if="kickError" class="font-body text-murder-danger text-xs">{{ kickError }}</p>
    <p v-if="restoreError" class="font-body text-murder-danger text-xs">{{ restoreError }}</p>
    <div
      v-for="p in participants"
      :key="p.id"
      class="flex flex-col"
    >
      <div
        :class="[
          'flex items-center justify-between px-3 py-2 border-2',
          p.id === currentPlayerId
            ? 'border-murder-accent bg-murder-accent/10'
            : 'border-murder-border bg-murder-surface',
        ]"
      >
        <div class="flex items-center gap-2 min-w-0">
          <span class="font-body text-murder-text text-sm truncate">{{ p.name }}</span>
          <span
            v-if="p.username"
            class="font-body text-murder-dim text-xs truncate"
          >@{{ p.username }}</span>
          <span
            v-if="p.id === adminPlayerId"
            class="font-pixel text-[6px] text-murder-warn border border-murder-warn px-1 shrink-0"
          >ADMIN</span>
          <span
            v-if="p.id === currentPlayerId"
            class="font-pixel text-[6px] text-murder-accent border border-murder-accent px-1 shrink-0"
          >YOU</span>
          <span
            v-if="p.kind === 'guest'"
            class="font-pixel text-[6px] text-murder-dim border border-murder-dim px-1 shrink-0"
          >GUEST</span>
        </div>
        <div class="flex items-center gap-1 shrink-0 ml-2">
          <button
            v-if="canRestorePlayer(p)"
            :disabled="restoringId === p.id"
            class="font-pixel text-[7px] text-murder-accent border border-murder-accent px-1.5 py-0.5 hover:bg-murder-accent/20 transition-colors disabled:opacity-50"
            @click="restore(p.id)"
          >
            {{ restoringId === p.id ? '...' : 'RESTORE' }}
          </button>
          <button
            v-if="canKickPlayer(p)"
            :disabled="kickingId === p.id"
            class="font-pixel text-[7px] text-murder-danger border border-murder-danger px-1.5 py-0.5 hover:bg-murder-danger/20 transition-colors disabled:opacity-50"
            @click="kick(p.id)"
          >
            {{ kickingId === p.id ? '...' : 'KICK' }}
          </button>
        </div>
      </div>
      <!-- Restore link display -->
      <div
        v-if="restoreLink && restoreLink.playerId === p.id"
        class="border-2 border-murder-accent/50 bg-murder-surface px-3 py-2 flex flex-col gap-1.5"
      >
        <p class="font-pixel text-[6px] text-murder-accent">RESTORE LINK (single-use, 15 min)</p>
        <div class="flex items-center gap-2">
          <input
            :value="restoreLink.url"
            readonly
            class="flex-1 bg-murder-bg border border-murder-border text-murder-text font-body text-xs px-2 py-1 select-all outline-none"
            @click="($event.target as HTMLInputElement).select()"
          />
          <button
            class="font-pixel text-[7px] border px-1.5 py-0.5 transition-colors shrink-0"
            :class="copied ? 'text-murder-accent border-murder-accent bg-murder-accent/20' : 'text-murder-text border-murder-border hover:bg-murder-border/20'"
            @click="copyLink"
          >
            {{ copied ? 'COPIED' : 'COPY' }}
          </button>
          <button
            class="font-pixel text-[7px] text-murder-dim border border-murder-dim px-1.5 py-0.5 hover:bg-murder-dim/20 transition-colors shrink-0"
            @click="dismissLink"
          >
            X
          </button>
        </div>
      </div>
    </div>
    <p v-if="!participants.length" class="font-pixel text-[8px] text-murder-dim text-center py-4">
      NO PLAYERS YET
    </p>
  </div>
</template>
