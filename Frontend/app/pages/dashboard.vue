<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'
import { handleResponseError, useAuth } from '~/apis/api'
import type { PagingResult } from '~/apis/paging'
import ReadingRecomendationList from '~/components/home/Recomendation/ReadingRecomendationList.vue'
import ReadingReportList from '~/components/reading-passport/ReadingReportList.vue'
import type { ReadingReportData } from '~/components/reading-passport/ReadingReportList.vue'
import ReadingResources from '~/components/reading-passport/ReadingResources.vue'
import Streak from '~/components/reading-passport/Streak.vue'
import DailyReads from '~/components/daily-reads/DailyReads.vue'

definePageMeta({
  keepalive: true,
  middleware: ['auth', 'participant-only']
})

const readingResource = useTemplateRef<typeof ReadingResources>('readingResource')
const recommendation = useTemplateRef<typeof ReadingRecomendationList>('recommendation')
const auth = useAuth()
const useAuthedFetch = useNuxtApp().$useAuthedFetch

const { data: readingReports, pending: reportPending, error } = await useAuthedFetch<PagingResult<ReadingReportData>>('/reading-resources/reports/latest-activity', {
  query: {
    page: 1,
    pageSize: 5
  }
})

interface MyAssignmentItem {
  id: number
  title: string
  description?: string
  dueDate?: string
  submissionId: number
  isCompleted: boolean
  completedAt?: string
  fileCount: number
}

const { data: assignmentsData, error: assignmentsError } = await useAuthedFetch<{ data: MyAssignmentItem[] }>('/assignments/my')

watch(error, (err) => {
  if (err) handleResponseError(err)
})

watch(assignmentsError, (err) => {
  if (err) handleResponseError(err)
})

const activeAssignments = computed(() => {
  return (assignmentsData.value?.data || []).filter(a => !a.isCompleted)
})

function getRemainingTime(dueDate?: string): string {
  if (!dueDate) return 'No due date'
  const now = new Date()
  const due = new Date(dueDate)
  const diff = due.getTime() - now.getTime()
  if (diff <= 0) return 'Overdue'
  const days = Math.floor(diff / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))
  if (days > 0) return `${days}d ${hours}h remaining`
  if (hours > 0) return `${hours}h ${minutes}m remaining`
  return `${minutes}m remaining`
}

function getRemainingTimeColor(dueDate?: string): 'error' | 'warning' | 'success' | 'neutral' {
  if (!dueDate) return 'neutral'
  const now = new Date()
  const due = new Date(dueDate)
  const diff = due.getTime() - now.getTime()
  if (diff <= 0) return 'error'
  const hours = diff / (1000 * 60 * 60)
  if (hours <= 24) return 'error'
  if (hours <= 72) return 'warning'
  return 'success'
}

function fetchReadingResources() {
  if (!readingResource.value?.fetch) return
  readingResource.value?.fetch()
}

function fetchRecommendation() {
  recommendation.value?.fetch()
}

onMounted(() => {
  const quizComposable = usePersistedQuiz()
  const reportState = usePersistedReadingReport()

  quizComposable.init()
  reportState.init()
})

const tabs = ref<TabsItem[]>([
  {
    label: 'Books',
    icon: 'i-lucide-book',
    slot: 'books',
    value: 0
  },
  {
    label: 'Papers',
    icon: 'i-lucide-form',
    slot: 'journal-paper',
    value: 1
  },
  {
    label: 'Daily',
    icon: 'i-lucide-calendar',
    slot: 'daily-reading',
    value: 2
  }
])

const tab = ref(0)
</script>

<template>
  <UContainer>
    <div class="flex flex-col space-y-[32px]">
      <div class="flex flex-col md:flex-row mb-0">
        <UPageHeader
          class="border-none"
          :ui="{
            root: 'w-full'
          }"
        >
          <template #title>
            <div class="flex flex-col md:flex-row md:space-x-2">
              <h1 class="text-[36px] font-extrabold tracking-tighter">
                Welcome,

                {{ auth.getFullname() }} 😁
              </h1>
            </div>
          </template>

          <template #description>
            <p class="tracking-tight text-[20px]">
              Start your reading journey now
            </p>
          </template>
        </UPageHeader>

        <Streak />
      </div>

      <div class="grid grid-cols-12">
        <div class="col-span-12 md:col-span-6">
          <UTabs
            v-model="tab"
            :items="tabs"
            :ui="{
              list: 'mb-2',
              trigger: 'cursor-pointer'
            }"
            class="max-w-full mt-3 mb-2 mx-auto md:mx-0 md:mr-auto"
          />
          <div class="min-h-[495px] flex items-start justify-center">
            <ReadingResources
              v-if="tab !== 2 && tab !== 3"
              ref="readingResource"
              :journal="tab === 1"
              @refresh="fetchRecommendation"
            />
            <DailyReads
              v-else-if="tab === 2"
            />
          </div>
        </div>
        <div class="hidden md:flex sm:col-span-6 flex-row justify-center items-center">
          <nuxt-img
            src="/book-dash.png"
            class="max-h-[450px]"
          />
        </div>
      </div>

      <div
        v-if="activeAssignments.length > 0"
        class="space-y-3"
      >
        <div class="flex items-center justify-between">
          <h2 class="text-xl font-bold tracking-tight">
            Active Assignments
          </h2>
          <UButton
            icon="i-lucide-arrow-right"
            color="neutral"
            variant="ghost"
            size="sm"
            label="View all"
            to="/assignments"
          />
        </div>
        <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <NuxtLink
            v-for="assignment in activeAssignments"
            :key="assignment.id"
            :to="`/assignments/${assignment.id}`"
          >
            <UCard class="hover:ring-1 hover:ring-primary transition-shadow cursor-pointer h-full">
              <div class="flex flex-col gap-2">
                <h3 class="font-semibold truncate">
                  {{ assignment.title }}
                </h3>
                <div class="flex items-center gap-2 flex-wrap">
                  <UBadge
                    :color="getRemainingTimeColor(assignment.dueDate)"
                    variant="subtle"
                    size="sm"
                    icon="i-lucide-clock"
                  >
                    {{ getRemainingTime(assignment.dueDate) }}
                  </UBadge>
                  <UBadge
                    color="neutral"
                    variant="subtle"
                    size="sm"
                    icon="i-lucide-paperclip"
                  >
                    {{ assignment.fileCount }} file{{ assignment.fileCount !== 1 ? 's' : '' }}
                  </UBadge>
                </div>
              </div>
            </UCard>
          </NuxtLink>
        </div>
      </div>

      <ReadingRecomendationList
        ref="recommendation"
        @refresh="fetchReadingResources"
      />

      <div
        v-if="reportPending"
        class="flex items-center justify-center py-12"
      >
        <UIcon
          name="i-heroicons-arrow-path"
          class="animate-spin text-4xl"
        />
      </div>

      <ReadingReportList
        v-else
        dashboard
        :reports="readingReports?.rows || []"
      />
    </div>
  </UContainer>
</template>
