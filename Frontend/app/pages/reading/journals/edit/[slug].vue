<script lang="ts" setup>
import { $authedFetch, type ApiResponse } from '~/apis/api'
import ReadingResourceForm, {
  type ReadingResourceSchema
} from '~/components/reading-passport/ReadingResourceForm.vue'

definePageMeta({
  name: 'UpdateReadingJournal',
  middleware: ['auth', 'participant-only']
})

const slug = useRoute().params.slug as string
const formRef = useTemplateRef<typeof ReadingResourceForm>('formRef')

// TODO-SSR-Fetch
onMounted(async () => {
  const response = await $authedFetch<
    { data: Omit<ReadingResourceSchema, 'authors'> & { authors: string } }
  >(`/reading-resources/${slug}`)
  formRef.value?.setState({
    ...response.data,
    authors: response.data.authors?.length > 0 ? response.data.authors.split(',') : ['']
  })
})

const loading = ref(false)
const toast = useToast()
async function handleSubmit(
  data: Omit<ReadingResourceSchema, 'authors'> & { authors: string }
) {
  try {
    loading.value = true
    const response = await $authedFetch<ApiResponse>(`/reading-resources/${slug}`, {
      method: 'PUT',
      body: {
        ...data
      }
    })

    if (response.errorCode || response.errorDescription)
      toast.add({
        title: 'Error',
        description: response.errorDescription || 'An error occurred while updating the journal.',
        color: 'error'
      })
    else {
      toast.add({
        title: 'Journal Updated',
        description: 'The reading journal has been updated successfully.',
        color: 'success'
      })

      useRouter().back()
    }
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <!-- TODO: Adjust spacing on the page -->
  <div class="flex flex-col items-center justify-center gap-4 h-full">
    <UContainer class="flex flex-col gap-y-4">
      <!-- TODO: Style below card to follow figma design -->
      <UCard
        class="overflow-visible"
        :ui="{
          header: ' border-0',
          root: 'bg-primary mx-[-16px] sm:mx-[-24px] lg:mx-[-32px] rounded-t-none rounded-b-3xl px-4 py-4'
        }"
      >
        <template #header>
          <div class="flex items-start justify-between gap-2">
            <div class="text-white">
              <UIcon
                name="i-heroicons-chevron-left"
                class="size-6"
                @click="useRouter().back()"
              />
            </div>
            <UPageHeader
              title="Update Journal"
              :ui="{
                root: 'py-0 border-0 pb-10',
                wrapper: 'lg:justify-center',
                title:
                  'text-white text-center  line-clamp-1 text-[22px] lg:text-[24px] leading-tight font-medium'
              }"
              class="flex-1 border-0"
            />
            <!-- This is just to make the title center -->
            <div class="text-transparent">
              <UIcon
                name="i-heroicons-chevron-left"
                class="size-6"
              />
            </div>
          </div>
        </template>

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
        is-edit
        :loading
        @submit="handleSubmit"
      />
    </UContainer>
  </div>
</template>
