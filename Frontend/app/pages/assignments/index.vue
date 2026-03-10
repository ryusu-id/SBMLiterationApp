<script setup lang="ts">
import { handleResponseError } from '~/apis/api'
import type { AssignmentItem } from '~/components/assignments/AssignmentCard.vue'
import AssignmentCard from '~/components/assignments/AssignmentCard.vue'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

// Grouped shape from GET /api/assignments/my
interface MyAssignmentsResponse {
  active: AssignmentItem[]
  done: AssignmentItem[]
  missing: AssignmentItem[]
}

const useAuthedFetch = useNuxtApp().$useAuthedFetch

const {
  data: assignments,
  pending,
  error,
  execute
} = await useAuthedFetch<{ data: MyAssignmentsResponse }>('/assignments/my', {
  immediate: false,
  lazy: true
})

onMounted(() => {
  execute()
})

watch(error, (err) => {
  if (err) handleResponseError(err)
})

const hasAnyAssignment = computed(() => {
  const d = assignments.value?.data
  return (
    (d?.active?.length || 0)
    + (d?.done?.length || 0)
    + (d?.missing?.length || 0)
    > 0
  )
})
</script>

<template>
  <ClientOnly>
    <UContainer class="py-8">
      <div class="flex flex-col space-y-6">
        <UPageHeader>
          <template #title>
            <h1 class="text-3xl font-bold tracking-tighter">
              Assignments
            </h1>
          </template>
          <template #description>
            <p>Your group's assignments and submission status</p>
          </template>
        </UPageHeader>

        <div
          v-if="pending"
          class="flex justify-center py-12"
        >
          <UIcon
            name="i-heroicons-arrow-path"
            class="animate-spin text-4xl"
          />
        </div>

        <div
          v-else-if="!hasAnyAssignment"
          class="flex flex-col items-center py-12 text-muted"
        >
          <UIcon
            name="i-lucide-clipboard-list"
            class="text-5xl mb-4"
          />
          <p>No assignments yet</p>
        </div>

        <template v-else>
          <div
            v-if="assignments?.data?.active?.length"
            class="space-y-3"
          >
            <h2
              class="text-lg font-semibold tracking-tight flex items-center gap-2"
            >
              <UIcon
                name="i-lucide-clock"
                class="size-4 text-primary"
              />
              Active
            </h2>
            <div class="grid gap-4">
              <AssignmentCard
                v-for="assignment in assignments.data.active"
                :key="assignment.id"
                :assignment="assignment"
                status="active"
              />
            </div>
          </div>

          <!-- Missing (past due, not submitted) -->
          <div
            v-if="assignments?.data?.missing?.length"
            class="space-y-3"
          >
            <h2
              class="text-lg font-semibold tracking-tight flex items-center gap-2"
            >
              <UIcon
                name="i-lucide-alert-triangle"
                class="size-4 text-error"
              />
              Not Submitted
            </h2>
            <div class="grid gap-4">
              <AssignmentCard
                v-for="assignment in assignments.data.missing"
                :key="assignment.id"
                :assignment="assignment"
                status="missing"
              />
            </div>
          </div>

          <!-- Done -->
          <div
            v-if="assignments?.data?.done?.length"
            class="space-y-3"
          >
            <h2
              class="text-lg font-semibold tracking-tight flex items-center gap-2"
            >
              <UIcon
                name="i-lucide-check-circle"
                class="size-4 text-success"
              />
              Done
            </h2>
            <div class="grid gap-4">
              <AssignmentCard
                v-for="assignment in assignments.data.done"
                :key="assignment.id"
                :assignment="assignment"
                status="done"
              />
            </div>
          </div>
        <!-- Active -->
        </template>
      </div>
    </UContainer>
  </ClientOnly>
</template>
