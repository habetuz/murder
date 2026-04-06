import { createRouter, createWebHistory } from 'vue-router';
import StartView from '../views/StartView.vue';
import GameView from '../views/GameView.vue';
import SettingsView from '../views/SettingsView.vue';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'start',
      component: StartView,
    },
    {
      path: '/game/:gameId',
      name: 'game',
      component: GameView,
      meta: { requiresAuth: true },
    },
    {
      path: '/games/:gameId',
      redirect: (to) => ({ name: 'game', params: { gameId: to.params.gameId } }),
    },
    {
      path: '/settings',
      name: 'settings',
      component: SettingsView,
      meta: { requiresAuth: true, requiresUser: true },
    },
  ],
});

router.beforeEach(async (to) => {
  const auth = useAuthStore();
  await auth.bootstrap();

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'start' };
  }

  if (to.meta.requiresUser && !auth.isUser) {
    return { name: 'start' };
  }

  return true;
});

export default router;
