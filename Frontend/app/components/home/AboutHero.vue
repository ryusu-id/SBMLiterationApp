<template>
  <section class="py-[40px] md:py-[120px]">
    <UContainer>
      <div class="grid grid-cols-12 gap-8">
        <!-- LEFT CONTENT -->
        <div class="col-span-12 lg:col-span-7 flex flex-col justify-between">
          <div class="font-poppins tracking-tight text-[50px] font-[500] mb-8">
            <h1 class="leading-tight">
              Discover New and Exciting
            </h1>
            <h1 class="leading-tight">
              Reading Experience with
            </h1>
            <h1 class="leading-tight">
              SIGMA
            </h1>
          </div>
        </div>

        <!-- RIGHT FEATURES -->
        <div class="col-span-12 lg:col-span-5 p-6 rounded-xl">
          <h1 class="text-[30px] font-[500] mb-8 leading-none tracking-tight">
            What Makes This App Different?
          </h1>

          <ul class="flex flex-col space-y-4">
            <li
              v-for="it, i in datas"
              :key="i"
              class="flex space-x-3"
            >
              <div class="p-2 rounded-full w-fit h-fit flex items-center">
                <UIcon
                  :name="it.icon"
                  class="size-6"
                />
              </div>
              <div>
                <h2 class="text-[18px] font-[500]">
                  {{ it.title }}
                </h2>
                <p class="text-[13px] font-[300] tracking-tight">
                  {{ it.desc }}
                </p>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </UContainer>
  </section>
</template>


<script setup lang="ts">
const datas = [
  {
    icon: 'i-heroicons-bolt',
    title: 'Built for Daily Use',
    desc: 'Got time for a one-page reading only today? No worries! The app is designed for daily momentum, not guilt.'
  },
  {
    icon: 'i-heroicons-sparkles',
    title: 'Curated just for You',
    desc: 'Everything is selected to expand how future managers and entrepreneurs think, not just what they know.'
  },
  {
    icon: 'i-heroicons-trophy',
    title: 'Gamified Reading Experience',
    desc: 'Earn points, track progress, hit milestones! Celebrate a victory with every page that you read.'
  }
]

</script>