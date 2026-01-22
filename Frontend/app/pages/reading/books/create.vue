<script lang="ts" setup>
import { $authedFetch } from '~/apis/api'
import ReadingResourceForm, { type ReadingResourceSchema } from '~/components/reading-passport/ReadingResourceForm.vue'

const loading = ref(false)
async function handleSubmit(data: Omit<ReadingResourceSchema, 'authors'> & { authors: string }) {
  try {
    loading.value = true
    await $authedFetch('/reading-resources/books', {
      method: 'POST',
      body: {
        ...data,
        userId: 1
      }
    })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <!-- TODO: Adjust spacing on the page -->
  <div class="flex flex-col items-center justify-center gap-4 p-4 h-full">
    <UContainer class="flex flex-col gap-y-4">
      <!-- TODO: Style below card to follow figma design -->
      <UCard
        class="overflow-visible mb-[100px]"
      >
        <UPageHeader
          class="border-none"
          title="Add Book to Read"
        />

        <template #footer>
          <div class="flex flex-row justify-between relative overflow-visible">
            <!-- TODO: Change this to the book cover icon
              <img
                :src="readingResource.imageUrl"
                :alt="`${readingResource.title} Cover`"
                class="h-48 aspect-2/3 rounded-md absolute -top-12"
              >
            -->
          </div>
        </template>
      </UCard>
      <ReadingResourceForm
        :loading
        @submit="handleSubmit"
      />
    </UContainer>
  </div>
</template>
