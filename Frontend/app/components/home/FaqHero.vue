<template>
  <section class="bg-white dark:bg-transparent py-[40px] md:py-[80px]">
    <UContainer>
      <div class="grid grid-cols-12 bg-amber-50 pt-5 md:pt-8 md:p-8 rounded-2xl gap-8">
        <!-- FAQ SECTION -->
        <div class="col-span-12 lg:col-span-6">
          <div class="mx-auto px-4">
            <h2
              class="text-[32px] md:text-[40px] font-[500] tracking-tight mb-10 text-gray-900"
            >
              Frequently Asked Questions
            </h2>

            <div class="space-y-4">
              <div
                v-for="(item, index) in faqs"
                :key="index"
                class="border border-gray-200 rounded-xl overflow-hidden transition-all duration-300"
                :class="{
                  'ring-2 ring-base border-transparent': activeIndex === index
                }"
              >
                <button
                  class="w-full flex justify-between items-center p-5 text-left bg-white hover:bg-gray-50 transition-colors"
                  @click="toggle(index)"
                >
                  <span class="font-[600] text-gray-800 tracking-tight">
                    {{ item.question }}
                  </span>

                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="h-5 w-5 text-gray-500 transition-transform"
                    :class="{ 'rotate-180': activeIndex === index }"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="19 9l-7 7-7-7"
                    />
                  </svg>
                </button>

                <div
                  v-show="activeIndex === index"
                  class="p-5 pt-0 text-gray-600 bg-white leading-relaxed"
                >
                  {{ item.answer }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- IMAGE SECTION -->
        <div class="col-span-12 lg:col-span-6 flex items-end justify-center">
          <div
            class="w-fit h-[90%] bg-[#e2f047] rounded-t-full flex items-end justify-center"
          >
            <img
              class="h-full w-auto"
              src="https://static.vecteezy.com/system/resources/thumbnails/057/630/740/small/smiling-african-american-student-with-books-and-backpack-on-a-transparent-background-png.png"
              alt="FAQ Illustration"
            >
          </div>
        </div>
      </div>
    </UContainer>
  </section>
</template>

<script setup>
import { ref } from 'vue'

const activeIndex = ref(null)

const faqs = [
  {
    question: 'How do I start a reading challenge?',
    answer:
      'Simply sign up, set your yearly or monthly goal, and start logging the books you read. The system will automatically track your progress.'
  },
  {
    question: 'Can I compete with my friends?',
    answer:
      'Yes! You can follow your friends, join public leaderboards, or create private reading leagues to see who reads the most.'
  },
  {
    question: 'Is there a limit to how many books I can track?',
    answer:
      'Not at all. Whether you read 1 book or 100 books a year, our platform is designed to handle your entire library.'
  },
  {
    question: 'How are the rankings calculated?',
    answer:
      'Rankings are based on the number of pages or books completed within a specific timeframe, depending on the challenge rules.'
  }
]

const toggle = (index) => {
  activeIndex.value = activeIndex.value === index ? null : index
}
</script>
