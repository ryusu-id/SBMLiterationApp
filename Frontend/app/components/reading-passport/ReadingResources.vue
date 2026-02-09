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
import ReadingResourceCard from './ReadingResourceCard.vue'

const props = defineProps<{
  journal?: boolean
}>()

export interface ReadingResource {
  id: number
  title: string
  isbn: string
  readingCategory: string
  authors: string
  publishYear: string
  page: number
  resourceLink: string
  coverImageUri: string
  cssClass: string
}

const swiperInstance = ref<SwiperType>()

const onSwiper = (swiper: SwiperType) => {
  swiperInstance.value = swiper
}

const type = computed(() => props.journal ? 'journals' : 'books')
const apiUri = computed(() => `/reading-resources/${type.value}`)
const useAuthedFetch = useNuxtApp().$useAuthedFetch

const refreshCallback = ref<() => void>()

defineExpose({
  fetch: computed(() => refreshCallback.value)
})

const { data, error, refresh } = await useAuthedFetch<{
  rows: ReadingResource[]
}>(() => apiUri.value, {
  query: {
    page: 1,
    pageSize: 100
  }
})

refreshCallback.value = refresh

if (error.value) {
  handleResponseError(error.value)
}

const rows = computed(() => data.value?.rows || [])

onMounted(async () => {
  if (rows.value.length > 0 && swiperInstance.value?.activeIndex === 0)
    setTimeout(() => swiperInstance.value?.slideNext(), 10)
})

watch(() => props.journal, async () => {
  await refresh()

  if (rows.value.length > 0 && swiperInstance.value?.activeIndex === 0)
    setTimeout(() => swiperInstance.value?.slideNext(), 10)
})

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

function onRefresh() {
  refresh()
  emit('refresh')
}

function onCreate() {
  useRouter().push(props.journal
    ? { name: 'CreateReadingJournal' }
    : { name: 'CreateReadingBook' })
}
</script>

<template>
  <div class="relative w-full">
    <Swiper
      :modules="[EffectCards, Mousewheel, Navigation]"
      effect="cards"
      grab-cursor
      mousewheel
      class="w-full max-w-[300px] sm:max-w-[330px]"
      :navigation="{
        prevEl: '.prev-btn',
        nextEl: '.next-btn'
      }"
      @swiper="onSwiper"
    >
      <SwiperSlide class="aspect-[2/3] rounded-[36px] overflow-hidden">
        <div
          class="w-full h-full border-5 border-primary rounded-[36px] bg-white flex flex-col items-center justify-center gap-4 text-primary
              cursor-pointer transition-all duration-300 group
              hover:bg-primary hover:text-white hover:shadow-xl active:scale-95"
          @click="onCreate"
        >
          <UIcon
            name="i-heroicons-plus"
            class="size-16 transition-transform duration-300 group-hover:rotate-90 group-hover:scale-110 "
          />

          <h1 class="text-center font-semibold text-xl leading-tight">
            New<br>
            Reading Source
          </h1>
        </div>
      </SwiperSlide>
      <SwiperSlide
        v-for="res in rows"
        :key="res.isbn"
        class="rounded-[36px] overflow-hidden"
      >
        <ReadingResourceCard
          :journal
          :resource="res"
          @refresh="onRefresh"
        />
      </SwiperSlide>
    </Swiper>
    <button class="prev-btn absolute left-[-5px] sm:left-[-15px] top-1/2 -translate-y-1/2 z-30 flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg  border border-gray-100 text-primary transition-all duration-300 hover:bg-primary hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none cursor-pointer">
      <UIcon
        name="i-heroicons-chevron-left"
        class="size-6"
      />
    </button>

    <button class="next-btn absolute right-[-5px] sm:right-[-15px] top-1/2 -translate-y-1/2 z-30  flex items-center justify-center w-12 h-12 rounded-full bg-white shadow-lg border border-gray-100 text-primary transition-all duration-300 hover:bg-primary hover:text-white active:scale-95 disabled:opacity-0 disabled:pointer-events-none  cursor-pointer">
      <UIcon
        name="i-heroicons-chevron-right"
        class="size-6"
      />
    </button>
  </div>
</template>

<style>
.swiper-slide-shadow-cards {
  /* background: transparent !important;
  opacity: 0 !important; */
  border-radius: 36px !important;
}
</style>
