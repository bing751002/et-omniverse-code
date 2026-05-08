<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const username = ref('')
const password = ref('')

onMounted(() => {
  document.title = 'Login — ET-Omniverse'
})

function onSubmit() {
  router.push('/welcome')
}
</script>

<template>
  <h1>Login</h1>
  <form @submit.prevent="onSubmit">
    <label>
      Username
      <input v-model="username" type="text" name="username" autocomplete="username" />
    </label>
    <label>
      Password
      <input v-model="password" type="password" name="password" autocomplete="current-password" />
    </label>
    <button type="submit">Log in</button>
  </form>
</template>

<style scoped>
form {
  display: grid;
  gap: 8px;
  max-width: 320px;
}
label {
  display: grid;
  gap: 4px;
}
</style>
