<script setup>
import { useAuth } from '~/apis/api'

useHead({
  meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }],
  link: [{ rel: 'icon', href: '/favicon.ico' }],
  htmlAttrs: {
    lang: 'en'
  }
})

const title = 'Nuxt Starter Template'
const description
  = 'A production-ready starter template powered by Nuxt UI. Build beautiful, accessible, and performant applications in minutes, not hours.'

useSeoMeta({
  title,
  description,
  ogTitle: title,
  ogDescription: description,
  ogImage: 'https://ui.nuxt.com/assets/templates/nuxt/starter-light.png',
  twitterImage: 'https://ui.nuxt.com/assets/templates/nuxt/starter-light.png',
  twitterCard: 'summary_large_image'
})

const auth = useAuth()

onMounted(() => {
  auth.getToken()
  auth.getRefreshToken()
})
</script>

<template>
  <UApp>
    <UHeader>
      <template #left>
        <NuxtLink to="/">
          <AppLogo class="w-auto h-6 shrink-0" />
        </NuxtLink>
      </template>

      <template #right>
        <UColorModeButton />

        <UButton
          to="https://github.com/nuxt-ui-templates/starter"
          target="_blank"
          icon="i-simple-icons-github"
          aria-label="GitHub"
          color="neutral"
          variant="ghost"
        />
        <ClientOnly>
          <UButton
            v-if="!auth.getToken()"
            to="/signin"
            color="neutral"
            variant="ghost"
            label="Sign In"
          />
          <UButton
            v-else
            to="/dashboard"
            color="neutral"
            variant="ghost"
            label="Dashboard"
          />
        </ClientOnly>
      </template>
    </UHeader>

    <UMain
      class="min-h-[calc(100vh-147px)]"
    >
      <NuxtPage />
    </UMain>

    <USeparator icon="i-simple-icons-nuxtdotjs" />

    <UFooter>
      <template #left>
        <p class="text-sm text-muted">
          PureTCO • © {{ new Date().getFullYear() }}
          by ryusu.id
        </p>
      </template>
    </UFooter>
  </UApp>
</template>
