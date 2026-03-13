<script setup lang="ts">
import { DotLottieVue } from '@lottiefiles/dotlottie-vue'

const { initializeReminder } = useReminder()
const { autoSubscribe } = usePushNotification()

const lottie = useLottie()

onMounted(() => {
  initializeReminder()
  // Silently re-register push subscription if permission was already granted.
  // Small delay so the auth session/JWT cookie is ready before $authedFetch is called.
  setTimeout(autoSubscribe, 1500)
})
</script>

<template>
  <NuxtLayout>
    <UModal
      title=" "
      description=" "
      :overlay="false"
      :open="lottie.shouldRender"
      fullscreen
      :ui="{
        content: 'bg-transparent border-none ring-0 shadow-none'
      }"
    >
      <template #content>
        <div class="w-full h-full flex justify-center items-center">
          <DotLottieVue
            class="w-full h-full"
            :layout="{
              fit: 'contain'
            }"
            :speed="lottie.speed"
            autoplay
            :src="lottie.animation"
          />
        </div>
      </template>
    </UModal>
    <NuxtPage />
    <NuxtPwaManifest />
  </NuxtLayout>
</template>
