<script setup lang="ts">
import { Swiper, SwiperSlide } from 'swiper/vue'
import type { Swiper as SwiperType } from 'swiper'

// Swiper styles (WAJIB)
import 'swiper/css'
import 'swiper/css/effect-cards'

// Swiper module
import { EffectCards, Mousewheel } from 'swiper/modules'
import { $authedFetch, handleResponseError } from '~/apis/api'
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

const rows = ref<ReadingResource[]>([])
const swiperInstance = ref<SwiperType>()

const onSwiper = (swiper: SwiperType) => {
  swiperInstance.value = swiper
}

async function fetch() {
  const type = props.journal ? 'journals' : 'books'
  try {
    const data = await $authedFetch<{
      rows: ReadingResource[]
    }>(`/reading-resources/${type}`)
    if (data.rows) {
      rows.value = data.rows
    } else {
      handleResponseError(data)
    }
  } catch (error) {
    handleResponseError(error)
  }
}

onMounted(async () => {
  await fetch()

  if (rows.value.length > 0)
    swiperInstance.value?.slideNext()
})

function onRefresh() {
  fetch()
}

function onCreate() {
  useRouter().push(props.journal ? { name: 'CreateReadingBook' } : { name: 'CreateReadingJournal' })
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
