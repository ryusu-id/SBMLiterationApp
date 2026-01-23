<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import type { ReadingCategory } from '~/pages/admin/categories.vue'
import { usePaging } from '~/apis/paging'

const UButton = resolveComponent('UButton')

const paging = usePaging<ReadingCategory>('/reading-categories')

onMounted(() => {
  paging.fetch()
})

defineExpose({
  refresh: paging.fetch
})

const emit = defineEmits(['edit', 'delete'])

const columns: TableColumn<ReadingCategory>[] = [
  {
    id: 'actions',
    meta: {
      class: {
        th: 'w-[20px]'
      }
    },
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex justify-start' },
        [
          h(UButton, {
            'icon': 'i-lucide-pencil',
            'color': 'primary',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Edit category',
            'onClick': () => emit('edit', row.original)
          }),
          h(UButton, {
            'icon': 'i-lucide-trash',
            'color': 'error',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Delete category',
            'onClick': () => emit('delete', row.original)
          })
        ]
      )
    }
  },
  {
    id: 'id',
    accessorKey: 'id',
    header: 'Id',
    meta: {
      class: {
        th: 'w-[50px]'
      }
    }
  },
  {
    id: 'categoryName',
    accessorKey: 'categoryName',
    header: 'Category Name'
  }
]
</script>

<template>
  <UTable
    sticky
    :data="paging.rows.value"
    :columns="columns"
    class="flex-1"
    :loading="paging.loading.value"
  />
</template>
