<script lang="ts" setup>
import { useScroll } from '@vueuse/core'

const route = useRoute()
const { y } = useScroll(window)
const showBorder = computed(() => y.value > 0)
</script>

<template>
  <UApp>
    <UHeader :class="[showBorder ? '' : 'border-none', 'transition-all duration-300']">
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
      </template>
    </UHeader>

    <UMain
      class="min-h-[calc(100vh-147px)] pb-[97px]"
    >
      <slot />
      <UCard
        :ui="{
          body: 'p-[8px] sm:p-[8px] flex flex-row items-center justify-center gap-x-2'
        }"
        class="fixed bottom-4 left-1/2 -translate-x-1/2 z-50 rounded-[20px]"
      >
        <UButton
          class="h-[64px] w-[64px] rounded-2xl flex flex-row items-center justify-center p-0"
          :variant="route.name === 'dashboard' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="reading"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UButton
          class="h-[64px] w-[64px] rounded-2xl flex flex-row items-center justify-center p-0"
          :variant="route.name === 'posts' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="posts"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UButton
          class="h-[64px] w-[64px] rounded-2xl flex flex-row items-center justify-center p-0"
          :variant="route.name === 'assignments' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="assignments"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UButton
          class="h-[64px] w-[64px] rounded-2xl flex flex-row items-center justify-center p-0"
          :variant="route.name === 'profile' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="users"
            class="text-[34px] mb-0"
          />
        </UButton>
      </UCard>
    </UMain>
  </UApp>
</template>
