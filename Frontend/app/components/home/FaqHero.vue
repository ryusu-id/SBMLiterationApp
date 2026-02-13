<template>
  <section class="py-[40px] md:py-[80px]">
    <UContainer>
      <div class="grid grid-cols-12 pt-5 md:pt-8 md:p-8 rounded-2xl gap-8">
        <!-- FAQ SECTION -->
        <div class="col-span-12 lg:col-span-12">
          <div class="mx-auto px-4">
            <h2
              class="text-[32px] md:text-[40px] font-[500] tracking-tight mb-10"
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
                  class="w-full flex justify-between items-center p-5 text-left transition-colors"
                  @click="toggle(index)"
                >
                  <span class="font-[600] tracking-tight">
                    {{ item.question }}
                  </span>

                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="h-5 w-5 transition-transform"
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
                  class="p-5 pt-0 leading-relaxed space-y-3"
                >
                  <!-- Jika answer berupa text biasa -->
                  <p v-if="typeof item.answer === 'string'">
                    {{ item.answer }}
                  </p>

                  <!-- Jika answer berupa bullet object -->
                  <div v-else-if="item.answer.bullets">
                    <p>{{ item.answer.intro }}</p>

                    <ul class="list-disc pl-6 space-y-1">
                      <li
                        v-for="(bullet, i) in item.answer.bullets"
                        :key="i"
                      >
                        {{ bullet }}
                      </li>
                    </ul>

                    <p class="pt-2">
                      {{ item.answer.outro }}
                    </p>
                  </div>

                  <!-- Jika answer berupa HTML dengan link -->
                  <div
                    v-else-if="item.answer.html"
                    v-html="item.answer.html"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- IMAGE SECTION -->
        <!-- <div class="col-span-12 lg:col-span-6 flex items-end justify-center">
          <div
            class="w-fit h-[90%] rounded-t-full flex items-end justify-center"
          >
            <img
              class="h-full w-auto"
              src="https://static.vecteezy.com/system/resources/thumbnails/057/630/740/small/smiling-african-american-student-with-books-and-backpack-on-a-transparent-background-png.png"
              alt="FAQ Illustration"
            >
          </div>
        </div> -->
      </div>
    </UContainer>
  </section>
</template>

<script setup>
import { ref } from 'vue'

const activeIndex = ref(null)

const faqs = [
  {
    question: 'Who is this app for?',
    answer:
      'This app is designed primarily for college students majoring in management and entrepreneurship who want to build a consistent reading habit and improve critical thinking skills. However, students from other majors may also use the app if they are interested.'
  },
  {
    question: 'Are students required to read specific books?',
    answer:
      'No. This is your chance to get out of academic context and explore your reading preference as much as possible. Any format (physical, ebook, or audiobook), any genre, anything to your liking!'
  },
  {
    question: 'How is reading activity tracked?',
    answer: {
      intro: 'Students log their reading manually by:',
      bullets: [
        'Recording the number of pages read',
        'Logging reading sessions',
        'Submitting short reflections or responses'
      ],
      outro:
        'The system tracks reading frequency, consistency, and progress over time.'
    }
  },
  {
    question: 'What kind of reflection or report is required?',
    answer: {
      intro: 'Reflections are short and structured, focusing on:',
      bullets: [
        'Key ideas or insights from the reading',
        'Personal interpretation or takeaway',
        'Relevance to daily life, leadership, or decision-making'
      ],
      outro:
        'Formal summaries or academic writing are not required.'
    }
  },
  {
    question: 'How does the points or rewards system work?',
    answer: {
      intro: 'Points are awarded based on:',
      bullets: [
        'Reading consistency',
        'Completion of reading logs',
        'Streaks and milestones'
      ],
      outro:
        'The system is designed to encourage habit formation.'
    }
  },
  {
    question: 'How is my data and privacy protected?',
    answer: {
      html: 'We take your privacy seriously and only collect data necessary to provide the reading tracking service. Your reading data is stored securely and is never shared with third parties without your consent. For complete details about how we collect, use, and protect your information, please read our <a href="/privacy-policy" class="text-primary hover:underline font-medium">Privacy Policy</a> and <a href="/terms-of-service" class="text-primary hover:underline font-medium">Terms of Service</a>.'
    }
  }
]

const toggle = (index) => {
  activeIndex.value = activeIndex.value === index ? null : index
}
</script>
