<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import type { GameDto } from '../../types/api'
import { useGameStore } from '../../stores/game'
import { ApiError } from '../../api/client'
import PixelButton from '../ui/PixelButton.vue'
import PixelInput from '../ui/PixelInput.vue'
import PixelModal from '../ui/PixelModal.vue'
import ErrorBanner from '../ui/ErrorBanner.vue'

const props = defineProps<{
  game: GameDto
}>()

const gameStore = useGameStore()
const router = useRouter()

// Edit modal
const showEdit = ref(false)
const editName = ref(props.game.name)
const editDescription = ref(props.game.description ?? '')
const editEndTime = ref(props.game.endTime ? props.game.endTime.slice(0, 16) : '')
const editLoading = ref(false)
const editError = ref<string | null>(null)

// Start
const startLoading = ref(false)
const startError = ref<string | null>(null)

// End / Delete
const showEndConfirm = ref(false)
const showDeleteConfirm = ref(false)
const actionLoading = ref(false)
const actionError = ref<string | null>(null)

async function saveEdit() {
  editLoading.value = true
  editError.value = null
  try {
    await gameStore.patchGame(props.game.id, {
      name: editName.value,
      description: editDescription.value || undefined,
      endTime: editEndTime.value ? new Date(editEndTime.value).toISOString() : undefined,
    })
    showEdit.value = false
  } catch (e) {
    editError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error saving'
  } finally {
    editLoading.value = false
  }
}

async function startGame() {
  startLoading.value = true
  startError.value = null
  try {
    await gameStore.startGame(props.game.id)
  } catch (e) {
    startError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error starting game'
  } finally {
    startLoading.value = false
  }
}

async function endGame() {
  actionLoading.value = true
  actionError.value = null
  try {
    await gameStore.endGame(props.game.id)
    showEndConfirm.value = false
  } catch (e) {
    actionError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error ending game'
  } finally {
    actionLoading.value = false
  }
}

async function deleteGame() {
  actionLoading.value = true
  actionError.value = null
  try {
    await gameStore.deleteGame(props.game.id)
    router.push({ name: 'landing' })
  } catch (e) {
    actionError.value = e instanceof ApiError ? (e.problem.detail ?? e.message) : 'Error deleting game'
  } finally {
    actionLoading.value = false
  }
}
</script>

<template>
  <div class="flex flex-col gap-3">
    <p class="font-pixel text-[8px] text-murder-warn uppercase tracking-widest">Admin Controls</p>

    <ErrorBanner :message="startError ?? actionError" />

    <!-- Pending state actions -->
    <template v-if="game.state === 'pending'">
      <PixelButton variant="primary" size="lg" :loading="startLoading" @click="startGame">
        START GAME
      </PixelButton>
      <div class="flex gap-2">
        <PixelButton variant="ghost" size="sm" class="flex-1" @click="showEdit = true">
          EDIT
        </PixelButton>
        <PixelButton variant="danger" size="sm" @click="showDeleteConfirm = true">
          DELETE
        </PixelButton>
      </div>
    </template>

    <!-- Running state actions -->
    <template v-if="game.state === 'running'">
      <PixelButton variant="warn" size="md" @click="showEdit = true">
        CHANGE END TIME
      </PixelButton>
      <PixelButton variant="danger" size="md" @click="showEndConfirm = true">
        END GAME NOW
      </PixelButton>
    </template>

    <!-- Edit modal -->
    <PixelModal :open="showEdit" title="EDIT GAME" @close="showEdit = false">
      <div class="flex flex-col gap-4">
        <ErrorBanner :message="editError" />
        <PixelInput v-model="editName" label="Game Name" placeholder="Office Round" />
        <PixelInput v-model="editDescription" label="Description (optional)" placeholder="Spring event" />
        <PixelInput
          v-model="editEndTime"
          label="End Time (optional)"
          type="datetime-local"
        />
        <div class="flex gap-2">
          <PixelButton variant="primary" size="sm" class="flex-1" :loading="editLoading" @click="saveEdit">
            SAVE
          </PixelButton>
          <PixelButton variant="ghost" size="sm" @click="showEdit = false">
            CANCEL
          </PixelButton>
        </div>
      </div>
    </PixelModal>

    <!-- End confirm modal -->
    <PixelModal :open="showEndConfirm" title="END GAME?" @close="showEndConfirm = false">
      <div class="flex flex-col gap-4">
        <p class="font-body text-murder-text text-sm">This will immediately end the game for all players.</p>
        <ErrorBanner :message="actionError" />
        <div class="flex gap-2">
          <PixelButton variant="danger" size="sm" class="flex-1" :loading="actionLoading" @click="endGame">
            END GAME
          </PixelButton>
          <PixelButton variant="ghost" size="sm" @click="showEndConfirm = false">
            CANCEL
          </PixelButton>
        </div>
      </div>
    </PixelModal>

    <!-- Delete confirm modal -->
    <PixelModal :open="showDeleteConfirm" title="DELETE GAME?" @close="showDeleteConfirm = false">
      <div class="flex flex-col gap-4">
        <p class="font-body text-murder-text text-sm">This will permanently delete the game.</p>
        <ErrorBanner :message="actionError" />
        <div class="flex gap-2">
          <PixelButton variant="danger" size="sm" class="flex-1" :loading="actionLoading" @click="deleteGame">
            DELETE
          </PixelButton>
          <PixelButton variant="ghost" size="sm" @click="showDeleteConfirm = false">
            CANCEL
          </PixelButton>
        </div>
      </div>
    </PixelModal>
  </div>
</template>
