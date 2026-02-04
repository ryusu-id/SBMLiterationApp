<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'
import type { Swiper as SwiperType } from 'swiper'

// Swiper styles (WAJIB)
import 'swiper/css'
import 'swiper/css/effect-cards'
import 'swiper/css/navigation'

// Swiper module
import { EffectCards, Mousewheel, Navigation } from 'swiper/modules'
import { $authedFetch, handleResponseError } from '~/apis/api'
import type { DailyRead } from './DailyReadsTable.vue'
import DailyReadCard from './DailyReadCard.vue'
import type { PagingResult } from '~/apis/paging'

const swiperInstance = ref<SwiperType>()

const onSwiper = (swiper: SwiperType) => {
  swiperInstance.value = swiper
}

// Date management
const selectedDate = ref(new Date())
const pending = ref(false)
const rows = ref<DailyRead[]>([])

const formatDateForAPI = (date: Date) => {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

const formatDateDisplay = (date: Date) => {
  return date.toLocaleDateString('en-US', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const isToday = computed(() => {
  const today = new Date()
  return selectedDate.value.toDateString() === today.toDateString()
})

const canGoNext = computed(() => {
  const today = new Date()
  const nextDay = new Date(selectedDate.value)
  nextDay.setDate(nextDay.getDate() + 1)
  return nextDay <= today
})

const fetchDailyReads = async () => {
  try {
    pending.value = true
    const response = await $authedFetch<PagingResult<DailyRead>>('/daily-reads/participant', {
      query: {
        page: 1,
        pageSize: 100,
        dateTo: isToday.value ? undefined : formatDateForAPI(selectedDate.value)
      }
    })

    if (response.rows) {
      rows.value = response.rows
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }
}

const goToPreviousDay = () => {
  const prevDay = new Date(selectedDate.value)
  prevDay.setDate(prevDay.getDate() - 1)
  selectedDate.value = prevDay
  fetchDailyReads()
}

const goToNextDay = () => {
  if (canGoNext.value) {
    const nextDay = new Date(selectedDate.value)
    nextDay.setDate(nextDay.getDate() + 1)
    selectedDate.value = nextDay
    fetchDailyReads()
  }
}

onMounted(async () => {
  await fetchDailyReads()

  if (rows.value.length > 0 && swiperInstance.value?.activeIndex === 0)
    setTimeout(() => swiperInstance.value?.slideNext(), 10)
})
</script>

<template>
  <div class="relative space-y-6">
    <!-- Date Navigation -->
    <div class="flex flex-col items-center gap-4">
      <div class="flex items-center gap-3">
        <UButton
          icon="i-heroicons-chevron-left"
          color="neutral"
          variant="outline"
          size="sm"
          :disabled="pending"
          @click="goToPreviousDay"
        />

        <div class="text-center min-w-[280px]">
          <p class="text-sm text-gray-500 font-medium">
            Daily Reading for
          </p>
          <p class="text-lg font-semibold">
            {{ formatDateDisplay(selectedDate) }}
          </p>
        </div>

        <UButton
          icon="i-heroicons-chevron-right"
          color="neutral"
          variant="outline"
          size="sm"
          :disabled="pending || !canGoNext"
          @click="goToNextDay"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div
      v-if="pending"
      class="flex flex-col items-center justify-center py-12 aspect-[2/3] max-w-[300px] sm:max-w-[330px] mx-auto"
    >
      <UIcon
        name="i-heroicons-arrow-path"
        class="animate-spin text-4xl text-primary mb-4"
      />
      <p class="text-gray-500 text-center">
        Loading...
      </p>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="rows.length === 0"
      class="flex flex-col items-center justify-center py-12 aspect-[2/3] max-w-[300px] sm:max-w-[330px] mx-auto border-2 border-dashed border-gray-300 rounded-[36px]"
    >
      <UIcon
        name="i-heroicons-calendar"
        class="size-16 mb-4"
      />
      <p class="text-center px-4">
        No daily reading available for this date!<br>
        Try another day.
      </p>
    </div>

    <!-- Swiper Cards -->
    <div
      v-else
      class="relative w-full"
    >
      <Swiper
        :modules="[EffectCards, Mousewheel, Navigation]"
        effect="cards"
        grab-cursor
        mousewheel
        class="w-full max-w-[300px] sm:max-w-[330px]"
        :navigation="{
          prevEl: '.prev-btn-daily',
          nextEl: '.next-btn-daily'
        }"
        @swiper="onSwiper"
      >
        <SwiperSlide
          v-for="dailyRead in rows"
          :key="dailyRead.id"
          class="aspect-[2/3] rounded-[36px] overflow-hidden"
        >
          <DailyReadCard :daily-read="dailyRead" />
        </SwiperSlide>
      </Swiper>

      <button
        class="prev-btn-daily absolute left-[-5px] sm:left-[-15px] top-1/2 -translate-y-1/2 z-30 flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg border border-gray-100 text-[#3566CD] transition-all duration-300 hover:bg-[#3566CD] hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none cursor-pointer"
      >
        <UIcon
          name="i-heroicons-chevron-left"
          class="size-6"
        />
      </button>

      <button
        class="next-btn-daily absolute right-[-5px] sm:right-[-15px] top-1/2 -translate-y-1/2 z-30 flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg border border-gray-100 text-[#3566CD] transition-all duration-300 hover:bg-[#3566CD] hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none cursor-pointer"
      >
        <UIcon
          name="i-heroicons-chevron-right"
          class="size-6"
        />
      </button>
    </div>
  </div>
</template>

<style>
.swiper-slide-shadow-cards {
  border-radius: 36px !important;
}
</style>
