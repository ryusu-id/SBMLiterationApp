<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import DailyReadForm from '~/components/daily-reads/DailyReadForm.vue'
import DailyReadQuizForm from '~/components/daily-reads/DailyReadQuizForm.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

const form = useTemplateRef<typeof DailyReadForm>('form')
const quizForm = useTemplateRef<typeof DailyReadQuizForm>('quizForm')
const formLoading = ref(false)
const toast = useToast()
const router = useRouter()
const dailyReadId = ref<number | null>(null)

async function onSubmit(param: {
  action: 'Create' | 'Update'
  data: {
    title: string
    coverImg: string
    content: string
    date: string
    category: string
    exp: number
    minimalCorrectAnswer: number
  }
  id: number | null
}) {
  try {
    formLoading.value = true

    // First, create the daily read
    const response = await $authedFetch<{ data: { id: number } }>('/daily-reads', {
      method: 'POST',
      body: {
        ...param.data
      }
    })

    dailyReadId.value = response.data?.id

    // Then, upload quiz if file is selected
    const quizFile = quizForm.value?.getQuizFile()
    if (quizFile && dailyReadId.value) {
      const formData = new FormData()
      formData.append('file', quizFile)

      await $authedFetch(`/daily-reads/${dailyReadId.value}/quiz/upload`, {
        method: 'POST',
        body: formData
      })

      quizForm.value?.refresh()
    }

    toast.add({
      title: 'Daily read created successfully',
      color: 'success'
    })

    router.push('/admin/daily-reads')
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Create Daily Read" />
    </template>

    <template #body>
      <div class="max-w-7xl mx-auto">
        <div class="flex items-center gap-2 mb-6">
          <UButton
            icon="i-lucide-arrow-left"
            color="neutral"
            variant="ghost"
            to="/admin/daily-reads"
          />
          <h1 class="text-2xl font-bold">
            Create New Daily Read
          </h1>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-[70%_30%] gap-6 pr-6">
          <div>
            <DailyReadForm
              ref="form"
              :loading="formLoading"
              @submit="onSubmit"
            />
          </div>

          <div class="lg:sticky lg:top-6 lg:self-start">
            <UCard>
              <DailyReadQuizForm
                ref="quizForm"
                :daily-read-id="dailyReadId"
              />
            </UCard>
          </div>
        </div>
      </div>
    </template>
  </UDashboardPanel>
</template>
