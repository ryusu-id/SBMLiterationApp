<template>
  <UPageHero
    title="Discover Reading Challenges Tailored to Your Goals"
    description="Track your progress, conquer daily challenges, and see how you rank against readers worldwide. Turn every book you read into a victory."
    headline="SuperApp"
    :links="links"
  />
</template>

<script setup lang="ts">
import type { ButtonProps } from '@nuxt/ui'

const links = ref<ButtonProps[]>([
  {
    label: 'Join Program Now',
    to: '/signin',
    icon: 'i-heroicons-arrow-right-circle'
  }
])
</script>
