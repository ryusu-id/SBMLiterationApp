<script lang="ts" setup>
import type { ButtonProps } from '@nuxt/ui'
import { $authedFetch, handleResponseError, useAuth, type ApiResponse } from '~/apis/api'
import ProfileForm from '~/components/profile/ProfileForm.vue'
import type { ProfileFormSchema } from '~/components/profile/ProfileForm.vue'
import type { PersistedQuizState } from '~/composables/quiz'

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
const quizComposable = usePersistedQuiz()
const unfinishedQuizzes = ref<PersistedQuizState[]>([])

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

function loadUnfinishedQuizzes() {
  // Clean up stale quizzes first
  quizComposable.cleanupStaleQuizzes()
  unfinishedQuizzes.value = quizComposable.getUnfinishedQuizzes()
}

function continueQuiz(slug: string) {
  router.push(`/daily/quiz/${slug}`)
}

function formatDate(dateString: string) {
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
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
  loadUnfinishedQuizzes()
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

        <!-- Unfinished Quizzes Section -->
        <UCard
          v-if="unfinishedQuizzes.length > 0"
          :ui="{
            body: 'space-y-6'
          }"
        >
          <div class="flex items-center justify-between">
            <div>
              <h2 class="text-xl font-semibold">
                Unfinished Quizzes
              </h2>
              <p class="text-sm text-gray-600 mt-1">
                Continue where you left off
              </p>
            </div>
            <UBadge
              color="primary"
              variant="subtle"
              size="lg"
            >
              {{ unfinishedQuizzes.length }}
            </UBadge>
          </div>

          <div class="space-y-3">
            <div
              v-for="quiz in unfinishedQuizzes"
              :key="quiz.slug"
              class="flex items-center justify-between p-4 rounded-lg border-2 border-gray-200 dark:border-gray-700 hover:border-primary-300 dark:hover:border-primary-700 transition-colors"
            >
              <div class="flex-1">
                <div class="flex items-center gap-2 mb-1">
                  <UIcon
                    name="i-heroicons-academic-cap"
                    class="size-5 text-primary-600"
                  />
                  <h3 class="font-medium">
                    {{ quiz.title || `Quiz: ${quiz.slug}` }}
                  </h3>
                </div>
                <div class="flex items-center gap-4 text-sm text-gray-600">
                  <span>{{ quiz.answeredQuestions }} / {{ quiz.totalQuestions }} answered</span>
                  <span>•</span>
                  <span>{{ formatDate(quiz.lastUpdated) }}</span>
                </div>
                <div class="mt-2">
                  <div class="w-full bg-gray-200 dark:bg-gray-700 rounded-full h-2">
                    <div
                      class="bg-primary-600 h-2 rounded-full transition-all"
                      :style="{ width: `${(quiz.answeredQuestions / quiz.totalQuestions) * 100}%` }"
                    />
                  </div>
                </div>
              </div>
              <div class="ml-4">
                <UButton
                  color="primary"
                  variant="soft"
                  @click="continueQuiz(quiz.slug)"
                >
                  Continue
                  <template #trailing>
                    <UIcon name="i-heroicons-arrow-right" />
                  </template>
                </UButton>
              </div>
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
