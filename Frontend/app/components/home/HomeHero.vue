<template>
  <UPageHero
    title="No Longer Reading Feels Boring or Burdensome"
    description="Track your progress, get XP daily, and see how you rank against other students. Turn every page you read into a victory."
    :links="links"
  />
</template>

<script setup lang="ts">
import type { ButtonProps } from '@nuxt/ui'

const links = ref<ButtonProps[]>([
  {
    label: 'Start Your Reading Journey',
    to: '/signin',
    icon: 'i-heroicons-arrow-right-circle'
  }
])
</script>
