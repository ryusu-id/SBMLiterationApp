<script lang="ts" setup>
import type { ButtonProps } from '@nuxt/ui'
import { $authedFetch, handleResponseError, useAuth, type ApiResponse } from '~/apis/api'
import ProfileForm from '~/components/profile/ProfileForm.vue'
import type { ProfileFormSchema } from '~/components/profile/ProfileForm.vue'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

export interface UserProfile {
  id: number
  fullname: string
  nim: string
  programStudy: string
  faculty: string
  generationYear: string
  pictureUrl?: string
}

const profile = ref<UserProfile | null>(null)
const pending = ref(false)
const form = useTemplateRef<typeof ProfileForm>('form')
const formLoading = ref(false)
const toast = useToast()

async function fetchProfile() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<UserProfile>>('/auth/site')
    if (response.data) {
      profile.value = response.data
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }
}

function openEditForm() {
  if (profile.value) {
    form.value?.setState({
      fullname: profile.value.fullname,
      nim: profile.value.nim,
      programStudy: profile.value.programStudy,
      faculty: profile.value.faculty,
      generationYear: profile.value.generationYear
    })
  }
  form.value?.open()
}

async function onSubmit(data: ProfileFormSchema) {
  try {
    formLoading.value = true
    const response = await $authedFetch<ApiResponse>('/auth/site', {
      method: 'PUT',
      body: data
    })

    if (response.errorDescription || response.errorCode) {
      handleResponseError(response)
      return
    }

    toast.add({
      title: 'Profile updated successfully',
      color: 'success'
    })

    form.value?.close()
    await fetchProfile()
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}

onMounted(() => {
  fetchProfile()
})

const dialog = useDialog()
const auth = useAuth()
const router = useRouter()
const color = useColorMode()

const links = ref<ButtonProps[]>([
  {
    variant: 'soft',
    color: 'error',
    icon: 'i-heroicons-arrow-right-on-rectangle',
    onClick: async () => {
      dialog.confirm({
        title: 'Logout',
        subTitle: 'You will need to re-login with Google',
        message: 'Are you sure you want to logout?',
        onOk: () => {
          auth.clearToken()
          router.push('/')
        }
      })
    }
  }])

function toggleColorMode() {
  color.preference = color.value === 'dark' ? 'light' : 'dark'
}
</script>

<template>
  <div
    class="max-w-[1200px] mx-auto flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UContainer>
      <div class="flex flex-col space-y-6">
        <UPageHeader
          class="flex-1"
          :ui="{
            wrapper: 'flex flex-row justify-between'
          }"
          title="Profile"
          description="Manage your personal information"
        >
          <template #links>
            <ClientOnly>
              <UButton
                variant="soft"
                color="neutral"
                :icon="color.value === 'dark' ? 'i-heroicons-moon' : 'i-heroicons-sun'"
                @click="toggleColorMode"
              />
            </ClientOnly>
            <UButton
              v-for="(link, index) in links"
              :key="index"
              v-bind="link"
            />
            <UAvatar
              :src="profile?.pictureUrl"
              icon="i-lucide-user-circle"
              size="2xl"
            />
          </template>
        </UPageHeader>

        <div
          v-if="pending"
          class="flex items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <UCard
          v-else-if="profile"
          :ui="{
            body: 'space-y-6'
          }"
        >
          <div class="flex items-center justify-between">
            <h2 class="text-xl font-semibold">
              Personal Information
            </h2>
            <UButton
              icon="i-heroicons-pencil"
              color="primary"
              variant="soft"
              @click="openEditForm"
            >
              Edit
            </UButton>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="space-y-2">
              <p class="text-sm font-medium">
                Full Name
              </p>
              <p class="font-semibold">
                {{ profile.fullname }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium">
                NIM (Student ID)
              </p>
              <p class="font-semibold">
                {{ profile.nim }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium">
                Program Study
              </p>
              <p class="font-semibold">
                {{ profile.programStudy }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium">
                Faculty
              </p>
              <p class="font-semibold">
                {{ profile.faculty }}
              </p>
            </div>

            <div class="space-y-2">
              <p class="text-sm font-medium">
                Generation Year
              </p>
              <p class="font-semibold">
                {{ profile.generationYear }}
              </p>
            </div>
          </div>
        </UCard>

        <ProfileForm
          ref="form"
          :loading="formLoading"
          @submit="onSubmit"
        />
      </div>
    </UContainer>
  </div>
</template>
