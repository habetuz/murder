import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { api } from '../api/client';
import type { GameDto } from '../types/api';

export const useGamesStore = defineStore('games', () => {
  const myGames = ref<GameDto[]>([]);
  const loadingMyGames = ref(false);

  const activeGames = computed(() => myGames.value.filter((game) => game.state !== 'ended'));
  const historyGames = computed(() => myGames.value.filter((game) => game.state === 'ended'));

  async function fetchMyGames() {
    loadingMyGames.value = true;
    try {
      const response = await api.listMyGames();
      const gameDetails = await Promise.allSettled(response.games.map((id) => api.getGame(id)));
      myGames.value = gameDetails
        .flatMap((result) => (result.status === 'fulfilled' ? [result.value.game] : []))
        .sort((a, b) => b.name.localeCompare(a.name));
    } finally {
      loadingMyGames.value = false;
    }
  }

  function getGame(gameId: string) {
    return api.getGame(gameId).then((response) => response.game);
  }

  function getParticipants(gameId: string) {
    return api.participants(gameId).then((response) => response.participants);
  }

  function getLeaderboard(gameId: string) {
    return api.leaderboard(gameId).then((response) => response.entries);
  }

  function getVictim(gameId: string) {
    return api.victim(gameId).then((response) => response.victimPlayerId);
  }

  async function createGame(payload: { name: string; description?: string; endTime?: string }) {
    const response = await api.createGame(payload);
    await fetchMyGames();
    return response.game;
  }

  async function joinGame(gameId: string) {
    await api.joinGame(gameId);
    await fetchMyGames();
  }

  async function leaveGame(gameId: string, refresh = true) {
    await api.leaveGame(gameId);
    if (refresh) {
      await fetchMyGames();
    }
  }

  return {
    myGames,
    loadingMyGames,
    activeGames,
    historyGames,
    fetchMyGames,
    getGame,
    getParticipants,
    getLeaderboard,
    getVictim,
    createGame,
    joinGame,
    leaveGame,
    api,
  };
});
