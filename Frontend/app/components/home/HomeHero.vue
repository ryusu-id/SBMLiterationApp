<template>
  <div class="hero-container min-h-screen">
    <UPageHero
      title="No Longer Reading Feels Boring or Burdensome"
      description="Track your progress, get XP daily, and see how you rank against other students. Turn every page you read into a victory."
      :links="links"
    />
  </div>
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

<style scoped>
.hero-container {
  position: relative;
  margin-top: calc(-1 * var(--ui-header-height, 0px));
  padding-top: var(--ui-header-height, 0px);
  background-image: url('/background-hero.png');
  background-size: cover;
  background-position: center;
  background-repeat: no-repeat;
}

@media (max-width: 768px) {
  .hero-container {
    background-size: 150% 100%;
    background-position: center top;
  }
}

.hero-container::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 200px;
  background: linear-gradient(to bottom, transparent, var(--ui-bg));
  pointer-events: none;
}
</style>
