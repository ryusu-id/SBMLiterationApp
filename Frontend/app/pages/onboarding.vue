<script setup lang="ts">
import { $authedFetch, handleResponseError, useAuth, type ApiResponse } from '~/apis/api'
import type { ProfileFormSchema } from '~/components/profile/ProfileForm.vue'

definePageMeta({
  layout: 'landing',
  middleware: ['auth']
})

const { $useAuthedFetch } = useNuxtApp()
const router = useRouter()
const toast = useToast()
const auth = useAuth()
const loading = ref(false)

// Redirect admins directly to admin page (onboarding is only for participants)
if (auth.getRoles().includes('admin')) {
  router.push('/admin')
}

const { data: userInfo, error, refresh, pending } = await $useAuthedFetch<ApiResponse<{
  nim: string
  fullname: string
  programStudy: string
  faculty: string
  generationYear: string
  pictureUrl: string
}>>('/auth/site')

const initial = ref(true)

watch(error, (err) => {
  if (err) handleResponseError(err)
})

// Check if all fields are filled and redirect to dashboard
watch(() => userInfo.value, (newVal) => {
  if (newVal.data) {
    const { nim, fullname, programStudy, faculty, generationYear } = newVal.data
    if (nim && fullname && programStudy && faculty && generationYear) {
      // All fields filled, redirect to dashboard
      router.push('/dashboard')
      return
    }
    initial.value = false
  }
}, {
  immediate: true
})

async function onSubmit(data: ProfileFormSchema) {
  try {
    loading.value = true
    await $authedFetch('/auth/site', {
      method: 'PUT',
      body: data
    })

    toast.add({
      title: 'Profile updated successfully',
      color: 'success'
    })

    // Refresh user info to trigger redirect
    await refresh()
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex justify-center p-4 pt-9">
    <div
      v-if="initial"
      class="max-w-md w-full text-center"
    >
      <!-- Loading State -->
      <div v-if="pending">
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
    <UContainer v-else>
      <div class="max-w-2xl mx-auto">
        <UCard>
          <template #header>
            <div class="text-center space-y-2">
              <div class="flex justify-center mb-4">
                <UAvatar
                  v-if="userInfo?.data?.pictureUrl"
                  :src="userInfo.data.pictureUrl"
                  :alt="userInfo.data.fullname"
                  size="2xl"
                />
              </div>
              <h1 class="text-3xl font-bold tracking-tight">
                Welcome to SIGMA!
              </h1>
              <p class="text-gray-600 dark:text-gray-400">
                Let's complete your profile to get started
              </p>
            </div>
          </template>

          <ProfileForm
            v-if="userInfo"
            :initial-data="userInfo.data"
            :loading="loading"
            embedded
            @submit="onSubmit"
          />
        </UCard>
      </div>
    </UContainer>
  </div>
</template>
