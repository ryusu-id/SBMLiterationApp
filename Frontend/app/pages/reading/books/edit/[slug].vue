<script lang="ts" setup>
import { $authedFetch } from '~/apis/api'
import ReadingResourceForm, { type ReadingResourceSchema } from '~/components/reading-passport/ReadingResourceForm.vue'

const slug = useRoute().params.slug as string
const formRef = useTemplateRef<typeof ReadingResourceForm>('formRef')

// TODO-SSR-Fetch
onMounted(async () => {
  const response = await $authedFetch<Omit<ReadingResourceSchema, 'authors'> & { authors: string }>(`/reading-resources/${slug}`)
  formRef.value?.setState({
    ...response,
    authors: response.authors.length > 0 ? response.authors.split(',') : ['']
  })
})

const loading = ref(false)
async function handleSubmit(data: Omit<ReadingResourceSchema, 'authors'> & { authors: string }) {
  try {
    loading.value = true
    await $authedFetch(`/reading-resources/${slug}`, {
      method: 'PUT',
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
          title="Edit Book"
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
        ref="formRef"
        :loading
        @submit="handleSubmit"
      />
    </UContainer>
  </div>
</template>
