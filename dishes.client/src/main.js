import './assets/main.css'

import { createApp } from 'vue'
import { VueQueryPlugin } from '@tanstack/vue-query'

import App from './App.vue'

App.use(VueQueryPlugin);

createApp(App).mount('#app')
