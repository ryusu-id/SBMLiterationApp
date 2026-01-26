<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import ReadingRecommendationsTable from '~/components/recommendation/ReadingRecommendationsTable.vue'
import ReadingRecommendationForm from '~/components/recommendation/ReadingRecommendationForm.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

export interface ReadingRecommendation {
  id: string
  title: string
  isbn: string
  readingCategory: string
  authors: string
  publishYear: string
  page: number
  resourceLink?: string
  coverImageUri: string
}

const form = useTemplateRef<typeof ReadingRecommendationForm>('form')
const formLoading = ref(false)
const table = useTemplateRef<typeof ReadingRecommendationsTable>('table')
const toast = useToast()

async function onSubmit(param: {
  action: 'Create' | 'Update'
  data: {
    title: string
    isbn: string
    readingCategory: string
    authors: string
    publishYear: string
    page: number
    resourceLink?: string
  }
  id: number | null
}) {
  try {
    formLoading.value = true
    if (param.action === 'Create') {
      await $authedFetch('/reading-recommendations', {
        method: 'POST',
        body: param.data
      })

      table.value?.refresh()
      toast.add({
        title: 'Recommendation created successfully',
        color: 'success'
      })
    } else if (param.action === 'Update' && param.id !== null) {
      await $authedFetch('/reading-recommendations/' + param.id, {
        method: 'PUT',
        body: param.data
      })

      toast.add({
        title: 'Recommendation updated successfully',
        color: 'success'
      })

      table.value?.refresh()
    }
    form.value?.close()
  } catch (error) {
    handleResponseError(error)
  } finally {
    formLoading.value = false
  }
}

function onUpdate(recommendation: ReadingRecommendation) {
  form.value?.update({
    id: Number(recommendation.id),
    title: recommendation.title,
    isbn: recommendation.isbn,
    readingCategory: recommendation.readingCategory,
    authors: recommendation.authors,
    publishYear: recommendation.publishYear,
    page: recommendation.page,
    resourceLink: recommendation.resourceLink || '',
    coverImageUri: recommendation.coverImageUri
  })
}

function onDelete(recommendation: ReadingRecommendation) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Delete Reading Recommendation',
    message: 'Are you sure you want to delete this recommendation?',
    subTitle: `This action cannot be undone. Book: "${recommendation.title}"`,
    async onOk() {
      try {
        await $authedFetch('/reading-recommendations/' + recommendation.id, {
          method: 'DELETE'
        })
        toast.add({
          title: 'Recommendation deleted successfully',
          color: 'success'
        })
        table.value?.refresh()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Book Recommendation" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Reading Recommendations
        </h1>
        <div>
          <UButton
            icon="i-lucide-plus"
            color="neutral"
            class="mb-4"
            label="Create New Recommendation"
            variant="subtle"
            @click="form?.create()"
          />
        </div>
        <ReadingRecommendationsTable
          ref="table"
          @delete="onDelete"
          @edit="onUpdate"
        />
        <ReadingRecommendationForm
          ref="form"
          :loading="formLoading"
          @submit="onSubmit"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
