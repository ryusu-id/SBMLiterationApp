<script lang="ts" setup>
import AboutHero from '~/components/home/AboutHero.vue'
import ExploreHero from '~/components/home/ExploreHero.vue'
import FaqHero from '~/components/home/FaqHero.vue'
import HighlightHero from '~/components/home/HighlightHero.vue'
import HomeHero from '~/components/home/HomeHero.vue'
import { useAuth } from '~/apis/api'

definePageMeta({
  layout: 'landing',
  name: 'LandingPage',
  middleware: () => {
    // Only redirect on direct access (not from internal navigation)
    const auth = useAuth()
    if (auth.getToken()) {
      return navigateTo('/dashboard')
    }
  }
})
</script>

<template>
  <div>
    <HomeHero />
    <HighlightHero />

    <AboutHero />
    <ExploreHero />
    <FaqHero />
  </div>
</template>
