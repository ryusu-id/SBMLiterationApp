<script setup lang="ts">
import LeaderboardRow from './LeaderboardRow.vue'
import type { LeaderboardEntry } from '~/pages/leaderboard.vue'

defineProps<{
  data: LeaderboardEntry[]
}>()
</script>

<template>
  <div>
    <LeaderboardRow
      first-col="Rank"
      second-col="Name"
      third-col="Points"
      header
    />
    <LeaderboardRow
      v-for="entry in data"
      :key="entry.rank"
      :first-col="String(entry.rank)"
      :second-col="entry.username"
      :third-col="String(entry.exp)"
    />
  </div>
</template>
