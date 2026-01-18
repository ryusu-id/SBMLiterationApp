<template>
  <div
    class="flex flex-col items-center justify-center gap-4 p-4 h-full"
  >
    <UPageCard class="w-full max-w-md">
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
      <PlaceHolder class="h-32" />
    </UPageCard>
  </div>
</template>

<script setup lang="ts">
import { $authedFetch, useAuth } from '~/apis/api'

definePageMeta({
  middleware: 'auth'
})

async function sendThings() {
  const result = await $authedFetch('/test-items')
  console.log(result)
}

const auth = useAuth()
function sendRefreshToken() {
  auth.requestRefreshToken()
}
</script>
