<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import GroupsTable from '~/components/groups/GroupsTable.vue'
import GroupForm from '~/components/groups/GroupForm.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

export interface Group {
  id: number
  name: string
  description?: string
  memberCount?: number
}

const form = useTemplateRef<typeof GroupForm>('form')
const formLoading = ref(false)
const table = useTemplateRef<typeof GroupsTable>('table')
const toast = useToast()

const uploadAModalOpen = ref(false)
const uploadAFile = ref<File | null>(null)
const uploadALoading = ref(false)

async function onSubmit(param: {
  action: 'Create' | 'Update'
  data: { name: string, description?: string }
  id: number | null
}) {
  try {
    formLoading.value = true
    if (param.action === 'Create') {
      await $authedFetch('/groups', {
        method: 'POST',
        body: {
          name: param.data.name,
          description: param.data.description || null
        }
      })
      table.value?.refresh()
      toast.add({
        title: 'Group created successfully',
        color: 'success'
      })
    } else if (param.action === 'Update' && param.id !== null) {
      await $authedFetch('/groups/' + param.id, {
        method: 'PUT',
        body: {
          name: param.data.name
        }
      })
      toast.add({
        title: 'Group updated successfully',
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

function onUpdate(group: Group) {
  form.value?.update({
    id: group.id,
    name: group.name,
    description: group.description
  })
}

function onDelete(group: Group) {
  const dialog = useDialog()
  dialog.confirm({
    title: 'Delete Group',
    message: 'Are you sure you want to delete this group?',
    subTitle: `This action cannot be undone. Group: "${group.name}"`,
    async onOk() {
      try {
        await $authedFetch('/groups/' + group.id, {
          method: 'DELETE'
        })
        toast.add({
          title: 'Group deleted successfully',
          color: 'success'
        })
        table.value?.refresh()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}

function handleUploadAFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files?.[0]) {
    uploadAFile.value = target.files[0]
  }
}

async function uploadTemplateA() {
  if (!uploadAFile.value) return
  try {
    uploadALoading.value = true
    const formData = new FormData()
    formData.append('file', uploadAFile.value)
    const response = await $authedFetch<{ data: { warnings: string[] } }>('/groups/members/upload', {
      method: 'POST',
      body: formData
    })
    if (response?.data?.warnings?.length) {
      toast.add({
        title: 'Upload completed with warnings',
        description: response.data.warnings.join(', '),
        color: 'warning'
      })
    } else {
      toast.add({
        title: 'Members uploaded successfully',
        color: 'success'
      })
    }
    uploadAModalOpen.value = false
    uploadAFile.value = null
    table.value?.refresh()
  } catch (error) {
    handleResponseError(error)
  } finally {
    uploadALoading.value = false
  }
}

async function downloadTemplateA() {
  try {
    const response = await $authedFetch('/groups/templates/master', {
      responseType: 'blob'
    })

    // Create download link
    const url = window.URL.createObjectURL(new Blob([response as Blob]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', 'group-template.xlsx')
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)

    toast.add({
      title: 'Template downloaded',
      color: 'success'
    })
  } catch (error) {
    handleResponseError(error)
  }
}
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Groups" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Groups
        </h1>
        <div class="flex gap-2 mb-4">
          <UButton
            icon="i-lucide-plus"
            color="neutral"
            variant="subtle"
            label="Create New Group"
            @click="form?.create()"
          />
          <UButton
            icon="i-lucide-download"
            color="neutral"
            variant="subtle"
            label="Groups Template"
            @click="downloadTemplateA"
          />
          <UButton
            icon="i-lucide-upload"
            color="neutral"
            variant="subtle"
            label="Groups"
            @click="uploadAModalOpen = true"
          />
        </div>
        <GroupsTable
          ref="table"
          @delete="onDelete"
          @edit="onUpdate"
        />
        <GroupForm
          ref="form"
          :loading="formLoading"
          @submit="onSubmit"
        />

        <!-- Template A Upload Modal -->
        <UModal
          v-model:open="uploadAModalOpen"
          title="Upload Members with Group"
          description="Upload an Excel file with GroupName and NIM columns"
          :close="{
            variant: 'subtle'
          }"
        >
          <template #body>
            <div class="space-y-4">
              <p class="text-sm text-muted">
                The Excel file should have two columns: <strong>GroupName</strong> and <strong>NIM</strong>.
                Groups will be created automatically if they don't exist. Existing group members will be fully replaced.
              </p>
              <UFormField
                label="Excel File"
                required
              >
                <input
                  type="file"
                  accept=".xlsx,.xls"
                  class="block w-full text-sm"
                  @change="handleUploadAFileChange"
                >
              </UFormField>
              <div class="flex justify-end gap-2">
                <UButton
                  color="neutral"
                  variant="subtle"
                  @click="uploadAModalOpen = false"
                >
                  Cancel
                </UButton>
                <UButton
                  :disabled="!uploadAFile"
                  :loading="uploadALoading"
                  @click="uploadTemplateA"
                >
                  Upload
                </UButton>
              </div>
            </div>
          </template>
        </UModal>
      </div>
    </template>
  </UDashboardPanel>
</template>
