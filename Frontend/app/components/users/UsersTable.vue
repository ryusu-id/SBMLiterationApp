<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import type { User } from '~/pages/admin/users.vue'
import { usePaging } from '~/apis/paging'

const UButton = resolveComponent('UButton')
const UTooltip = resolveComponent('UTooltip')

const searchQuery = ref('')
const paging = usePaging<User>('/users')

onMounted(() => {
  paging.fetch()
})

function handleSearch() {
  paging.search(searchQuery.value)
}

function clearSearch() {
  searchQuery.value = ''
  paging.fetch()
}

defineExpose({
  refresh: paging.fetch
})

const emit = defineEmits(['assignAdmin', 'unassignAdmin', 'disable'])

const columns: TableColumn<User>[] = [
  {
    id: 'actions',
    meta: {
      class: {
        th: 'w-[200px]'
      }
    },
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex justify-start gap-1' },
        [
          h(UTooltip, {
            text: 'Assign Admin Role',
            delayDuration: 0
          },
          () =>
            h(UButton, {
              'icon': 'i-lucide-shield-plus',
              'color': 'primary',
              'variant': 'ghost',
              'size': 'sm',
              'aria-label': 'Assign admin',
              'onClick': () => emit('assignAdmin', row.original)
            })
          ),
          h(UTooltip, {
            text: 'Unassign Admin Role',
            delayDuration: 0
          },
          () =>
            h(UButton, {
              'icon': 'i-lucide-shield-minus',
              'color': 'warning',
              'variant': 'ghost',
              'size': 'sm',
              'aria-label': 'Unassign admin',
              'onClick': () => emit('unassignAdmin', row.original)
            })),
          h(UTooltip, {
            text: 'Disable User',
            delayDuration: 0
          },
          () =>
            h(UButton, {
              'icon': 'i-lucide-user-x',
              'color': 'error',
              'variant': 'ghost',
              'size': 'sm',
              'aria-label': 'Disable user',
              'onClick': () => emit('disable', row.original)
            }))
        ]
      )
    }
  },
  {
    id: 'id',
    accessorKey: 'id',
    header: 'ID',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    }
  },
  {
    id: 'email',
    accessorKey: 'email',
    header: 'Email'
  },
  {
    id: 'name',
    accessorKey: 'name',
    header: 'Name'
  },
  {
    id: 'roles',
    accessorKey: 'roles',
    header: 'Roles',
    cell: ({ row }) => {
      const roles = row.original.roles || []
      if (roles.length === 0) return h('span', { class: 'text-gray-400' }, 'No roles')
      return h('div', { class: 'flex gap-1 flex-wrap' },
        roles.map(role =>
          h('span', {
            class: 'px-2 py-1 text-xs rounded bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200'
          }, role)
        )
      )
    }
  },
  {
    id: 'isActive',
    accessorKey: 'isActive',
    header: 'Status',
    meta: {
      class: {
        th: 'w-[100px]'
      }
    },
    cell: ({ row }) => {
      const isActive = row.original.isActive
      return h('span', {
        class: isActive
          ? 'px-2 py-1 text-xs rounded bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
          : 'px-2 py-1 text-xs rounded bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
      }, isActive ? 'Active' : 'Disabled')
    }
  }
]
</script>

<template>
  <div class="space-y-4">
    <div class="flex gap-2">
      <UInput
        v-model="searchQuery"
        placeholder="Search users by email or name..."
        class="flex-1"
        @keyup.enter="handleSearch"
      >
        <template #trailing>
          <UButton
            v-if="searchQuery"
            icon="i-lucide-x"
            variant="ghost"
            size="xs"
            @click="clearSearch"
          />
        </template>
      </UInput>
      <UButton
        icon="i-lucide-search"
        @click="handleSearch"
      >
        Search
      </UButton>
    </div>

    <div class="flex flex-col items-end gap-y-2">
      <UTable
        sticky
        :data="paging.rows.value"
        :columns="columns"
        class="w-full"
        :loading="paging.loading.value"
      />
      <UPagination
        :page="paging.page.value"
        :total="paging.totalRows.value"
        :items-per-page="paging.rowsPerPage.value"
        :show-controls="false"
        @update:page="paging.goTo($event)"
      />
    </div>
  </div>
</template>
