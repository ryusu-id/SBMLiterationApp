<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import { useAuth } from '~/apis/api'
import UserMenu from '~/components/layout/UserMenu.vue'

useHead({
  meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }],
  link: [{ rel: 'icon', href: '/favicon.ico' }],
  htmlAttrs: {
    lang: 'en'
  }
})

const auth = useAuth()

onMounted(() => {
  auth.getToken()
  auth.getRefreshToken()
})
const items: NavigationMenuItem[] = [{
  label: 'Home',
  icon: 'i-lucide-house',
  to: '/admin'
}, {
  label: 'Book Categories',
  icon: 'i-lucide-chart-column-stacked',
  to: '/admin/categories'
}, {
  label: 'Book Recommendation',
  icon: 'i-lucide-album',
  to: '/admin/recommendation'
}, {
  label: 'Daily Reads',
  icon: 'i-lucide-calendar-clock',
  to: '/admin/daily-reads'
}, {
  label: 'Groups',
  icon: 'i-lucide-users-round',
  to: '/admin/groups'
}, {
  label: 'Assignments',
  icon: 'i-lucide-clipboard-list',
  to: '/admin/assignments'
}, {
  label: 'Users',
  icon: 'i-lucide-users',
  to: '/admin/users'
}]
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-right'
    }"
  >
    <UDashboardGroup>
      <UDashboardSidebar
        collapsible
        resizable
        :min-size="19"
        :default-size="20"
        :max-size="24"
        :ui="{ footer: 'border-t border-default' }"
      >
        <template #header="{ collapsed }">
          <NuxtLink
            v-if="!collapsed"
            to="/"
          >
            <AppLogo class="w-auto h-6 shrink-0" />
          </NuxtLink>
          <UIcon
            v-else
            name="i-simple-icons-nuxtdotjs"
            class="size-5 text-primary mx-auto"
          />
        </template>

        <template #default="{ collapsed }">
          <UNavigationMenu
            :collapsed="collapsed"
            :items="items"
            orientation="vertical"
          />
        </template>

        <template #footer="{ collapsed }">
          <UserMenu :collapsed="collapsed" />
        </template>
      </UDashboardSidebar>
      <UDashboardPanel :grow="true">
        <slot />

        <!-- Footer with legal links -->
        <div class="mt-auto pt-6 pb-4 border-t border-default">
          <div class="flex flex-col sm:flex-row items-center justify-between gap-2 text-xs text-muted px-4">
            <p>
              PureTCO • © {{ new Date().getFullYear() }} by ryusu.id
            </p>
            <div class="flex items-center gap-4">
              <NuxtLink
                to="/privacy-policy"
                class="hover:text-primary transition-colors"
              >
                Privacy Policy
              </NuxtLink>
              <NuxtLink
                to="/terms-of-service"
                class="hover:text-primary transition-colors"
              >
                Terms of Service
              </NuxtLink>
            </div>
          </div>
        </div>
      </UDashboardPanel>
    </UDashboardGroup>
  </UApp>
</template>
