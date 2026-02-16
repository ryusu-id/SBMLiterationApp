<script setup lang="ts">
// TODO: add discriminator for display for Dashboard (with image) and non Dashboard (without image)
defineProps<{
  report: {
    resourceTitle: string
    imageUrl: string
    insight: string
    timeSpent?: number
    readDate?: string
    currentPage: number
    totalPage: number
    coverImageUrl?: string | undefined
  }
  withImage?: boolean
}>()

</script>

<template>
  <!-- TODO: Style and space all of below component to match -->
  <UCard
    variant="soft"
    :ui="{
      root: 'rounded-[20px] max-w-xl col-span-12 lg:col-span-6 bg-elevated',
      body: 'p-2 sm:p-2  '
    }"
  >
    <div class="flex flex-row gap-x-4">
      <div
        v-if="withImage"
        class="w-[80px] aspect-[3/4] shrink-0 overflow-hidden rounded-[12px]"
      >
        <img
          v-if="report.imageUrl"
          :src="report.imageUrl"
          alt="Book Cover"
          class="w-full h-full object-cover"
        >
        <div
          v-else
          class="w-full h-full border-2 border-primary bg-accented rounded-[12px] flex items-center justify-center text-primary"
        >
          <UIcon
            name="i-heroicons-book-open"
            class="size-14"
          />
        </div>
      </div>

      <div class="flex flex-col w-full justify-between">
        <div
          v-if="withImage"
          class="flex flex-row justify-between"
        >
          <h1
            class="text-[13px] sm:text-[15px] tracking-tight font-bold line-clamp-1"
          >
            {{ report.resourceTitle }}
          </h1>
          <!-- TODO: Use chevron down icon -->
        </div>

        <UBadge
          v-else
          class="w-fit rounded-full mb-1 text-[12px]"
        >
          Insight
        </UBadge>
        <div class="flex gap-x-2">
          <p
            class="mt-2 text-wrap line-clamp-2 font-medium tracking-tight text-[11px] sm:text-[13px]"
          >
            {{ report.insight }}
          </p>
          
          <UModal
          :title="report.resourceTitle"
          :close="{
            color: 'primary',
            variant: 'outline',
            class: 'rounded-full'
          }"
          :ui="{
            title: 'line-clamp-1 max-w-[90%]',
             body: 'p-0'
          }"
        >
        
  <UButton icon="i-heroicons-chevron-right" color="primary" class="mt-[8px] rounded-full cursor-pointer" variant="subtle" :ui="{
    base: '!rounded-full'
  }" />

          <template #body>
            <div class="!p-4 sm:!p-0 text-sm">
              {{ report.insight }}
            </div>
          </template>
        </UModal>
        </div>

        <USeparator
          class="my-2"
        />
        <div class="flex flex-row justify-between w-full">
          <div class="flex flex-row gap-x-2 items-center">
            <div
              class="flex flex-col tracking-tight"
            >
              <p
                class="font-medium text-[10px] sm:text-[12px] leading-none"
              >
                Time Spent
              </p>
              <p class="font-semibold text-primary text-[11px] sm:text-[13px] text-primary">
                {{ report.timeSpent }}m
              </p>
            </div>
            <USeparator
              orientation="vertical"
            />
            <div class="flex flex-col tracking-tight">
              <p
                class="font-medium text-[10px] sm:text-[12px] leading-none"
              >
                Read Date
              </p>
              <p class="font-semibold text-primary text-[11px] sm:text-[13px] text-primary">
                {{ report.readDate }}
              </p>
            </div>
          </div>
          <div
            class="flex items-center gap-x-2 tracking-tight pl-3 pr-1 py-[1px] bg-elevated rounded-full"
          >
            <p class="font-medium text-[10px] sm:text-[12px]">
              Page Progress
            </p>
            <p
              class="font-semibold py-0.5 px-2 sm:px-3 bg-primary text-dark rounded-full text-[10px] sm:text-[12px]"
            >
              {{ report.currentPage }}{{ report.totalPage ? '/' + report.totalPage : '' }}
            </p>
          </div>
        </div>
      </div>
    </div>
  </UCard>
</template>
