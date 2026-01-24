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
  active: true
}, {
  label: 'Inbox',
  icon: 'i-lucide-inbox',
  badge: '4'
}, {
  label: 'Contacts',
  icon: 'i-lucide-users'
}, {
  label: 'Settings',
  icon: 'i-lucide-settings',
  defaultOpen: true,
  children: [{
    label: 'General'
  }, {
    label: 'Members'
  }, {
    label: 'Notifications'
  }]
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
          <UButton
            :label="collapsed ? undefined : 'Search...'"
            icon="i-lucide-search"
            color="neutral"
            variant="outline"
            block
            :square="collapsed"
          >
            <template
              v-if="!collapsed"
              #trailing
            >
              <div class="flex items-center gap-0.5 ms-auto">
                <UKbd
                  value="meta"
                  variant="subtle"
                />
                <UKbd
                  value="K"
                  variant="subtle"
                />
              </div>
            </template>
          </UButton>

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
      <slot />
    </UDashboardGroup>
  </UApp>
</template>
