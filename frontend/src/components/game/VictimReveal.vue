<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  victimName: string | null
  victimUsername?: string | null
  loading?: boolean
}>()

const COLS = 10
const ROWS = 4
const TOTAL = COLS * ROWS

// Each cell: true = hidden, false = revealed
const cells = ref<boolean[]>(Array(TOTAL).fill(true))

const revealedCount = computed(() => cells.value.filter(c => !c).length)
const isFullyRevealed = computed(() => revealedCount.value === TOTAL)

function revealCell(index: number) {
  if (cells.value[index]) {
    cells.value[index] = false
    // Auto-reveal all if >50% uncovered
    if (revealedCount.value > TOTAL * 0.5 && !isFullyRevealed.value) {
      revealAll()
    }
  }
}

function revealAll() {
  cells.value = Array(TOTAL).fill(false)
}

function rehide() {
  cells.value = Array(TOTAL).fill(true)
}

// Touch/drag support: track which cells we've dragged over
let isDragging = false

function onPointerDown(index: number) {
  isDragging = true
  revealCell(index)
}

function onPointerEnter(index: number) {
  if (isDragging) revealCell(index)
}

function onPointerUp() {
  isDragging = false
}
</script>

<template>
  <div class="flex flex-col items-center gap-4" @pointerup="onPointerUp" @pointerleave="onPointerUp">
    <p class="font-pixel text-[8px] text-murder-dim uppercase tracking-widest">Your Target</p>

    <div class="relative w-full max-w-xs select-none">
      <!-- Victim name underneath -->
      <div
        class="flex items-center justify-center bg-murder-bg border-2 border-murder-danger shadow-pixel-danger"
        style="min-height: 80px;"
      >
        <span v-if="loading" class="font-pixel text-[8px] text-murder-dim">LOADING...</span>
        <div v-else-if="victimName" class="flex flex-col items-center gap-1 px-2">
          <span class="font-pixel text-murder-danger text-sm text-center break-all">
            {{ victimName }}
          </span>
          <span v-if="victimUsername" class="font-body text-murder-dim text-xs">
            @{{ victimUsername }}
          </span>
        </div>
        <span v-else class="font-pixel text-[8px] text-murder-dim">???</span>
      </div>

      <!-- Scratch grid overlay -->
      <div
        v-if="!isFullyRevealed"
        class="absolute inset-0 grid touch-none"
        :style="{ gridTemplateColumns: `repeat(${COLS}, 1fr)`, gridTemplateRows: `repeat(${ROWS}, 1fr)` }"
      >
        <div
          v-for="(hidden, i) in cells"
          :key="i"
          :class="[
            'cursor-crosshair transition-all duration-200',
            hidden
              ? 'bg-murder-surface-light border border-murder-surface'
              : 'opacity-0 pointer-events-none',
          ]"
          @pointerdown.prevent="onPointerDown(i)"
          @pointerenter="onPointerEnter(i)"
        />
      </div>
    </div>

    <div class="flex gap-3">
      <button
        v-if="!isFullyRevealed"
        class="font-pixel text-[7px] text-murder-dim hover:text-murder-accent transition-colors"
        @click="revealAll"
      >
        REVEAL ALL
      </button>
      <button
        v-if="isFullyRevealed"
        class="font-pixel text-[7px] text-murder-dim hover:text-murder-warn transition-colors"
        @click="rehide"
      >
        RE-HIDE
      </button>
    </div>
  </div>
</template>
