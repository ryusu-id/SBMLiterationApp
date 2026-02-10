<script lang="ts" setup>
import AboutHero from '~/components/home/AboutHero.vue'
import ExploreHero from '~/components/home/ExploreHero.vue'
import FaqHero from '~/components/home/FaqHero.vue'
import HighlightHero from '~/components/home/HighlightHero.vue'
import HomeHero from '~/components/home/HomeHero.vue'
import { usePWA } from '../composables/pwa'
import { useAuth } from '~/apis/api'

definePageMeta({
  layout: 'landing',
  name: 'LandingPage'
})

if (import.meta.client) {
  const pwa = usePWA()
  const router = useRouter()
  const auth = useAuth()

  if (pwa?.isPWA.value && auth.getToken())
    router.replace('/dashboard')
}
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
