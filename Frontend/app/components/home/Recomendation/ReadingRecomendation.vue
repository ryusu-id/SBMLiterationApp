<script setup lang="ts">
// TODO: add discriminator for display for Dashboard (with image) and non Dashboard (without image)
defineProps<{
  book: {
    title: string;
    author: string;
    imageUrl: string;
    totalPage: number;
    xp: number;
  };
}>();
</script>

<template>
  <!-- TODO: Style and space all of below component to match -->
  <UCard
    class=""
    :ui="{
      root: 'ring ring-0 border border-[#D1D1D1] bg-white rounded-[22px] w-fit',
      body: 'p-2 sm:p-2  ',
    }"
  >
    <div class="flex flex-col gap-x-4">
      <div
        class="w-[178px] sm:w-[240px] aspect-[3/4] shrink-0 overflow-hidden rounded-[12px] relative"
      >
        <div
          class="absolute right-3 top-3 bg-primary w-fit h-fit tracking-tight font-semibold px-2 py-1 rounded-lg text-[12px]"
        >
          {{ book.xp }}xp
        </div>
        <img
          :src="book.imageUrl"
          alt="Book Cover"
          class="w-full h-full object-cover"
        />
      </div>

      <div class="flex flex-col mt-[15px]">
        <div class="flex flex-col justify-between mb-[10px]">
          <h1
            class="text-[13px] sm:text-[15px] tracking-tight font-bold line-clamp-1 leading-none text-dark"
          >
            {{ book.title }}
          </h1>
          <h2
            class="text-[11px] sm:text-[13px] tracking-tight text-primary font-semibold line-clamp-1 text-primary leading-tight"
          >
            {{ book.category }}
          </h2>
        </div>

        <div
          class="flex flex-row gap-x-2 items-center justify-between w-full mb-[10px]"
        >
          <div class="flex flex-col tracking-tight">
            <p
              class="text-[#363030] font-medium text-[10px] sm:text-[12px] leading-none"
            >
              Author
            </p>
            <p
              class="font-semibold text-[11px] sm:text-[13px] line-clamp-1 text-dark max-w-[150px]"
            >
              {{ book.author }}
            </p>
          </div>
          <div class="flex flex-col tracking-tight text-right min-w-[50px]">
            <p
              class="text-[#363030] font-medium text-[10px] sm:text-[12px] leading-none"
            >
              Total Page
            </p>
            <p class="font-semibold text-[11px] sm:text-[13px] text-dark">
              {{ book.totalPage }}
            </p>
          </div>
        </div>
        <div>
          <UButton
            class="w-full font-semibold rounded-full text-center bg-primary text-[13px] justify-center"
          >
            Add to Read List
          </UButton>
        </div>
      </div>
    </div>
  </UCard>
</template>
