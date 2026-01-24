<script setup lang="ts">
// TODO: add discriminator for display for Dashboard (with image) and non Dashboard (without image)
defineProps<{
  report: {
    title: string;
    imageUrl: string;
    insight: string;
    timeSpent: string;
    readDate: string;
    currentPage: number;
    totalPage: number;
  };
  withImage?: boolean;
}>();
</script>

<template>
  <!-- TODO: Style and space all of below component to match -->
  <UCard
    class=""
    :ui="{
      root: 'ring ring-[#EDEDED] bg-white rounded-[20px] max-w-3xl col-span-12 lg:col-span-6',
      body: 'p-2 sm:p-2  ',
    }"
  >
    <div class="flex flex-row gap-x-4">
      <div
        v-if="withImage"
        class="w-[80px] sm:w-[110px] aspect-[3/4] shrink-0 overflow-hidden rounded-[12px]"
      >
        <img
          :src="report.imageUrl"
          alt="Book Cover"
          class="w-full h-full object-cover"
        />
      </div>

      <div class="flex flex-col">
        <div v-if="withImage" class="flex flex-row justify-between">
          <h1
            class="text-[13px] sm:text-[15px] tracking-tight font-bold line-clamp-1 text-dark"
          >
            {{ report.title }}
          </h1>
          <!-- TODO: Use chevron down icon -->
          <nuxt-icon name="i-heroicons-chevron-down" />
        </div>

        <UBadge v-else class="w-fit rounded-full mb-1 text-[12px]">
          Insight
        </UBadge>
        <p
          class="mt-2 text-wrap text-dark line-clamp-2 font-medium tracking-tight text-[11px] sm:text-[13px]"
        >
          {{ report.insight }}
        </p>

        <USeparator
          class="my-2"
          :ui="{
            border: 'border-[#EDEDED]',
          }"
        />
        <div class="flex flex-row justify-between">
          <div class="flex flex-row gap-x-2 items-center">
            <div class="flex flex-col tracking-tight">
              <p
                class="text-[#363030] font-medium text-[10px] sm:text-[12px] leading-none"
              >
                Total Spent
              </p>
              <p class="font-semibold text-fire text-[11px] sm:text-[13px]">
                {{ report.timeSpent }}
              </p>
            </div>
            <USeparator
              orientation="vertical"
              :ui="{
                border: 'border-[#EDEDED]',
              }"
            />
            <div class="flex flex-col tracking-tight">
              <p
                class="text-[#363030] font-medium text-[10px] sm:text-[12px] leading-none"
              >
                Read Date
              </p>
              <p class="font-semibold text-fire text-[11px] sm:text-[13px]">
                {{ report.readDate }}
              </p>
            </div>
          </div>
          <div
            class="flex items-center gap-x-2 tracking-tight pl-3 pr-2 py-1 bg-[#EBEBEB] rounded-full"
          >
            <p class="text-[#363030] font-medium text-[10px] sm:text-[12px]">
              Page Progress
            </p>
            <p
              class="font-semibold text-white py-1 px-2 sm:px-3 sm:py-1 bg-fire rounded-full text-[10px] sm:text-[12px]"
            >
              {{ report.currentPage }}/{{ report.totalPage }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </UCard>
</template>
