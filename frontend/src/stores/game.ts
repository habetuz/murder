import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api, watchGame as apiWatchGame } from '../api/client'
import { useAuthStore } from './auth'
import type { GameDto, ParticipantDto, LeaderboardEntry, WatchPayload, CreateGameRequest, PatchGameRequest, StartGameRequest } from '../types/api'

export const useGameStore = defineStore('game', () => {
  const authStore = useAuthStore()

  // My games list
  const myGameIds = ref<string[]>([])
  const myGames = ref<GameDto[]>([])
  const loadingMyGames = ref(false)

  // Current game
  const currentGame = ref<GameDto | null>(null)
  const currentParticipants = ref<ParticipantDto[]>([])
  const currentLeaderboard = ref<LeaderboardEntry[]>([])
  const currentVictimId = ref<string | null>(null)
  const currentVictimName = ref<string | null>(null)
  const isDead = ref(false)
  const pendingKill = ref(false)
  const pendingKillSent = ref(false)
  const loadingGame = ref(false)
  const gameError = ref<string | null>(null)
  const gameDeleted = ref(false)

  // SSE watch handle
  let closeWatch: (() => void) | null = null

  const isAdmin = computed(() =>
    currentGame.value !== null &&
    authStore.player !== null &&
    currentGame.value.adminPlayerId === authStore.player.id,
  )

  const participantMap = computed(() => {
    const map = new Map<string, string>()
    for (const p of currentParticipants.value) {
      map.set(p.id, p.name)
    }
    return map
  })

  const usernameMap = computed(() => {
    const map = new Map<string, string | null>()
    for (const p of currentParticipants.value) {
      map.set(p.id, p.username)
    }
    return map
  })

  const activeGames = computed(() => myGames.value.filter(g => g.state !== 'ended'))
  const historyGames = computed(() => myGames.value.filter(g => g.state === 'ended'))

  async function fetchMyGames() {
    loadingMyGames.value = true
    try {
      const { games } = await api.getMyGames()
      myGameIds.value = games
      const results = await Promise.allSettled(games.map(id => api.getGame(id)))
      myGames.value = results
        .filter((r): r is PromiseFulfilledResult<{ game: GameDto }> => r.status === 'fulfilled')
        .map(r => r.value.game)
    } finally {
      loadingMyGames.value = false
    }
  }

  function applyWatchPayload(payload: WatchPayload) {
    currentGame.value = payload.game
    currentParticipants.value = payload.participants
    currentLeaderboard.value = payload.leaderboard ?? []
    currentVictimId.value = payload.me.victimPlayerId
    currentVictimName.value = payload.me.victimName
    isDead.value = !payload.me.alive
    pendingKill.value = payload.me.pendingKill
    pendingKillSent.value = payload.me.pendingKillSent
    gameError.value = null
  }

  function startWatch(gameId: string) {
    stopWatch()
    loadingGame.value = true
    gameDeleted.value = false

    closeWatch = apiWatchGame(gameId, {
      onSync(payload) {
        loadingGame.value = false
        applyWatchPayload(payload)
      },
      onUpdate(payload) {
        applyWatchPayload(payload)
      },
      onDeleted() {
        gameDeleted.value = true
        currentGame.value = null
        stopWatch()
      },
      onError(error) {
        loadingGame.value = false
        gameError.value = error
      },
    })
  }

  function stopWatch() {
    if (closeWatch) {
      closeWatch()
      closeWatch = null
    }
  }

  async function createGame(body: CreateGameRequest): Promise<GameDto> {
    const { game } = await api.createGame(body)
    myGames.value.unshift(game)
    return game
  }

  async function joinGame(gameId: string, name: string) {
    await api.joinGame(gameId, { name })
  }

  async function leaveGame(gameId: string) {
    await api.leaveGame(gameId)
    myGames.value = myGames.value.filter(g => g.id !== gameId)
    if (currentGame.value?.id === gameId) {
      currentGame.value = null
    }
  }

  async function kickPlayer(gameId: string, playerId: string) {
    await api.kickPlayer(gameId, playerId)
  }

  async function generateRestoreToken(gameId: string, playerId: string): Promise<string> {
    const { token } = await api.generateRestoreToken(gameId, playerId)
    return token
  }

  async function startGame(gameId: string, body?: StartGameRequest) {
    await api.startGame(gameId, body)
  }

  async function endGame(gameId: string) {
    await api.endGame(gameId)
  }

  async function deleteGame(gameId: string) {
    await api.deleteGame(gameId)
    myGames.value = myGames.value.filter(g => g.id !== gameId)
    if (currentGame.value?.id === gameId) {
      currentGame.value = null
    }
  }

  async function patchGame(gameId: string, body: PatchGameRequest) {
    await api.patchGame(gameId, body)
  }

  async function changeEnd(gameId: string, endTime: string) {
    await api.changeEnd(gameId, { endTime })
  }

  async function submitKill(gameId: string, victimPlayerId: string) {
    const result = await api.submitKill(gameId, { victimPlayerId })
    // Kill is now pending — SSE will update pendingKillSent
    return result
  }

  async function respondToKill(gameId: string, accepted: boolean) {
    const result = await api.respondToKill(gameId, { accepted })
    // SSE will update state after response
    return result
  }

  function clearCurrentGame() {
    stopWatch()
    currentGame.value = null
    currentParticipants.value = []
    currentLeaderboard.value = []
    currentVictimId.value = null
    currentVictimName.value = null
    isDead.value = false
    pendingKill.value = false
    pendingKillSent.value = false
    gameError.value = null
    gameDeleted.value = false
  }

  return {
    myGameIds,
    myGames,
    loadingMyGames,
    currentGame,
    currentParticipants,
    currentLeaderboard,
    currentVictimId,
    currentVictimName,
    isDead,
    pendingKill,
    pendingKillSent,
    loadingGame,
    gameError,
    gameDeleted,
    isAdmin,
    participantMap,
    usernameMap,
    activeGames,
    historyGames,
    fetchMyGames,
    startWatch,
    stopWatch,
    createGame,
    joinGame,
    leaveGame,
    kickPlayer,
    generateRestoreToken,
    startGame,
    endGame,
    deleteGame,
    patchGame,
    changeEnd,
    submitKill,
    respondToKill,
    clearCurrentGame,
  }
})
