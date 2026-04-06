import { createApp } from 'vue';
import { createPinia } from 'pinia';
import PrimeVue from 'primevue/config';
import Aura from '@primeuix/themes/aura';
import { registerSW } from 'virtual:pwa-register';
import App from './App.vue';
import router from './router';
import './style.css';
import 'primeicons/primeicons.css';

registerSW({ immediate: true });

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(PrimeVue, {
	theme: {
		preset: Aura,
	},
});

app.mount('#app');
