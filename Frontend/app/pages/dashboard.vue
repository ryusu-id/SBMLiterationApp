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

watch(error, (err) => {
  if (err) handleResponseError(err)
})

function fetchReadingResources() {
  readingResource.value?.fetch()
}

function fetchRecommendation() {
  recommendation.value?.fetch()
}

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

                {{ auth.getFullname() }}
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
              v-if="tab !== 2"
              ref="readingResource"
              :journal="tab === 1"
              @refresh="fetchRecommendation"
            />
            <DailyReads
              v-else
            />
          </div>
        </div>
        <div class="hidden md:flex sm:col-span-6 flex-row justify-center items-center">
          <nuxt-img
            src="/book-hero.png"
            class="max-h-[450px]"
          />
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
