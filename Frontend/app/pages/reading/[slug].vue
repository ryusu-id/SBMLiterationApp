<script lang="ts" setup>
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import ReadingReportList from '~/components/reading-passport/ReadingReportList.vue'
import ReadingReportForm from '~/components/reading-passport/ReadingReportForm.vue'
import type { ReadingResource } from '~/components/reading-passport/ReadingResources.vue'
import type { ReadingReportData } from '~/components/reading-passport/ReadingReportList.vue'
import type { PagingResult } from '~/apis/paging'

definePageMeta({
  name: 'ReadingReport',
  middleware: ['auth', 'participant-only']
})

const route = useRoute()

const readingResource = ref<ReadingResource | null>(null)
const readingReports = ref<ReadingReportData[]>([])
const pending = ref(false)
const reportPending = ref(false)
const form = useTemplateRef<typeof ReadingReportForm>('form')
const formLoading = ref(false)
const toast = useToast()

async function onSubmit(data: { readingResourceId: number, currentPage: number, insight: string }) {
  try {
    formLoading.value = true
    await $authedFetch('/reading-resources/reports', {
      method: 'POST',
      body: data
    })

    toast.add({
      title: 'Reading report created successfully',
      color: 'success'
    })

    form.value?.close()

    // Refresh the reports list
    try {
      reportPending.value = true
      const response = await $authedFetch<PagingResult<ReadingReportData>>(`/reading-resources/reports`, {
        query: {
          readingResourceId: readingResource.value?.id || 0,
          page: 1,
          pageSize: 100
        }
      })
      if (response.rows)
        readingReports.value = response.rows
    } catch (err) {
      handleResponseError(err)
    } finally {
      reportPending.value = false
    }
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}

// Get latest page progress from reports
const latestPageProgress = computed(() => {
  if (readingReports.value.length === 0) return 0
  return Math.max(...readingReports.value.map(r => r.currentPage))
})

onMounted(async () => {
  // fetch reading resource detail
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<ReadingResource>>(`/reading-resources/${route.params.slug}`)
    if (response.data)
      readingResource.value = response.data
    else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    pending.value = false
  }

  // fetch latest reading report for this resource (if any)
  try {
    reportPending.value = true
    const response = await $authedFetch<PagingResult<ReadingReportData>>(`/reading-resources/reports`, {
      query: {
        readingResourceId: readingResource.value?.id || 0,
        page: 1,
        pageSize: 100
      }
    })
    if (response.rows)
      readingReports.value = response.rows
    else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    reportPending.value = false
  }
})
</script>

<template>
  <!-- TODO: Adjust spacing on the page -->
  <div class="flex flex-col items-center justify-center gap-4 h-full">
    <div
      v-if="pending"
      class="flex items-center justify-center h-full"
    >
      <UIcon
        name="i-heroicons-arrow-path"
        class="animate-spin text-4xl"
      />
    </div>

    <template v-else-if="readingResource">
      <UContainer
        class="flex flex-col gap-y-4"
        :ui="{ root: 'px-0 mx-[-30px]' }"
      >
        <!-- TODO: Style below card to follow figma design -->
        <UCard
          class="overflow-visible mb-[100px]"
          :ui="{
            header: ' border-0',
            root: 'bg-[#265FC4] mx-[-16px] sm:mx-[-24px] lg:mx-[-32px] rounded-t-none rounded-b-3xl px-4 py-4'
          }"
        >
          <template #header>
            <div class="flex flex-row items-start justify-between gap-4">
              <div class="text-white pt-2">
                <UIcon
                  name="i-heroicons-chevron-left"
                  class="size-6"
                  @click="useRouter().back()"
                />
              </div>
              <UPageHeader
                :title="readingResource?.title"
                :ui="{
                  root: 'py-0 border-0 pb-16',
                  title:
                    'text-white  line-clamp-3 self-start max-w-[90%] text-[32px] leading-tight'
                }"
                class="flex-1 border-0"
              />
            </div>
          </template>

          <template #footer>
            <div
              class="flex flex-row justify-between relative overflow-visible border-0 ring-0 text-white"
            >
              <img
                :src="readingResource?.coverImageUri"
                :alt="`${readingResource?.title} Cover`"
                class="h-48 md:h-60 aspect-2/3 rounded-2xl absolute -top-12"
              >
              <div />
              <p class="text-right leading-tight font-medium text-[20px]">
                {{ readingResource?.page }}
                <br>
                Total Pages
              </p>
            </div>
          </template>
        </UCard>
        <!-- TODO: Style below infographic to follow figma design -->
        <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 my-[15px]">
          <UCard>
            <div class="flex flex-row gap-x-2">
              <nuxt-icon
                name="users"
                class="text-[38px]"
              />
              <div class="flex flex-col items-start">
                <p class="font-semibold">
                  Author
                </p>
                <p class="text-sm text-gray-500">
                  {{ readingResource?.authors }}
                </p>
              </div>
            </div>
          </UCard>

          <UCard>
            <div class="flex flex-row gap-x-2">
              <nuxt-icon
                name="reading"
                class="text-[38px]"
              />
              <div class="flex flex-col items-start">
                <p class="font-semibold">
                  Publish Year
                </p>
                <p class="text-sm text-gray-500">
                  {{ readingResource?.publishYear }}
                </p>
              </div>
            </div>
          </UCard>

          <UCard>
            <div class="flex flex-row gap-x-2">
              <nuxt-icon
                name="reading"
                class="text-[38px]"
              />
              <div class="flex flex-col items-start">
                <p class="font-semibold">
                  ISBN
                </p>
                <p class="text-sm text-gray-500">
                  {{ readingResource?.isbn }}
                </p>
              </div>
            </div>
          </UCard>

          <UCard>
            <div class="flex flex-row gap-x-2">
              <nuxt-icon
                name="reading"
                class="text-[38px]"
              />
              <div class="flex flex-col items-start">
                <p class="font-semibold">
                  Category
                </p>
                <p class="text-sm text-gray-500">
                  {{ readingResource?.readingCategory }}
                </p>
              </div>
            </div>
          </UCard>
        </div>

        <!-- TODO: Style the report reading session button -->
        <UButton
          class="w-full flex justify-center py-4 bg-[#265FC4]"
          @click="form?.open()"
        >
          <nuxt-icon
            name="reading"
            class="text-2xl"
          />
          Report Reading Session
        </UButton>

        <div
          v-if="reportPending"
          class="flex items-center justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <ReadingReportList
          v-else
          :reports="readingReports"
          :resource-title="readingResource?.title"
          :total-page="readingResource?.page"
        />

        <ReadingReportForm
          ref="form"
          :loading="formLoading"
          :reading-resource-id="readingResource?.id || 0"
          :latest-page-progress="latestPageProgress"
          :max-page="readingResource?.page || 0"
          @submit="onSubmit"
        />
      </UContainer>
    </template>
  </div>
</template>
