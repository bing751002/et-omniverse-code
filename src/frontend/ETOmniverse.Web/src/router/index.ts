import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import Login from '../pages/Login.vue'
import Welcome from '../pages/Welcome.vue'

const routes: RouteRecordRaw[] = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'login', component: Login },
  { path: '/welcome', name: 'welcome', component: Welcome },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

export default router
