<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import { usePaging } from '~/apis/paging'

const UButton = resolveComponent('UButton')
const UBadge = resolveComponent('UBadge')

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

const paging = usePaging<DailyRead>('/daily-reads')

onMounted(() => {
  paging.fetch()
})

defineExpose({
  refresh: paging.fetch
})

const emit = defineEmits(['edit', 'delete'])

const columns: TableColumn<DailyRead>[] = [
  {
    id: 'actions',
    meta: {
      class: {
        th: 'w-[100px]'
      }
    },
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex justify-start gap-1' },
        [
          h(UButton, {
            'icon': 'i-lucide-pencil',
            'color': 'primary',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Edit daily read',
            'onClick': () => emit('edit', row.original)
          }),
          h(UButton, {
            'icon': 'i-lucide-trash',
            'color': 'error',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Delete daily read',
            'onClick': () => emit('delete', row.original)
          })
        ]
      )
    }
  },
  {
    id: 'coverImg',
    accessorKey: 'coverImg',
    header: 'Cover',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    },
    cell: ({ row }) => {
      if (!row.original.coverImg) return null
      return h('img', {
        src: row.original.coverImg,
        alt: row.original.title,
        class: 'w-12 h-12 object-cover rounded'
      })
    }
  },
  {
    id: 'title',
    accessorKey: 'title',
    header: 'Title'
  },
  {
    id: 'date',
    accessorKey: 'date',
    header: 'Date',
    meta: {
      class: {
        th: 'w-[120px]'
      }
    },
    cell: ({ row }) => {
      return new Date(row.original.date).toLocaleDateString()
    }
  },
  {
    id: 'category',
    accessorKey: 'category',
    header: 'Category',
    meta: {
      class: {
        th: 'w-[120px]'
      }
    },
    cell: ({ row }) => {
      if (!row.original.category) {
        return h('span', { class: 'text-gray-400' }, '—')
      }
      return h(UBadge, {
        color: 'neutral',
        variant: 'subtle'
      }, () => row.original.category)
    }
  },
  {
    id: 'exp',
    accessorKey: 'exp',
    header: 'EXP',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    }
  },
  {
    id: 'minimalCorrectAnswer',
    accessorKey: 'minimalCorrectAnswer',
    header: 'Min. Correct',
    meta: {
      class: {
        th: 'w-[100px]'
      }
    }
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
