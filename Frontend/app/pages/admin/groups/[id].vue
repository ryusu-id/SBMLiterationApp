<script setup lang="ts">
import { h } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { $authedFetch, handleResponseError } from '~/apis/api'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  middleware: ['auth', 'admin-only']
})

interface GroupMemberUser {
  id: number
  name: string
  nim: string
}

interface GroupMember {
  id: number
  userId: number
  user: GroupMemberUser
}

interface GroupDetail {
  id: number
  name: string
  description?: string
  members: GroupMember[]
}

const route = useRoute()
const groupId = computed(() => Number(route.params.id))

const group = ref<GroupDetail | null>(null)
const loading = ref(false)
const toast = useToast()

const renameOpen = ref(false)
const renameName = ref('')
const renameLoading = ref(false)

const uploadOpen = ref(false)
const uploadFile = ref<File | null>(null)
const uploadLoading = ref(false)

async function fetchGroup() {
  loading.value = true
  try {
    const response = await $authedFetch<{ data: GroupDetail }>(`/groups/${groupId.value}`)
    group.value = response?.data || null
  } catch (error) {
    handleResponseError(error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchGroup()
})

function openRename() {
  renameName.value = group.value?.name || ''
  renameOpen.value = true
}

async function submitRename() {
  if (!renameName.value.trim()) return
  try {
    renameLoading.value = true
    await $authedFetch(`/groups/${groupId.value}`, {
      method: 'PUT',
      body: { name: renameName.value }
    })
    toast.add({ title: 'Group renamed successfully', color: 'success' })
    renameOpen.value = false
    fetchGroup()
  } catch (error) {
    handleResponseError(error)
  } finally {
    renameLoading.value = false
  }
}

function handleFileChange(event: Event) {
  const target = event.target as HTMLInputElement
  if (target.files?.[0]) {
    uploadFile.value = target.files[0]
  }
}

async function uploadMembers() {
  if (!uploadFile.value) return
  try {
    uploadLoading.value = true
    const formData = new FormData()
    formData.append('file', uploadFile.value)
    const response = await $authedFetch<{ data: { warnings: string[] } }>(`/groups/${groupId.value}/members/upload`, {
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
      toast.add({ title: 'Members uploaded successfully', color: 'success' })
    }
    uploadOpen.value = false
    uploadFile.value = null
    fetchGroup()
  } catch (error) {
    handleResponseError(error)
  } finally {
    uploadLoading.value = false
  }
}

const memberColumns: TableColumn<GroupMember>[] = [
  {
    id: 'userId',
    accessorKey: 'userId',
    header: 'User ID',
    meta: { class: { th: 'w-[80px]' } }
  },
  {
    id: 'name',
    header: 'Name',
    cell: ({ row }) => row.original.user?.name || h('span', { class: 'text-muted' }, '-')
  },
  {
    id: 'nim',
    header: 'NIM',
    cell: ({ row }) => row.original.user?.nim || h('span', { class: 'text-muted' }, '-')
  }
]
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar :title="group?.name || 'Group Detail'" />
    </template>

    <template #body>
      <div>
        <div class="flex items-center gap-2 mb-6">
          <UButton
            icon="i-lucide-arrow-left"
            color="neutral"
            variant="ghost"
            to="/admin/groups"
          />
          <h1 class="text-2xl font-bold">
            {{ group?.name || 'Loading...' }}
          </h1>
        </div>

        <div
          v-if="loading"
          class="flex justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <div
          v-else-if="group"
          class="space-y-6"
        >
          <UCard>
            <template #header>
              <div class="flex items-center justify-between">
                <h2 class="text-lg font-semibold">
                  Group Info
                </h2>
                <UButton
                  icon="i-lucide-pencil"
                  color="neutral"
                  variant="subtle"
                  size="sm"
                  label="Rename"
                  @click="openRename"
                />
              </div>
            </template>

            <div class="space-y-3">
              <div>
                <p class="text-sm text-muted">
                  Name
                </p>
                <p class="font-medium">
                  {{ group.name }}
                </p>
              </div>
              <div v-if="group.description">
                <p class="text-sm text-muted">
                  Description
                </p>
                <p>{{ group.description }}</p>
              </div>
              <div>
                <p class="text-sm text-muted">
                  Total Members
                </p>
                <p class="font-medium">
                  {{ group.members?.length || 0 }}
                </p>
              </div>
            </div>
          </UCard>

          <UCard>
            <template #header>
              <div class="flex items-center justify-between">
                <h2 class="text-lg font-semibold">
                  Members ({{ group.members?.length || 0 }})
                </h2>
                <UButton
                  icon="i-lucide-upload"
                  color="neutral"
                  variant="subtle"
                  size="sm"
                  label="Upload Members"
                  @click="uploadOpen = true"
                />
              </div>
            </template>
            <UTable
              :data="group.members || []"
              :columns="memberColumns"
              class="w-full"
            />
          </UCard>
        </div>

        <!-- Rename Modal -->
        <UModal
          v-model:open="renameOpen"
          title="Rename Group"
          description="Enter a new name for this group"
          :close="{
            variant: 'subtle'
          }"
        >
          <template #body>
            <div class="space-y-4">
              <UFormField
                label="Name"
                required
              >
                <UInput
                  v-model="renameName"
                  placeholder="Enter group name"
                  class="w-full"
                  maxlength="100"
                />
              </UFormField>
              <div class="flex justify-end gap-2">
                <UButton
                  color="neutral"
                  variant="subtle"
                  @click="renameOpen = false"
                >
                  Cancel
                </UButton>
                <UButton
                  :loading="renameLoading"
                  :disabled="!renameName.trim()"
                  @click="submitRename"
                >
                  Save
                </UButton>
              </div>
            </div>
          </template>
        </UModal>

        <!-- Upload Members Modal -->
        <UModal
          v-model:open="uploadOpen"
          title="Upload Members"
          description="Upload an Excel file with NIM column to replace existing members"
          :close="{
            variant: 'subtle'
          }"
        >
          <template #body>
            <div class="space-y-4">
              <p class="text-sm text-muted">
                The Excel file should have one column: <strong>NIM</strong>.
                All existing members of this group will be replaced.
              </p>
              <UFormField
                label="Excel File"
                required
              >
                <input
                  type="file"
                  accept=".xlsx,.xls"
                  class="block w-full text-sm"
                  @change="handleFileChange"
                >
              </UFormField>
              <div class="flex justify-end gap-2">
                <UButton
                  color="neutral"
                  variant="subtle"
                  @click="uploadOpen = false"
                >
                  Cancel
                </UButton>
                <UButton
                  :disabled="!uploadFile"
                  :loading="uploadLoading"
                  @click="uploadMembers"
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
