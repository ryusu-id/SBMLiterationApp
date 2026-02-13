<script setup lang="ts">
import { DotLottieVue } from '@lottiefiles/dotlottie-vue'

const { initializeReminder } = useReminder()

const lottie = useLottie()

onMounted(() => {
  initializeReminder()
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
