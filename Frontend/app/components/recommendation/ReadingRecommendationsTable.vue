<script setup lang="ts">
import { h, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import type { ReadingRecommendation } from '~/pages/admin/recommendation.vue'
import { usePaging } from '~/apis/paging'

const UButton = resolveComponent('UButton')

const paging = usePaging<ReadingRecommendation>('/reading-recommendations')

onMounted(() => {
  paging.fetch()
})

defineExpose({
  refresh: paging.fetch
})

const emit = defineEmits(['edit', 'delete'])

const columns: TableColumn<ReadingRecommendation>[] = [
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
            'aria-label': 'Edit recommendation',
            'onClick': () => emit('edit', row.original)
          }),
          h(UButton, {
            'icon': 'i-lucide-trash',
            'color': 'error',
            'variant': 'ghost',
            'size': 'sm',
            'aria-label': 'Delete recommendation',
            'onClick': () => emit('delete', row.original)
          })
        ]
      )
    }
  },
  {
    id: 'coverImageUri',
    accessorKey: 'coverImageUri',
    header: 'Cover',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    },
    cell: ({ row }) => {
      return h('img', {
        src: row.original.coverImageUri,
        alt: row.original.title,
        class: 'w-12 object-cover rounded'
      })
    }
  },
  {
    id: 'title',
    accessorKey: 'title',
    header: 'Title'
  },
  {
    id: 'isbn',
    accessorKey: 'isbn',
    header: 'ISBN',
    meta: {
      class: {
        th: 'w-[120px]'
      }
    }
  },
  {
    id: 'authors',
    accessorKey: 'authors',
    header: 'Authors'
  },
  {
    id: 'readingCategory',
    accessorKey: 'readingCategory',
    header: 'Category'
  },
  {
    id: 'publishYear',
    accessorKey: 'publishYear',
    header: 'Year',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    }
  },
  {
    id: 'page',
    accessorKey: 'page',
    header: 'Pages',
    meta: {
      class: {
        th: 'w-[80px]'
      }
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
    id: 'resourceLink',
    accessorKey: 'resourceLink',
    header: 'Link',
    meta: {
      class: {
        th: 'w-[80px]'
      }
    },
    cell: ({ row }) => {
      if (!row.original.resourceLink) return null
      return h('a', {
        href: row.original.resourceLink,
        target: '_blank',
        rel: 'noopener noreferrer',
        class: 'text-blue-600 hover:text-blue-800 underline text-sm'
      }, 'View')
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
