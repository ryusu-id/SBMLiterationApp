<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'
import type { Swiper as SwiperType } from 'swiper'

// Swiper styles (WAJIB)
import 'swiper/css'
import 'swiper/css/effect-cards'
import 'swiper/css/navigation'

// Swiper module
import { EffectCards, Mousewheel, Navigation } from 'swiper/modules'
import { handleResponseError } from '~/apis/api'
import type { DailyRead } from './DailyReadsTable.vue'
import DailyReadCard from './DailyReadCard.vue'
import type { PagingResult } from '~/apis/paging'

const swiperInstance = ref<SwiperType>()

const onSwiper = (swiper: SwiperType) => {
  swiperInstance.value = swiper
}

const useAuthedFetch = useNuxtApp().$useAuthedFetch

const pending = ref(true)

const { data, error } = await useAuthedFetch<PagingResult<DailyRead>>('/daily-reads', {
  query: {
    page: 1,
    pageSize: 100
  },
  onResponse() {
    pending.value = false
  },
  onResponseError() {
    pending.value = false
  }
})

if (error.value) {
  handleResponseError(error.value)
}

const rows = computed(() => data.value?.rows || [])

onMounted(async () => {
  if (rows.value.length > 0 && swiperInstance.value?.activeIndex === 0)
    setTimeout(() => swiperInstance.value?.slideNext(), 10)
})
</script>

<template>
  <div class="relative">
    <div
      v-if="rows.length === 0"
      class="flex flex-col items-center justify-center py-12 aspect-[2/3] max-w-[300px] sm:max-w-[330px] mx-auto border-2 border-dashed border-gray-300 rounded-[36px]"
    >
      <UIcon
        name="i-heroicons-calendar"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500 text-center px-4">
        No daily reading available yet!<br>
        Check back soon.
      </p>
    </div>

    <Swiper
      v-else
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
      v-if="rows.length > 0"
      class="prev-btn-daily absolute left-[-5px] sm:left-[-15px] top-1/2 -translate-y-1/2 z-30 flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg border border-gray-100 text-[#3566CD] transition-all duration-300 hover:bg-[#3566CD] hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none cursor-pointer"
    >
      <UIcon
        name="i-heroicons-chevron-left"
        class="size-6"
      />
    </button>

    <button
      v-if="rows.length > 0"
      class="next-btn-daily absolute right-[-5px] sm:right-[-15px] top-1/2 -translate-y-1/2 z-30 flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg border border-gray-100 text-[#3566CD] transition-all duration-300 hover:bg-[#3566CD] hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none cursor-pointer"
    >
      <UIcon
        name="i-heroicons-chevron-right"
        class="size-6"
      />
    </button>
  </div>
</template>

<style>
.swiper-slide-shadow-cards {
  border-radius: 36px !important;
}
</style>
