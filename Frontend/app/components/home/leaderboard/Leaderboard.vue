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
      second-col="Users"
      third-col="Exp"
      header
    />
    <LeaderboardRow
      v-for="entry in data"
      :key="entry.rank"
      :position="entry.rank"
      :first-col="String(entry.rank)"
      :profile-col="entry.picture"
      :second-col="entry.username"
      :third-col="String(entry.exp)"
    />
  </div>
</template>
