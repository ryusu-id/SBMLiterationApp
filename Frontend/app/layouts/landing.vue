<script setup>
import { useAuth } from '~/apis/api'

useHead({
  meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }],
  link: [{ rel: 'icon', href: '/favicon.ico' }],
  htmlAttrs: {
    lang: 'en'
  }
})

const auth = useAuth()
const isAuthLoading = ref(true)
const isScrolled = ref(false)

onMounted(() => {
  auth.getToken()
  auth.getRefreshToken()
  isAuthLoading.value = false

  const handleScroll = () => {
    isScrolled.value = window.scrollY > 20
  }

  window.addEventListener('scroll', handleScroll)

  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
  })
})

function goToDashboard() {
  if (auth.getRoles()?.includes('admin')) {
    navigateTo('/admin')
  } else {
    navigateTo('/dashboard')
  }
}
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-right'
    }"
  >
    <UHeader
      :toggle="false"
      :ui="{
        root: isScrolled ? 'transition' : 'bg-default/0 backdrop-blur-none border-none transition'
      }"
    >
      <template #left>
        <NuxtLink to="/">
          <AppLogo class="h-12 shrink-0" />
        </NuxtLink>
      </template>

      <template #right>
        <UColorModeButton />

        <UButton
          v-if="isAuthLoading"
          color="neutral"
          variant="ghost"
          loading
          disabled
        />
        <UButton
          v-else-if="!auth.getToken()"
          to="/signin"
          color="neutral"
          variant="ghost"
          label="Sign In"
        />
        <UButton
          v-else
          color="neutral"
          variant="ghost"
          label="Dashboard"
          @click="goToDashboard"
        />
      </template>
    </UHeader>

    <UMain
      class="min-h-[calc(100vh-147px)]"
    >
      <slot />
    </UMain>

    <USeparator>
      <img
        src="/3-short.png"
        class="h-auto w-6"
        alt="Logo"
      >
    </USeparator>

    <UFooter>
      <template #left>
        <p class="text-sm text-muted">
          PureTCO • © {{ new Date().getFullYear() }}
          by ryusu.id
        </p>
      </template>

      <template #right>
        <div class="flex items-center gap-4">
          <NuxtLink
            to="/privacy-policy"
            class="text-sm text-muted hover:text-primary transition-colors"
          >
            Privacy Policy
          </NuxtLink>
          <NuxtLink
            to="/terms-of-service"
            class="text-sm text-muted hover:text-primary transition-colors"
          >
            Terms of Service
          </NuxtLink>
        </div>
      </template>
    </UFooter>
  </UApp>
</template>

<style scoped>
.transition {
  transition: all 0.3s ease-in-out;
}
</style>
