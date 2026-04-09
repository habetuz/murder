import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api, ApiError } from '../api/client'
import { useAuthStore } from './auth'
import type { GameDto, ParticipantDto, LeaderboardEntry, CreateGameRequest, PatchGameRequest, StartGameRequest } from '../types/api'

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
  const loadingGame = ref(false)
  const gameError = ref<string | null>(null)

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

  async function fetchGame(gameId: string) {
    const { game } = await api.getGame(gameId)
    currentGame.value = game
    return game
  }

  async function fetchParticipants(gameId: string) {
    const { participants } = await api.getParticipants(gameId)
    currentParticipants.value = participants
    return participants
  }

  async function fetchLeaderboard(gameId: string) {
    const { entries } = await api.getLeaderboard(gameId)
    currentLeaderboard.value = entries
    return entries
  }

  async function fetchVictim(gameId: string) {
    try {
      const { victimPlayerId, victimName } = await api.getVictim(gameId)
      currentVictimId.value = victimPlayerId
      currentVictimName.value = victimName
      isDead.value = false
    } catch (e) {
      if (e instanceof ApiError && e.is('player-dead')) {
        isDead.value = true
        currentVictimId.value = null
        currentVictimName.value = null
      } else if (e instanceof ApiError && (e.is('no-more-victims') || e.is('invalid-game-state'))) {
        currentVictimId.value = null
        currentVictimName.value = null
      } else {
        throw e
      }
    }
  }

  async function refreshAll(gameId: string) {
    try {
      const game = await fetchGame(gameId)
      await fetchParticipants(gameId)

      if (game.state === 'running' || game.state === 'ended') {
        await Promise.allSettled([
          fetchLeaderboard(gameId),
          game.state === 'running' ? fetchVictim(gameId) : Promise.resolve(),
        ])
      }
      gameError.value = null
    } catch (e) {
      if (e instanceof ApiError) {
        if (e.status === 401) throw e // propagate so global handler fires
        gameError.value = e.problem.detail ?? e.message
      } else {
        gameError.value = 'Network error'
      }
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
    await fetchGame(gameId)
  }

  async function changeEnd(gameId: string, endTime: string) {
    await api.changeEnd(gameId, { endTime })
    await fetchGame(gameId)
  }

  async function submitKill(gameId: string, victimPlayerId: string) {
    const result = await api.submitKill(gameId, { victimPlayerId })
    // Update victim immediately
    currentVictimId.value = result.nextVictimPlayerId
    currentVictimName.value = result.nextVictimName
    return result
  }

  function clearCurrentGame() {
    currentGame.value = null
    currentParticipants.value = []
    currentLeaderboard.value = []
    currentVictimId.value = null
    currentVictimName.value = null
    isDead.value = false
    gameError.value = null
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
    loadingGame,
    gameError,
    isAdmin,
    participantMap,
    usernameMap,
    activeGames,
    historyGames,
    fetchMyGames,
    fetchGame,
    fetchParticipants,
    fetchLeaderboard,
    fetchVictim,
    refreshAll,
    createGame,
    joinGame,
    leaveGame,
    startGame,
    endGame,
    deleteGame,
    patchGame,
    changeEnd,
    submitKill,
    clearCurrentGame,
  }
})
