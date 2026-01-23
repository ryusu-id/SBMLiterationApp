<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import ReadingCategoriesTable from '~/components/categories/ReadingCategoriesTable.vue'
import ReadingCategoryForm from '~/components/categories/ReadingCategoryForm.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin'
})

export interface ReadingCategory {
  id: string
  categoryName: string
}

const form = useTemplateRef<typeof ReadingCategoryForm>('form')
const formLoading = ref(false)
const table = useTemplateRef<typeof ReadingCategoriesTable>('table')
const toast = useToast()

async function onSubmit(param: { action: 'Create' | 'Update', data: { categoryName: string }, id: number | null }) {
  try {
    formLoading.value = true
    if (param.action === 'Create') {
      await $authedFetch('/reading-categories', {
        method: 'POST',
        body: {
          categoryName: param.data.categoryName
        }
      })

      table.value?.refresh()
      toast.add({
        title: 'Category created successfully',
        color: 'success'
      })
    } else if (param.action === 'Update' && param.id !== null) {
      await $authedFetch('/reading-categories/' + param.id, {
        method: 'PUT',
        body: {
          categoryName: param.data.categoryName
        }
      })

      toast.add({
        title: 'Category updated successfully',
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

function onUpdate(category: ReadingCategory) {
  form.value?.update({
    id: Number(category.id),
    categoryName: category.categoryName
  })
}

function onDelete(category: ReadingCategory) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Delete Reading Category',
    message: 'Are you sure you want to delete this category?',
    subTitle: `This action cannot be undone. Category: "${category.categoryName}"`,
    async onOk() {
      try {
        await $authedFetch('/reading-categories/' + category.id, {
          method: 'DELETE'
        })
        toast.add({
          title: 'Category deleted successfully',
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
      <DashboardNavbar title="Reading Categories" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Reading Categories
        </h1>
        <div>
          <UButton
            icon="i-lucide-plus"
            color="neutral"
            class="mb-4"
            label="Create New Category"
            variant="subtle"
            @click="form?.create()"
          />
        </div>
        <ReadingCategoriesTable
          ref="table"
          @delete="onDelete"
          @edit="onUpdate"
        />
        <ReadingCategoryForm
          ref="form"
          :loading="formLoading"
          @submit="onSubmit"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
