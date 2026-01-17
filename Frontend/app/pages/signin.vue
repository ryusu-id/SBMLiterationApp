<template>
  <div class="flex flex-col items-center justify-center gap-4 p-4">
    <UPageCard class="w-full max-w-md">
      <UAuthForm
        title="Login"
        description="Enter your credentials to access your account."
        icon="i-lucide-user"
        :providers="providers"
      />
    </UPageCard>
  </div>
</template>

<script setup lang="ts">
const providers = [
  {
    label: 'Google',
    icon: 'i-simple-icons-google',
    onClick: async () => {
      const { data, status } = await useFetch<{ authUrl: string }>('api/v1/auth/google/url')

      if (status.value === 'success')
        window.location.href = data.value!.authUrl
    }
  }
]
</script>
