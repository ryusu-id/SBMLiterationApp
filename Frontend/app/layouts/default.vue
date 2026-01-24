<script lang="ts" setup>
const route = useRoute()
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-right'
    }"
  >
    <UMain
      class="min-h-[calc(100vh-147px)] pb-[113px]"
    >
      <slot />
      <UCard
        :ui="{
          body: 'p-[8px] sm:p-[8px] flex flex-row items-center justify-center gap-x-2'
        }"
        class="fixed bottom-4 left-1/2 -translate-x-1/2 z-50 rounded-[20px]"
      >
        <UButton
          to="/dashboard"
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
          to="/profile"
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
