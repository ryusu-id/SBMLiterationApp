<script setup lang="ts">
import ReadingReport from './ReadingReport.vue'

export interface ReadingReportData {
  status: number
  createTime: string
  updateTime: string
  createBy: string
  updateBy: string
  id: number
  userId: number
  readingResourceId: number
  reportDate: string
  currentPage: number
  insight: string
  timeSpent: number
  coverImageUri?: string | undefined
}

const props = defineProps<{
  reports: ReadingReportData[]
  resourceTitle?: string
  totalPage?: number
  dashboard?: boolean
}>()

// Map backend reports to display format
const mappedReports = computed(() => {
  return props.reports.map(report => ({
    title: props.resourceTitle || 'Reading Resource',
    imageUrl: report.coverImageUri || '',
    insight: report.insight,
    readDate: new Date(report.reportDate).toISOString().split('T')[0],
    currentPage: report.currentPage,
    totalPage: props.totalPage || 0,
    timeSpent: report.timeSpent
  }))
})
</script>

<template>
  <div>
    <h1 class="font-semibold text-[20px] tracking-tight">
      Your Reading Journey 🔥
    </h1>
  </div>

  <div
    v-if="mappedReports.length === 0"
    class="flex flex-col items-center justify-center py-12 px-4"
  >
    <UIcon
      name="i-heroicons-book-open"
      class="size-16 text-gray-300 mb-4"
    />
    <p class="text-gray-500 text-center text-lg font-medium">
      You haven't read yet! Let's report your progress! 📚
    </p>
  </div>

  <div
    v-else
    class="grid grid-cols-12 gap-4"
  >
    <!-- use With Image to show cover -->
    <ReadingReport
      v-for="(report, index) in mappedReports"
      :key="`${report.readDate}-${index}`"
      :with-image="dashboard"
      :report="report"
    />
  </div>
</template>
