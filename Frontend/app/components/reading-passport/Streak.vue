<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'

interface StreakData {
  currentStreakDays: number
  totalPoints: number
  weeklyStatus: Array<{
    date: string
    hasStreak: boolean
  }>
}

interface WeekDateDisplay {
  date: Date
  dateNum: number
  month: string
  hasStreak: boolean
  isToday: boolean
}

const streakData = ref<StreakData | null>(null)
const pending = ref(false)

const weekDates = computed<WeekDateDisplay[]>(() => {
  if (!streakData.value?.weeklyStatus) return []

  return streakData.value.weeklyStatus.map((status) => {
    // Parse yyyy-mm-dd format explicitly
    const [year, month, day] = status.date.split('-').map(Number)
    const date = new Date(year || 2026, (month || 1) - 1, day) // month is 0-indexed

    const today = new Date()
    today.setHours(0, 0, 0, 0)
    date.setHours(0, 0, 0, 0)

    return {
      date,
      dateNum: date.getDate(),
      month: date.toLocaleDateString('en-US', { month: 'short' }),
      hasStreak: status.hasStreak,
      isToday: date.getTime() === today.getTime()
    }
  })
})

const showLeftMonth = computed(() => {
  if (weekDates.value.length === 0) return null
  const firstDate = weekDates.value[0]
  // Show left month if first date is from previous month relative to most dates
  return firstDate?.month
})

const showRightMonth = computed(() => {
  if (weekDates.value.length === 0) return null
  const lastDate = weekDates.value[weekDates.value.length - 1]
  const firstDate = weekDates.value[0]
  // Show right month if it's different from first month
  return lastDate?.month !== firstDate?.month ? lastDate?.month : null
})

async function fetchStreak() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<StreakData>>('/streaks/me')
    if (response.data) {
      streakData.value = response.data
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }
}

onMounted(() => {
  fetchStreak()
})
</script>

<template>
  <div
    v-if="pending"
    class="w-full justify-center md:justify-end flex flex-row items-center gap-x-4"
  >
    <UIcon
      name="i-heroicons-arrow-path"
      class="animate-spin text-2xl"
    />
  </div>

  <div
    v-else-if="streakData"
    class="w-full justify-center md:justify-end flex flex-row items-center gap-x-4"
  >
    <div>
      <nuxt-img
        class="max-h-[100px]"
        src="/fire.png"
      />
    </div>
    <div>
      <p class="text-primary font-semibold tracking-tight mb-[10px]">
        {{ streakData.currentStreakDays }} Days Streak. Keep it up!
      </p>
      <div class="flex flex-col gap-y-1">
        <div class="flex flex-row justify-between items-center px-4">
          <span
            v-if="showLeftMonth"
            class="text-xs font-medium"
          >{{ showLeftMonth }}</span>
          <span
            v-if="showRightMonth"
            class="text-xs font-medium"
          >{{ showRightMonth }}</span>
        </div>
        <UCard
          class="rounded-2xl"
          variant="soft"
          :ui="{
            body: 'flex flex-row rounded-2xl gap-x-2 p-3 sm:p-3'
          }"
        >
          <div
            v-for="(dateItem, index) in weekDates"
            :key="index"
            class="flex flex-col items-center"
          >
            <p
              class="text-xs font-semibold mb-1"
              :class="dateItem.isToday ? 'text-primary' : 'text-gray-600'"
            >
              {{ dateItem.dateNum }}
            </p>
            <nuxt-icon
              :name="dateItem.hasStreak ? 'check' : 'no-streak'"
              :filled="false"
              class="text-[20px]"
              :class="dateItem.hasStreak ? 'text-primary' : 'text-[#CCCCCC]'"
            />
          </div>
        </UCard>
      </div>
    </div>
  </div>
</template>
