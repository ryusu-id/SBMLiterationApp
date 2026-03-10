<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import AssignmentsTable from '~/components/assignments/AssignmentsTable.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

export interface Assignment {
  id: number
  title: string
  description?: string
  dueDate?: string
}

const table = useTemplateRef<typeof AssignmentsTable>('table')
const toast = useToast()
const router = useRouter()

function onUpdate(assignment: Assignment) {
  router.push(`/admin/assignments/edit/${assignment.id}`)
}

function onDelete(assignment: Assignment) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Delete Assignment',
    message: 'Are you sure you want to delete this assignment?',
    subTitle: `This action cannot be undone. Assignment: "${assignment.title}"`,
    async onOk() {
      try {
        await $authedFetch('/assignments/' + assignment.id, {
          method: 'DELETE'
        })
        toast.add({
          title: 'Assignment deleted successfully',
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
      <DashboardNavbar title="Assignments" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Assignments
        </h1>
        <div>
          <UButton
            icon="i-lucide-plus"
            color="neutral"
            class="mb-4"
            label="Create New Assignment"
            variant="subtle"
            to="/admin/assignments/create"
          />
        </div>
        <AssignmentsTable
          ref="table"
          @delete="onDelete"
          @edit="onUpdate"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
