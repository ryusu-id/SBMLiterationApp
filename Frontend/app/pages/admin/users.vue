<script setup lang="ts">
import { $authedFetch, handleResponseError } from '~/apis/api'
import UsersTable from '~/components/users/UsersTable.vue'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin'
})

export interface User {
  id: string
  email: string
  name: string
  roles?: string[]
  isActive: boolean
}

const table = useTemplateRef<typeof UsersTable>('table')
const toast = useToast()
const dialog = useDialog()

async function onAssignAdmin(user: User) {
  await dialog.confirm({
    title: 'Assign Admin Role',
    message: `Are you sure you want to assign admin role to this user?`,
    subTitle: `User: ${user.name} (${user.email})`,
    async onOk() {
      try {
        await $authedFetch(`/users/${user.id}/assign`, {
          method: 'GET'
        })
        toast.add({
          title: 'Admin role assigned successfully',
          color: 'success'
        })
        table.value?.refresh()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}

async function onUnassignAdmin(user: User) {
  await dialog.confirm({
    title: 'Unassign Admin Role',
    message: `Are you sure you want to remove admin role from this user?`,
    subTitle: `User: ${user.name} (${user.email})`,
    async onOk() {
      try {
        await $authedFetch(`/users/${user.id}/unassign`, {
          method: 'GET'
        })
        toast.add({
          title: 'Admin role unassigned successfully',
          color: 'success'
        })
        table.value?.refresh()
      } catch (error) {
        handleResponseError(error)
      }
    }
  })
}

async function onDisable(user: User) {
  await dialog.confirm({
    title: 'Disable User',
    message: `Are you sure you want to disable this user? They will no longer be able to access the system.`,
    subTitle: `User: ${user.name} (${user.email})`,
    async onOk() {
      try {
        await $authedFetch(`/users/${user.id}/disable`, {
          method: 'GET'
        })
        toast.add({
          title: 'User disabled successfully',
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
      <DashboardNavbar title="User Management" />
    </template>

    <template #body>
      <div>
        <h1 class="text-2xl font-bold mb-4">
          Users
        </h1>
        <UsersTable
          ref="table"
          @assign-admin="onAssignAdmin"
          @unassign-admin="onUnassignAdmin"
          @disable="onDisable"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
