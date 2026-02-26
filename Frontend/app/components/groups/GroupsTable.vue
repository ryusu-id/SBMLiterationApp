<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import type { Group } from '~/pages/admin/groups/index.vue'
import { usePaging } from '~/apis/paging'

const UButton = resolveComponent('UButton')
const router = useRouter()

const paging = usePaging<Group>('/groups')

onMounted(() => {
  paging.fetch()
})

defineExpose({
  refresh: paging.fetch
})

const emit = defineEmits(['edit', 'delete'])

const columns: TableColumn<Group>[] = [
  {
    id: 'actions',
    meta: {
      class: {
        th: 'w-[120px]'
      }
    },
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex justify-start gap-1' },
        [
          h(UButton, {
            'icon': 'i-lucide-eye',
            'color': 'primary',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'View group',
            'onClick': () => router.push(`/admin/groups/${row.original.id}`)
          }),
          h(UButton, {
            'icon': 'i-lucide-pencil',
            'color': 'neutral',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Edit group',
            'onClick': () => emit('edit', row.original)
          }),
          h(UButton, {
            'icon': 'i-lucide-trash',
            'color': 'error',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Delete group',
            'onClick': () => emit('delete', row.original)
          })
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
    id: 'name',
    accessorKey: 'name',
    header: 'Name'
  },
  {
    id: 'description',
    accessorKey: 'description',
    header: 'Description',
    cell: ({ row }) => row.original.description || h('span', { class: 'text-muted' }, '-')
  },
  {
    id: 'memberCount',
    accessorKey: 'memberCount',
    header: 'Members',
    meta: {
      class: {
        th: 'w-[100px]'
      }
    },
    cell: ({ row }) => row.original.memberCount ?? '-'
  }
]
</script>

<template>
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
</template>
