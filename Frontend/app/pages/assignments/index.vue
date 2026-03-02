<script setup lang="ts">
import { handleResponseError } from '~/apis/api'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

// Grouped shape from GET /api/assignments/my
interface MyAssignmentItem {
  id: number
  title: string
  description?: string
  dueDate?: string
  submissionId: number
  isCompleted: boolean
  completedAt?: string
  fileCount: number
}

interface MyAssignmentsResponse {
  active: MyAssignmentItem[]
  done: MyAssignmentItem[]
  missing: MyAssignmentItem[]
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
        <!-- Active -->
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
            <NuxtLink
              v-for="assignment in assignments.data.active"
              :key="assignment.id"
              :to="`/assignments/${assignment.id}`"
            >
              <UCard
                class="hover:ring-1 hover:ring-primary transition-shadow cursor-pointer"
              >
                <div class="flex items-start justify-between gap-4">
                  <div class="flex-1 min-w-0">
                    <div class="flex justify-between">
                      <h3 class="text-lg font-semibold truncate">
                        {{ assignment.title }}
                      </h3>
                      <UBadge
                        color="neutral"
                        variant="subtle"
                        class="shrink-0"
                      >
                        Not Submitted
                      </UBadge>
                    </div>
                    <p
                      v-if="assignment.description"
                      class="text-sm text-muted mt-1 line-clamp-2"
                    >
                      {{ assignment.description }}
                    </p>
                    <div
                      class="flex justify-between items-center gap-3 mt-2 flex-wrap"
                    >
                      <span
                        v-if="assignment.dueDate"
                        class="text-xs text-muted flex items-center gap-2"
                      >
                        <UIcon
                          name="i-lucide-calendar"
                          class="size-3"
                        />
                        Due:
                        {{
                          new Date(assignment.dueDate).toLocaleString("en-GB", {
                            day: "numeric",
                            month: "short",
                            year: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                            hour12: false
                          })
                        }}
                      </span>
                      <span class="text-xs text-muted flex items-center gap-1">
                        <UIcon
                          name="i-lucide-paperclip"
                          class="size-3"
                        />
                        {{ assignment.fileCount || 0 }} file(s)
                      </span>
                    </div>
                  </div>
                </div>
              </UCard>
            </NuxtLink>
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
            <NuxtLink
              v-for="assignment in assignments.data.missing"
              :key="assignment.id"
              :to="`/assignments/${assignment.id}`"
            >
              <UCard
                class="hover:ring-1 hover:ring-error transition-shadow cursor-pointer"
              >
                <div class="flex items-start justify-between gap-4">
                  <div class="flex-1 min-w-0">
                    <div class="flex justify-between">
                      <h3 class="text-lg font-semibold truncate">
                        {{ assignment.title }}
                      </h3>
                      <UBadge
                        color="error"
                        variant="subtle"
                        class="shrink-0"
                      >
                        Overdue
                      </UBadge>
                    </div>
                    <p
                      v-if="assignment.description"
                      class="text-sm text-muted mt-1 line-clamp-2"
                    >
                      {{ assignment.description }}
                    </p>
                    <div
                      class="flex justify-between items-center gap-3 mt-2 flex-wrap"
                    >
                      <span
                        v-if="assignment.dueDate"
                        class="text-xs text-muted flex items-center gap-2"
                      >
                        <UIcon
                          name="i-lucide-calendar"
                          class="size-3"
                        />
                        Due:
                        {{
                          new Date(assignment.dueDate).toLocaleString("en-GB", {
                            day: "numeric",
                            month: "short",
                            year: "numeric",
                            hour: "2-digit",
                            minute: "2-digit",
                            hour12: false
                          })
                        }}
                      </span>
                      <span class="text-xs text-muted flex items-center gap-1">
                        <UIcon
                          name="i-lucide-paperclip"
                          class="size-3"
                        />
                        {{ assignment.fileCount || 0 }} file(s)
                      </span>
                    </div>
                  </div>
                </div>
              </UCard>
            </NuxtLink>
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
            <NuxtLink
              v-for="assignment in assignments.data.done"
              :key="assignment.id"
              :to="`/assignments/${assignment.id}`"
            >
              <UCard
                class="hover:ring-1 hover:ring-primary transition-shadow cursor-pointer opacity-75"
              >
                <div class="flex items-start justify-between gap-4">
                  <div class="flex-1 min-w-0">
                    <div class="flex justify-between">
                      <h3 class="text-lg font-semibold truncate">
                        {{ assignment.title }}
                      </h3>
                      <UBadge
                        color="success"
                        variant="subtle"
                        class="shrink-0"
                      >
                        Submitted
                      </UBadge>
                    </div>
                    <p
                      v-if="assignment.description"
                      class="text-sm text-muted mt-1 line-clamp-2"
                    >
                      {{ assignment.description }}
                    </p>
                    <div
                      class="flex justify-between items-center gap-3 mt-2 flex-wrap"
                    >
                      <span
                        v-if="assignment.completedAt"
                        class="text-xs text-muted flex items-center gap-2"
                      >
                        <UIcon
                          name="i-lucide-check"
                          class="size-3"
                        />
                        Submitted:
                        {{
                          new Date(assignment.completedAt).toLocaleString(
                            "en-GB",
                            {
                              day: "numeric",
                              month: "short",
                              year: "numeric",
                              hour: "2-digit",
                              minute: "2-digit",
                              hour12: false
                            }
                          )
                        }}
                      </span>
                      <span class="text-xs text-muted flex items-center gap-1">
                        <UIcon
                          name="i-lucide-paperclip"
                          class="size-3"
                        />
                        {{ assignment.fileCount || 0 }} file(s)
                      </span>
                    </div>
                  </div>
                </div>
              </UCard>
            </NuxtLink>
          </div>
        </div>
      </template>
    </div>
  </UContainer>
</template>
