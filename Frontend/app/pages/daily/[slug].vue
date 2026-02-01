<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import type { DailyRead } from '~/components/daily-reads/DailyReadsTable.vue'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

const route = useRoute()
const slug = computed(() => route.params.slug as string)

const dailyRead = ref<DailyRead | null>(null)
const pending = ref(false)

onMounted(async () => {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<DailyRead>>(
      `/daily-reads/${slug.value}`
    )

    if (response.data) {
      dailyRead.value = response.data
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }
})

function startQuiz() {
  useRouter().push({
    name: 'daily-quiz-slug',
    params: { slug: slug.value }
  })
}
</script>

<template>
  <UContainer class="py-8">
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
      v-else-if="dailyRead"
      class="space-y-6"
    >
      <!-- Header -->
      <div class="space-y-4">
        <UButton
          icon="i-heroicons-arrow-left"
          color="neutral"
          variant="ghost"
          to="/dashboard"
        >
          Back to Dashboard
        </UButton>

        <div
          v-if="dailyRead.coverImg"
          class="w-full rounded-xl overflow-hidden"
        >
          <img
            :src="dailyRead.coverImg"
            :alt="dailyRead.title"
            class="object-cover"
          >
        </div>

        <div class="space-y-2">
          <h1 class="text-4xl font-bold">
            {{ dailyRead.title }}
          </h1>

          <div class="flex items-center gap-4 text-sm text-gray-600">
            <div class="flex items-center gap-2">
              <UIcon
                name="i-lucide-calendar"
                class="size-4"
              />
              <span>{{ new Date(dailyRead.date).toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</span>
            </div>

            <div
              v-if="dailyRead.category"
              class="flex items-center gap-2"
            >
              <UBadge
                color="primary"
                variant="subtle"
              >
                {{ dailyRead.category }}
              </UBadge>
            </div>

            <div class="flex items-center gap-1">
              <UIcon
                name="i-lucide-star"
                class="size-4"
              />
              <span>{{ dailyRead.exp }} EXP</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Content -->
      <UCard>
        <!-- eslint-disable-next-line vue/no-v-html -->
        <UEditor
          class="prose prose-lg dark:prose-invert max-w-none"
          :model-value="dailyRead.content"
          content-type="markdown"
          readonly
          :editable="false"
        />
      </UCard>

      <!-- Quiz Button -->
      <div class="flex justify-center pt-6">
        <UButton
          size="xl"
          icon="i-lucide-brain"
          @click="startQuiz"
        >
          Take the Quiz
        </UButton>
      </div>
    </div>

    <div
      v-else
      class="flex flex-col items-center justify-center py-12"
    >
      <UIcon
        name="i-heroicons-exclamation-circle"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500 text-center">
        Daily reading not found
      </p>
    </div>
  </UContainer>
</template>
