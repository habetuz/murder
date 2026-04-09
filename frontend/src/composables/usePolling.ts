import { ref, watch, onUnmounted, isRef, type MaybeRef } from 'vue'

interface UsePollingOptions {
  immediate?: boolean
  pauseOnHidden?: boolean
}

export function usePolling(
  callback: () => Promise<void>,
  intervalMs: MaybeRef<number>,
  options: UsePollingOptions = {},
) {
  const { immediate = true, pauseOnHidden = true } = options
  const isActive = ref(false)
  let timer: ReturnType<typeof setInterval> | null = null
  let inFlight = false

  async function run() {
    if (inFlight) return
    if (pauseOnHidden && document.visibilityState === 'hidden') return
    inFlight = true
    try {
      await callback()
    } catch {
      // polling errors are handled inside callback
    } finally {
      inFlight = false
    }
  }

  function getInterval() {
    return isRef(intervalMs) ? intervalMs.value : intervalMs
  }

  function start() {
    if (isActive.value) return
    isActive.value = true
    if (immediate) run()
    timer = setInterval(run, getInterval())
  }

  function stop() {
    isActive.value = false
    if (timer !== null) {
      clearInterval(timer)
      timer = null
    }
  }

  function onVisibilityChange() {
    if (!isActive.value) return
    if (document.visibilityState === 'visible') {
      run() // immediate refresh on becoming visible
    }
  }

  if (pauseOnHidden) {
    document.addEventListener('visibilitychange', onVisibilityChange)
  }

  // If interval changes, restart
  if (isRef(intervalMs)) {
    watch(intervalMs, () => {
      if (isActive.value) {
        stop()
        start()
      }
    })
  }

  onUnmounted(() => {
    stop()
    if (pauseOnHidden) {
      document.removeEventListener('visibilitychange', onVisibilityChange)
    }
  })

  return { start, stop, isActive }
}
