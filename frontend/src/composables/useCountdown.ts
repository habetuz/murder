import { ref, computed, watch, onUnmounted, isRef, type MaybeRef } from 'vue'

export function useCountdown(endTime: MaybeRef<string | null>) {
  const remaining = ref<number | null>(null)
  let timer: ReturnType<typeof setInterval> | null = null

  function getEndTime() {
    return isRef(endTime) ? endTime.value : endTime
  }

  function update() {
    const et = getEndTime()
    if (!et) {
      remaining.value = null
      return
    }
    const diff = new Date(et).getTime() - Date.now()
    remaining.value = Math.max(0, diff)
  }

  function startTimer() {
    if (timer) clearInterval(timer)
    update()
    timer = setInterval(update, 1000)
  }

  if (isRef(endTime)) {
    watch(endTime, () => startTimer(), { immediate: true })
  } else {
    startTimer()
  }

  onUnmounted(() => {
    if (timer) clearInterval(timer)
  })

  const label = computed(() => {
    if (remaining.value === null) return null
    if (remaining.value <= 0) return "TIME'S UP"
    const total = Math.floor(remaining.value / 1000)
    const h = Math.floor(total / 3600)
    const m = Math.floor((total % 3600) / 60)
    const s = total % 60
    if (h > 0) return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  })

  const isUrgent = computed(() =>
    remaining.value !== null && remaining.value > 0 && remaining.value < 10 * 60 * 1000,
  )

  const isExpired = computed(() =>
    remaining.value !== null && remaining.value <= 0,
  )

  return { remaining, label, isUrgent, isExpired }
}
