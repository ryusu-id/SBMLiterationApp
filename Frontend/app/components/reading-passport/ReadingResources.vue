<template>
  <Swiper
    :modules="[EffectCards]"
    effect="cards"
    grabCursor
    class="w-full max-w-[300px]"
  >
    <SwiperSlide v-for="res in resources" :key="res.title" class="rounded-xl">
      <UCard variant="unstyled" class="bg-[#3566CD] rounded-[21px]">
        <template #header>
          <UPageHeader
            :title="res.title"
            class="line-clamp-3 border-0 pb-0 text-[60px]"
          />
        </template>
        <div
          class="w-full max-w-[150px] aspect-[3/4] overflow-hidden rounded-[12px]"
        >
          <img
            :src="res.imageUrl"
            alt="Resource Image"
            class="w-full h-full object-cover"
          />
        </div>

        <template #footer>
          <div class="flex flex-row justify-between text-white font-semibold">
            <div>
              {{ res.type === "book" ? "Book" : "Journal" }}
            </div>
            <div class="text-[17px]">
              {{ res.totalReadPages }}/{{ res.totalPages }}
            </div>
          </div>
        </template>
      </UCard>
    </SwiperSlide>
  </Swiper>
</template>

<script setup lang="ts">
import { Swiper, SwiperSlide } from "swiper/vue";

// Swiper styles (WAJIB)
import "swiper/css";
import "swiper/css/effect-cards";

// Swiper module
import { EffectCards } from "swiper/modules";

defineProps<{
  resources: {
    title: string;
    imageUrl: string;
    totalPages: number;
    totalReadPages: number;
    type: "book" | "journal";
  }[];
}>();
</script>
