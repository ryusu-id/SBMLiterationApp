<script setup lang="ts">
import { useAuth, useBackendFetch } from '~/apis/api'

definePageMeta({
  layout: 'landing'
})

const route = useRoute()
const router = useRouter()
const authStore = useAuth()

// Get authorization code and state from Google redirect
const code = route.query.code as string
const state = route.query.state as string

const loading = ref(true)
const error = ref<string | null>(null)

if (!code || !state) {
  error.value = 'Missing authorization code or state parameter'
  loading.value = false
}

try {
  // Send the authorization code and state to backend
  const { data, status, execute } = await useBackendFetch<{ accessToken: string, refreshToken: string }>('auth/google/callback', {
    method: 'POST',
    body: {
      code,
      state
    },
    lazy: true
  })

  // Handle successful authorization
  // Redirect to home or dashboard
  onMounted(async () => {
    await execute()
    if (status.value === 'success') {
      authStore.setToken(data.value!.accessToken)
      authStore.setRefreshToken(data.value!.refreshToken)

      if (authStore.getRoles().includes('admin'))
        router.push('/admin')
      else
        router.push('/dashboard')
    } else
      error.value = 'Authorization failed, please try again.'
  })
} catch (err: unknown) {
  // Handle authorization error
  console.error('Authorization failed:', err)
  const errorObj = err as { data?: { message?: string }, message?: string }
  error.value = errorObj.data?.message || errorObj.message || 'Authorization failed'
  loading.value = false
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4">
    <div class="max-w-md w-full text-center">
      <!-- Loading State -->
      <div v-if="loading">
        <h1>Processing Authorization...</h1>
        <p>Please wait while we complete your sign in.</p>
        <!-- Add loading spinner here -->
      </div>

      <!-- Error State -->
      <div v-else-if="error">
        <h1>Authorization Failed</h1>
        <p>{{ error }}</p>
        <!-- Add error content here -->

        <div class="mt-6">
          <button @click="router.push('/signin')">
            Back to Sign In
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
