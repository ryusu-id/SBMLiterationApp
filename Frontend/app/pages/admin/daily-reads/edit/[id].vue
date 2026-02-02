<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import DailyReadForm from '~/components/daily-reads/DailyReadForm.vue'
import DailyReadQuizForm from '~/components/daily-reads/DailyReadQuizForm.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

interface DailyRead {
  id: string
  title: string
  coverImg?: string
  content: string
  date: string
  category?: string
  exp: number
  minimalCorrectAnswer: number
}

const route = useRoute()
const router = useRouter()
const quizForm = useTemplateRef<typeof DailyReadQuizForm>('quizForm')
const form = useTemplateRef<typeof DailyReadForm>('form')
const formLoading = ref(false)
const pending = ref(false)
const toast = useToast()

const dailyReadId = computed(() => route.params.id as string)
const dailyReadData = ref<DailyRead | null>(null)

async function fetchDailyRead() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<DailyRead>>(
      `/daily-reads/${dailyReadId.value}`
    )

    if (response.data) {
      dailyReadData.value = response.data
    } else {
      handleResponseError(response)
      router.push('/admin/daily-reads')
    }
  } catch (err) {
    handleResponseError(err)
    router.push('/admin/daily-reads')
  } finally {
    pending.value = false
  }
}

// Watch for when both data is loaded and form is available
watch([dailyReadData, form], ([data, formRef]) => {
  if (data && formRef) {
    formRef.update({
      id: Number(data.id),
      title: data.title,
      coverImg: data.coverImg || '',
      content: data.content,
      date: data.date,
      category: data.category || '',
      exp: data.exp,
      minimalCorrectAnswer: data.minimalCorrectAnswer
    })
  }
})

onMounted(async () => {
  await fetchDailyRead()
})

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

    // First, update the daily read
    await $authedFetch(`/daily-reads/${dailyReadId.value}`, {
      method: 'PUT',
      body: param.data
    })

    // Then, upload quiz if file is selected
    const quizFile = form.value?.getQuizFile()
    if (quizFile) {
      const formData = new FormData()
      formData.append('file', quizFile)

      await $authedFetch(`/daily-reads/${dailyReadId.value}/quiz/upload`, {
        method: 'POST',
        body: formData
      })

      // Refresh the quiz display
      quizForm.value?.refresh()

      // Clear the quiz file from the form
      form.value?.clearQuizFile()
    }

    toast.add({
      title: 'Daily read updated successfully',
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
      <DashboardNavbar />
    </template>
    <template #body>
      <div
        v-if="pending"
        class="flex items-center justify-center py-12"
      >
        <UIcon
          name="i-heroicons-arrow-path"
          class="animate-spin text-4xl"
        />
      </div>

      <div
        v-else
        class="max-w-7xl mx-auto"
      >
        <div class="flex items-center gap-2 mb-6">
          <UButton
            icon="i-lucide-arrow-left"
            color="neutral"
            variant="ghost"
            to="/admin/daily-reads"
          />
          <h1 class="text-2xl font-bold">
            Edit Daily Read
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
                :daily-read-id="dailyReadData?.id ? Number(dailyReadData.id) : null"
              />
            </UCard>
          </div>
        </div>
      </div>
    </template>
  </UDashboardPanel>
</template>
