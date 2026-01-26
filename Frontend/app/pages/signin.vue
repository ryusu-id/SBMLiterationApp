<template>
  <div
    class="flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UPageCard class="w-full max-w-md">
      <UAuthForm
        title="Login"
        description="Enter your credentials to access your account."
        icon="i-lucide-user"
        :providers="providers"
        :loading="loading"
      />
    </UPageCard>
  </div>
</template>

<script setup lang="ts">
import { useAuth } from '~/apis/api'

definePageMeta({
  layout: 'landing',
  middleware: [
    function () {
      if (import.meta.client)
        return

      const auth = useAuth()
      if (!auth.getToken())
        return
      if (auth.getRoles() && auth.getRoles().includes('admin')) {
        return '/admin'
      } else if (auth.getRoles()) {
        return '/dashboard'
      }
    }
  ]
})

const loading = ref(false)

const $api = useNuxtApp().$backendApi
const providers = ref([
  {
    loading: loading,
    label: 'Google',
    icon: 'i-simple-icons-google',
    onClick: async () => {
      try {
        loading.value = true
        const result = await $api<{ authUrl: string }>('/auth/google/url')

        if (result.authUrl)
          window.location.href = result.authUrl
      } catch {
        useToast().add({
          title: 'Error',
          description: 'Failed to initiate Google sign-in.',
          color: 'error',
          icon: 'i-lucide-circle-off'
        })
      } finally {
        loading.value = false
      }
    }
  }
])
</script>
