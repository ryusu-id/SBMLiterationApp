<script setup lang="ts">
import type { PagingResult } from '~/apis/paging'

interface ReadingCategory {
  id: number
  categoryName: string
}

interface Props {
  modelValue?: string
  required?: boolean
  label?: string
  name?: string
  placeholder?: string
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  required: false,
  label: 'Category',
  name: 'category',
  placeholder: 'Select category',
  size: 'md'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
}>()

const selectedCategory = ref('')

const { $useAuthedFetch } = useNuxtApp()
const { data: categoriesData, pending: categoriesLoading, execute } = $useAuthedFetch<PagingResult<ReadingCategory>>('/reading-categories', {
  query: {
    page: 1,
    rowsPerPage: 1000
  },
  lazy: true,
  immediate: false
})

onMounted(async () => {
  await execute()
})

const categories = computed(() => categoriesData.value?.rows || [])

const categoryOptions = computed(() =>
  categories.value.map(cat => cat.categoryName)
)

watch(() => props.modelValue, (val) => {
  selectedCategory.value = val || ''
}, { immediate: true })

watch(() => selectedCategory.value, (val) => {
  emit('update:modelValue', val)
})

const open = ref(false)
function onCreate(newCategory: string) {
  selectedCategory.value = newCategory
  open.value = false
}
</script>

<template>
  <div class="space-y-4">
    <UFormField
      :label="label"
      :required="required"
    >
      <USelectMenu
        v-model:open="open"
        v-model="selectedCategory"
        virtualize
        create-item="always"
        clear
        :search-input="{
          placeholder: 'Search or type to create...'
        }"
        :items="categoryOptions"
        :loading="categoriesLoading"
        :placeholder="placeholder"
        :size="size"
        class="w-full"
        @create="onCreate"
      />
    </UFormField>
  </div>
</template>
