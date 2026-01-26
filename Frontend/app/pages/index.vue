<script lang="ts" setup>
import AboutHero from '~/components/home/AboutHero.vue'
import ExploreHero from '~/components/home/ExploreHero.vue'
import FaqHero from '~/components/home/FaqHero.vue'
// import HeaderHero from '~/components/home/HeaderHero.vue'
import HighlightHero from '~/components/home/HighlightHero.vue'
import HomeHero from '~/components/home/HomeHero.vue'

definePageMeta({
  layout: 'landing',
  name: 'LandingPage'
})
</script>

<template>
  <div>
    <HomeHero />
    <!-- <HeaderHero /> -->
    <HighlightHero />

    <AboutHero />
    <ExploreHero />
    <FaqHero />
  </div>
</template>
