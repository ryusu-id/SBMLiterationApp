<script lang="ts" setup>
import { $authedFetch, type ApiResponse } from '~/apis/api'
import ReadingResourceForm, {
  type ReadingResourceSchema
} from '~/components/reading-passport/ReadingResourceForm.vue'

definePageMeta({
  name: 'CreateReadingJournal',
  middleware: ['auth', 'participant-only']
})

const loading = ref(false)
const toast = useToast()
async function handleSubmit(
  data: Omit<ReadingResourceSchema, 'authors'> & { authors: string }
) {
  try {
    loading.value = true
    const response = await $authedFetch<ApiResponse>('/reading-resources/journals', {
      method: 'POST',
      body: {
        ...data
      }
    })

    if (response.errorCode || response.errorDescription)
      toast.add({
        title: 'Error',
        description: response.errorDescription || 'An error occurred while creating the journal.',
        color: 'error'
      })
    else {
      toast.add({
        title: 'Journal Created',
        description: 'The reading journal has been created successfully.',
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
        class="overflow-visible mb-[100px]"
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
              title="Add Journal"
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
          <div
            class="flex flex-row justify-center relative overflow-visible border-0 ring-0 text-white mb-12 md:mb-29"
          >
            <div
              class="h-48 md:h-60 aspect-2/3 rounded-2xl absolute -top-12 flex justify-center items-center text-[#3566CD] bg-[#F5F5F7]"
            >
              <UIcon
                name="i-heroicons-book-open"
                class="size-14"
                @click="useRouter().back()"
              />
            </div>

            <h6
              class="absolute bottom-[-175px] md:bottom-[-225px] left-1/2 -translate-x-1/2 text-[#737373] dark:text-white text-[14px]"
            >
              Cover
            </h6>
          </div>
        </template>
      </UCard>
      <ReadingResourceForm
        journal
        :loading
        @submit="handleSubmit"
      />
    </UContainer>
  </div>
</template>
