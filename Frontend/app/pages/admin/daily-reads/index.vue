<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import DailyReadsTable from '~/components/daily-reads/DailyReadsTable.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

export interface DailyRead {
  id: string
  title: string
  coverImg?: string
  content: string
  date: string
  category?: string
  exp: number
  minimalCorrectAnswer: number
}

const table = useTemplateRef<typeof DailyReadsTable>('table')
const toast = useToast()
const router = useRouter()

function onEdit(dailyRead: DailyRead) {
  router.push(`/admin/daily-reads/edit/${dailyRead.id}`)
}

function onDelete(dailyRead: DailyRead) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Delete Daily Read',
    message: 'Are you sure you want to delete this daily read?',
    subTitle: `This action cannot be undone. Title: "${dailyRead.title}"`,
    async onOk() {
      try {
        await $authedFetch('/daily-reads/' + dailyRead.id, {
          method: 'DELETE'
        })
        toast.add({
          title: 'Daily read deleted successfully',
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
      <DashboardNavbar title="Daily Reads" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Daily Reads Management
        </h1>
        <div>
          <UButton
            icon="i-lucide-plus"
            color="neutral"
            class="mb-4"
            label="Create New Daily Read"
            variant="subtle"
            to="/admin/daily-reads/create"
          />
        </div>
        <DailyReadsTable
          ref="table"
          @delete="onDelete"
          @edit="onEdit"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
