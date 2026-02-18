<template>
  <section class="py-[40px] md:py-[120px]">
    <UContainer>
      <div class="grid grid-cols-12 gap-8">
        <!-- LEFT CONTENT -->
        <div class="col-span-12 lg:col-span-7">
          <div
            class="text-[48px] md:text-[70px] tracking-tight leading-tight mb-4"
          >
            <h1>How It Works</h1>
          </div>

          <ol class="flex flex-col space-y-4 mb-8">
            <li
              v-for="(it, i) in datas"
              :key="i"
              class="flex space-x-3"
            >
              <span class="text-[14px] font-[500] w-[30px] h-[30px] bg-primary    text-center rounded-full flex items-center justify-center ">
                {{ i + 1 }}
              </span>

              <div>
                <h2 class="text-[18px] font-[500]">
                  {{ it.title }}
                </h2>
                <p class="text-[13px] font-[300] tracking-tight">
                  {{ it.desc }}
                </p>
              </div>
            </li>
          </ol>

          <UButton
            class="px-8 py-4 w-fit rounded-full font-[700] cursor-pointer"
            label="Start Reading Now"
            @click="router.push('/signin')"
          />
        </div>

        <!-- RIGHT IMAGE -->
        <div class="col-span-12 lg:col-span-5 flex items-end justify-center">
          <div class="w-full lg:w-fit lg:h-[90%]">
            <img
              class="w-full h-auto rounded-2xl"
              src="/sigma-screens.png"
              alt="Program Preview"
            >
          </div>
        </div>
      </div>
    </UContainer>
  </section>
</template>

<script setup lang="ts">
const router = useRouter()
const datas = [
  {
    title: 'Choose What You Want to Read',
    desc: 'Pick your reading and start tracking your progress everyday.'
  },
  {
    title: 'Read at Your Own Pace',
    desc: 'Anytime. Anywhere. Even 1 minute or 1 page counts.'
  },
  {
    title: 'Log & Reflect',
    desc: 'Share short reflections and do a daily check-in.'
  },
  {
    title: 'Earn Points & Build Streaks',
    desc: 'Consistency is rewarded. Progress is visible.'
  },
  {
    title: 'Grow—Quietly but Powerfully',
    desc: 'Better ideas. Broader perspectives. Stronger thinking.'
  }
]
</script>
