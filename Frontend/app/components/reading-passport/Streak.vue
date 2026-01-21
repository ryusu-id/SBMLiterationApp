<script setup lang="ts">
interface WeekDate {
  date: Date;
  day: string;
  isStreak: boolean;
}

defineProps<{
  weekDates: WeekDate[];
}>();
</script>

<template>
  <div class="w-full justify-center flex flex-row items-center gap-x-4">
    <!-- TODO: Change below to the streak fire image -->
    <div>Fire Image</div>
    <div>
      <p class="text-fire font-semibold tracking-tight mb-[10px]">
        7 Days Streak. Keep it up!
      </p>
      <div class="flex flex-row gap-x-2 bg-[#F1F1F1] px-4 py-2 rounded-2xl">
        <div v-for="(dateItem, index) in weekDates" :key="index">
          <p class="text-jose">{{ dateItem.day[0] }}</p>
          <nuxt-icon
            :name="dateItem.isStreak ? 'check' : 'no-streak'"
            :filled="false"
            class="text-[20px]"
            :class="dateItem.isStreak ? 'text-fire' : 'text-[#CCCCCC]'"
          />
        </div>
      </div>
    </div>
  </div>
</template>
