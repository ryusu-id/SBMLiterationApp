<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import type { ReadingResource } from './ReadingResources.vue'
import type { DropdownMenuItem } from '@nuxt/ui'

const props = defineProps<{
  resource: ReadingResource
  journal?: boolean
}>()

const handleImageError = (event: Event) => {
  const target = event.target as HTMLImageElement
  if (target) {
    target.style.display = 'none'
  }
}

const router = useRouter()
const handleEdit = () => {
  router.push(props.journal
    ? {
        name: 'UpdateReadingJournal',
        params: { slug: props.resource.id }
      }
    : {
        name: 'UpdateReadingBook',
        params: { slug: props.resource.id }
      })
}

const dialog = useDialog()
const toast = useToast()
const emit = defineEmits(['refresh'])
const handleDelete = () => {
  dialog.confirm({
    title: 'Delete Resource',
    subTitle: 'This action cannot be undone.',
    message: 'Are you sure you want to delete this reading?',
    onOk: async () => {
      try {
        const result = await $authedFetch<ApiResponse>(`/reading-resources/${props.resource.id}`, {
          method: 'DELETE'
        })

        if (result.errorDescription)
          handleResponseError(result)
        else {
          toast.add({
            title: 'Resource Deleted',
            description: 'The reading resource has been deleted successfully.',
            color: 'success'
          })

          emit('refresh')
        }
      } catch (err) {
        handleResponseError(err)
      }
    }

  })
}

const items: DropdownMenuItem[][] = [
  [
    {
      label: 'Edit',
      icon: 'i-lucide-edit',
      onSelect: handleEdit
    },
    {
      label: 'Delete',
      icon: 'i-lucide-trash',
      onSelect: handleDelete
    }
  ]
]

function goToReportPage() {
  useRouter().push({
    name: 'ReadingReport',
    params: { slug: props.resource.id }
  })
}
</script>

<template>
  <UCard
    class="bg-[#3566CD] rounded-[36px] aspect-[2/3] py-4 px-3 flex flex-col justify-between"
    :ui="{
      root: '',
      header: 'border-0',
      body: 'border-0'
    }"
  >
    <template #header>
      <div class="flex items-start justify-between gap-2">
        <!-- <UPageHeader
          :title="resource.title"
          :ui="{
            title: 'text-white max-h-[120px]   line-clamp-3 '
          }"
          class="flex-1 border-0 p-0"
        /> -->

        <div class="text-white max-h-[90px] text-[23px] font-bold  line-clamp-3 tracking-tight leading-tight">
          <h1>{{ resource.title }}</h1>
        </div>

        <div class="flex-1">
          <UDropdownMenu :items>
            <UButton
              class="dark"
              variant="ghost"
              :ui="{
                base: 'hover:bg-white/10'
              }"
              color="neutral"
              size="xl"
              icon="i-heroicons-ellipsis-vertical"
            />
          </UDropdownMenu>
        </div>
      </div>
    </template>

    <div
      class="w-full"
      @click="goToReportPage()"
    >
      <div
        v-if="resource.coverImageUri"
        class="w-full max-w-[150px] aspect-[3/4] overflow-hidden rounded-[12px]"
      >
        <img
          :src="resource.coverImageUri"
          alt="Resource Image"
          class="w-full h-full object-cover"
          @error="handleImageError"
        >
      </div>
    </div>

    <template #footer>
      <div class="flex flex-row justify-between text-white font-semibold">
        <div>
          {{ journal ? "Journal" : "Book" }}
        </div>
        <div class="text-[17px]">
          0/{{ resource.page }}
        </div>
      </div>
    </template>
  </UCard>
</template>
