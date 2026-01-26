<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'
import type { Swiper as SwiperType } from 'swiper'

// Swiper styles (WAJIB)
import 'swiper/css'
import 'swiper/css/effect-cards'

// Swiper module
import { EffectCards, Mousewheel } from 'swiper/modules'
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
  fetch: refreshCallback.value
})

const { data, error, refresh } = await useAuthedFetch<{
  rows: ReadingResource[]
}>(() => apiUri.value, {
  lazy: true,
  immediate: false,
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
  await refresh()

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
  <Swiper
    :modules="[EffectCards, Mousewheel]"
    effect="cards"
    grab-cursor
    mousewheel
    class="w-full max-w-[300px] sm:max-w-[330px]"
    @swiper="onSwiper"
  >
    <SwiperSlide class="aspect-[2/3] rounded-[36px] overflow-hidden">
      <div
        class="w-full h-full border-5 border-[#3566CD] rounded-[36px] bg-white flex flex-col items-center justify-center gap-4 text-[#3566CD]"
        @click="onCreate"
      >
        <UIcon
          name="i-heroicons-plus"
          class="size-16"
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
</template>

<style>
.swiper-slide-shadow-cards {
  /* background: transparent !important;
  opacity: 0 !important; */
  border-radius: 36px !important;
}
</style>
