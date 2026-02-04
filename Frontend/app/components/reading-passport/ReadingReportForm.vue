<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

const props = defineProps<{
  loading?: boolean
  readingResourceId: number
  latestPageProgress: number
  maxPage: number
}>()

const schema = z.object({
  currentPage: z.coerce.number().min(props.latestPageProgress, `Current page must be at least ${props.latestPageProgress}`).max(props.maxPage, `Current page cannot exceed ${props.maxPage}`),
  insight: z.string().min(1000, 'Insight should be at least 1000 characters long').max(5000, 'Insight cannot exceed 1000 characters'),
  timeSpent: z.coerce.number().min(1, 'Time spent must be at least 1 minute')
})

export type ReadingReportSchema = z.output<typeof schema>

const isOpen = ref(false)

const state = reactive({
  currentPage: props.latestPageProgress,
  insight: '',
  timeSpent: 0
})

const emit = defineEmits<{
  (
    e: 'submit',
    data: ReadingReportSchema & { readingResourceId: number }
  ): void
}>()

// Generate page options from latestPageProgress to maxPage
const pageOptions = computed(() => {
  const options = []
  for (let page = props.latestPageProgress + 1; page <= props.maxPage; page++) {
    const percentage = ((page / props.maxPage) * 100).toFixed(1)
    options.push({
      value: page,
      label: `Page ${page} (${percentage}%)`
    })
  }
  console.log(options)
  return options
})

function setState(data: Partial<ReadingReportSchema>) {
  if (data.currentPage !== undefined) state.currentPage = data.currentPage
  if (data.insight !== undefined) state.insight = data.insight
  if (data.timeSpent !== undefined) state.timeSpent = data.timeSpent
}

function resetState() {
  state.currentPage = props.latestPageProgress
  state.insight = ''
  state.timeSpent = 0
}

function open() {
  resetState()
  isOpen.value = true
}

function close() {
  isOpen.value = false
}

defineExpose({
  setState,
  resetState,
  open,
  close
})

async function onSubmit(event: FormSubmitEvent<ReadingReportSchema>) {
  emit('submit', {
    ...event.data,
    readingResourceId: props.readingResourceId
  })
}
</script>

<template>
  <UModal
    v-model:open="isOpen"
    :dismissible="false"
    title="Report Reading Session"
    description="Share your progress and give us what insight you get from the reading session"
    :close="{
      variant: 'subtle'
    }"
  >
    <template #body>
      <UForm
        :schema="schema"
        :state="state"
        class="space-y-6"
        @submit="onSubmit"
      >
        <!-- Current Page field - full width -->
        <UFormField
          label="Current Page"
          name="currentPage"
          required
        >
          <USelectMenu
            :model-value="pageOptions.find(opt => opt.value === state.currentPage)"
            :items="pageOptions"
            size="lg"
            class="w-full"
            placeholder="Select current page"
            @update:model-value="(selected) => state.currentPage = selected.value"
          />
        </UFormField>

        <!-- Time Spent field -->
        <UFormField
          label="Time Spent (minutes)"
          name="timeSpentInMinutes"
          required
        >
          <UInput
            v-model="state.timeSpent"
            type="number"
            placeholder="Enter time spent reading"
            size="lg"
            class="w-full"
            min="1"
          />
        </UFormField>

        <!-- Insight field - full width textarea -->
        <UFormField
          label="Insight"
          name="insight"
          required
        >
          <UTextarea
            v-model="state.insight"
            placeholder="Share your insights from this reading session..."
            size="lg"
            class="w-full"
            :rows="6"
          />
          <p class="text-muted">
            {{ state.insight.length }} / 1000 characters
          </p>
        </UFormField>

        <!-- Submit button -->
        <div class="flex justify-end pt-4">
          <UButton
            type="submit"
            size="lg"
            class="px-8 w-full text-center flex justify-center cursor-pointer"
            :loading
          >
            Save Reading Report
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>
