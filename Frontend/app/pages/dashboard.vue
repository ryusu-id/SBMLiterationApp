<template>
  <div
    class="max-w-[1200px] mx-auto flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UContainer>
      <div class="flex flex-col space-y-[32px]">
        <div class="flex flex-col md:flex-row">
          <UPageHeader
            class="border-none"
            :ui="{
              root: 'w-full'
            }"
          >
            <template #title>
              <div class="flex flex-col md:flex-row md:space-x-2">
                <h1 class="text-[36px] font-extrabold tracking-tighter">
                  Welcome,
                </h1>
                <h1
                  class="text-[36px] font-extrabold tracking-tighter leading-none"
                >
                  Budi
                </h1>
              </div>
            </template>

            <template #description>
              <p class="tracking-tight text-[20px]">
                Start your reading journey now
              </p>
            </template>
          </UPageHeader>

          <Streak :week-dates="weekDates" />
        </div>

        <div class=" ">
          <UTabs
            :items="tabs"
            class="max-w-[300px] mt-[20px] mb-[30px] mx-auto md:mx-0 md:mr-auto"
          >
            <template #books>
              <ReadingResources :resources="resources" />
            </template>
            <template #journal-paper>
              <ReadingResources
                journal
                :resources="resources"
              />
            </template>
          </UTabs>
        </div>

        <ReadingRecomendationList />
        <ReadingReportList />
      </div>
    </UContainer>
  </div>
</template>

<script setup lang="ts">
import ReadingRecomendationList from '~/components/home/Recomendation/ReadingRecomendationList.vue'
import ReadingReportList from '~/components/reading-passport/ReadingReportList.vue'
import ReadingResources from '~/components/reading-passport/ReadingResources.vue'
import Streak from '~/components/reading-passport/Streak.vue'

// definePageMeta({
//   middleware: 'auth'
// })

const getCurrentWeekDates = () => {
  const week = []
  const today = new Date()
  const currentDay = today.getDay()
  const startOfWeek = new Date(today)
  startOfWeek.setDate(today.getDate() - currentDay)

  for (let i = 0; i < 7; i++) {
    const date = new Date(startOfWeek)
    date.setDate(startOfWeek.getDate() + i)
    week.push({
      date: date,
      day: date.toLocaleDateString('en-US', { weekday: 'long' }),
      isStreak: Math.random() > 0.5 // Replace with actual streak logic
    })
  }
  return week
}

const weekDates = ref<
  {
    date: Date
    day: string
    isStreak: boolean
  }[]
>([])

onMounted(() => {
  weekDates.value = getCurrentWeekDates()
})

async function fetch() {

}

const resources = ref([
  {
    title: 'Domain Driven Design',
    imageUrl:
      'https://miro.medium.com/v2/resize:fit:1200/1*90o12yshV8kprH8mbXdnKQ.png',
    totalPages: 180,
    totalReadPages: 125,
    type: 'book' as const
  },
  {
    title: 'Machine Learning Fundamentals',
    imageUrl: 'https://picsum.photos/seed/journal1/400/300',
    totalPages: 45,
    totalReadPages: 32,
    type: 'journal' as const
  },
  {
    title: '1984',
    imageUrl: 'https://picsum.photos/seed/book2/400/300',
    totalPages: 328,
    totalReadPages: 89,
    type: 'book' as const
  }
])

const tabs = [
  {
    label: 'Books',
    icon: 'i-lucide-book',
    slot: 'books'
  },
  {
    label: 'Research Article',
    icon: 'i-lucide-form',
    slot: 'journal-paper'
  }
]
</script>
