<script lang="ts" setup>
const route = useRoute()
const navButtonClass = 'h-[48px] w-[48px] rounded-2xl flex flex-row items-center justify-center p-0'
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-right'
    }"
  >
    <UMain
      class="min-h-[calc(100vh-147px)] pb-[113px] overflow-x-hidden participant"
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
          :class="navButtonClass"
          :variant="route.name === 'dashboard' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="home"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UButton
          to="/leaderboard"
          :class="navButtonClass"
          :variant="route.name === 'leaderboard' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="trophy"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UButton
          to="/feeds"
          :class="navButtonClass"
          :variant="route.name === 'feeds' ? 'solid' : 'soft'"
        >
          <nuxt-icon
            name="heart"
            class="text-[34px] mb-0"
          />
        </UButton>
        <UTooltip text="Coming Soon">
          <div class="relative">
            <UButton
              :class="navButtonClass"
              :variant="route.name === 'posts' ? 'solid' : 'soft'"
              disabled
            >
              <nuxt-icon
                name="posts"
                class="text-[34px] mb-0"
              />
            </UButton>
            <UBadge
              color="primary"
              variant="solid"
              size="xs"
              class="absolute -top-1 -right-1 pointer-events-none"
              label="Soon"
            />
          </div>
        </UTooltip>
        <UTooltip text="Coming Soon">
          <div class="relative">
            <UButton
              :class="navButtonClass"
              :variant="route.name === 'assignments' ? 'solid' : 'soft'"
              disabled
            >
              <nuxt-icon
                name="assignments"
                class="text-[34px] mb-0"
              />
            </UButton>
            <UBadge
              color="primary"
              variant="solid"
              size="xs"
              class="absolute -top-1 -right-1 pointer-events-none"
              label="Soon"
            />
          </div>
        </UTooltip>
        <UButton
          :class="navButtonClass"
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
