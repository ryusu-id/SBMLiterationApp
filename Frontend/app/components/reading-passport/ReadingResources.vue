<template>
  <Swiper
    :modules="[EffectCards]"
    effect="cards"
    grab-cursor
    class="w-full max-w-[300px] sm:max-w-[330px]"
  >
    <SwiperSlide
      v-for="res in resources"
      :key="res.title"
      class="rounded-[36px] overflow-hidden"
    >
      <UCard
        variant="unstyled"
        class="bg-[#3566CD] rounded-[36px] aspect-[2/3] py-4 px-3 flex flex-col justify-between"
        :ui="{
          root: '',
        }"
      >
        <template #header>
          <div class="flex items-start justify-between gap-2">
            <UPageHeader
              :title="res.title"
              :ui="{
                title: 'text-white max-h-[120px]   line-clamp-3 ',
              }"
              class="flex-1 border-0 p-0"
            />

            <UPopover class="">
              <UButton
                color="white"
                variant="ghost"
                size="lg"
                icon="i-heroicons-ellipsis-vertical"
                class="text-white hover:bg-white/10"
              />

              <template #content>
                <Placeholder class="size-48 m-4 inline-flex" />
              </template>
            </UPopover>
          </div>
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
    <SwiperSlide class="aspect-[2/3] rounded-[36px] overflow-hidden">
      <div
        class="w-full h-full border-5 border-[#3566CD] rounded-[36px] bg-white flex flex-col items-center justify-center gap-4 text-[#3566CD]"
      >
        <UIcon name="i-heroicons-plus" class="size-16" />

        <h1 class="text-center font-semibold text-xl leading-tight">
          New<br />
          Reading Source
        </h1>
      </div>
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

<style>
.swiper-slide-shadow-cards {
  /* background: transparent !important;
  opacity: 0 !important; */
  border-radius: 36px !important;
}
</style>
