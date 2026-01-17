<script setup lang="ts">
const route = useRoute()
const router = useRouter()

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
  console.log('Sending authorization code and state to backend:', { code, state })
  const { data, status } = await useFetch('/api/v1/auth/google/callback', {
    method: 'POST',
    body: {
      code,
      state
    }
  })

  console.log('response data', data.value, status.value)

  // Handle successful authorization
  // Redirect to home or dashboard
  if (status.value === 'success')
    router.push('/')
  else
    error.value = 'Authorization failed, please try again.'
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
