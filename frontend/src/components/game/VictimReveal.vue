<script setup lang="ts">
import { ref, computed } from 'vue'

const props = defineProps<{
  victimName: string | null
  victimUsername?: string | null
  loading?: boolean
}>()

const COLS = 20
const ROWS = 8
const TOTAL = COLS * ROWS
const REAPPEAR_DELAY = 300

// Each cell: true = hidden, false = revealed
const cells = ref<boolean[]>(Array(TOTAL).fill(true))
const timers = new Map<number, ReturnType<typeof setTimeout>>()

const revealedCount = computed(() => cells.value.filter(c => !c).length)
const isFullyRevealed = computed(() => revealedCount.value === TOTAL)

function revealCell(index: number) {
  if (!cells.value[index]) return

  cells.value[index] = false

  // Clear existing timer for this cell if any
  const existing = timers.get(index)
  if (existing) clearTimeout(existing)

  // Schedule reappearance
  timers.set(index, setTimeout(() => {
    cells.value[index] = true
    timers.delete(index)
  }, REAPPEAR_DELAY))

  // Auto-reveal all if >60% uncovered
  if (revealedCount.value > TOTAL * 0.6 && !isFullyRevealed.value) {
    revealAll()
  }
}

function revealAll() {
  // Cancel all reappear timers
  for (const t of timers.values()) clearTimeout(t)
  timers.clear()
  cells.value = Array(TOTAL).fill(false)
}

function rehide() {
  for (const t of timers.values()) clearTimeout(t)
  timers.clear()
  cells.value = Array(TOTAL).fill(true)
}

// Touch/drag support: track pointer state and use pointermove for mobile
let isDragging = false

function onPointerDown(index: number, e: PointerEvent) {
  isDragging = true
  // Capture pointer so pointermove keeps firing even outside the element
  ;(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId)
  revealCell(index)
}

function onPointerMove(e: PointerEvent) {
  if (!isDragging) return
  const el = document.elementFromPoint(e.clientX, e.clientY) as HTMLElement | null
  if (!el) return
  const idx = el.dataset.cellIndex
  if (idx != null) revealCell(Number(idx))
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
        @pointermove="onPointerMove"
      >
        <div
          v-for="(hidden, i) in cells"
          :key="i"
          :data-cell-index="i"
          :class="[
            'cursor-crosshair transition-opacity',
            hidden
              ? 'bg-murder-surface-light border-[0.5px] border-murder-surface opacity-100 duration-300'
              : 'opacity-0 pointer-events-none duration-100',
          ]"
          @pointerdown.prevent="onPointerDown(i, $event)"
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
