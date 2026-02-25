<script setup lang="ts">
import { handleResponseError } from '~/apis/api'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

interface AssignmentSubmissionFile {
  id: number
}

interface AssignmentSubmission {
  id: number
  isCompleted: boolean
  completedAt?: string
  files: AssignmentSubmissionFile[]
}

interface MyAssignment {
  id: number
  title: string
  description?: string
  dueDate?: string
  submission?: AssignmentSubmission
}

const useAuthedFetch = useNuxtApp().$useAuthedFetch

const { data: assignments, pending, error } = await useAuthedFetch<{ data: MyAssignment[] }>('/assignments/my')

watch(error, (err) => {
  if (err) handleResponseError(err)
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
        v-else-if="!assignments?.data?.length"
        class="flex flex-col items-center py-12 text-muted"
      >
        <UIcon
          name="i-lucide-clipboard-list"
          class="text-5xl mb-4"
        />
        <p>No assignments yet</p>
      </div>

      <div
        v-else
        class="grid gap-4"
      >
        <NuxtLink
          v-for="assignment in assignments.data"
          :key="assignment.id"
          :to="`/assignments/${assignment.id}`"
        >
          <UCard class="hover:ring-1 hover:ring-primary transition-shadow cursor-pointer">
            <div class="flex items-start justify-between gap-4">
              <div class="flex-1 min-w-0">
                <h3 class="text-lg font-semibold truncate">
                  {{ assignment.title }}
                </h3>
                <p
                  v-if="assignment.description"
                  class="text-sm text-muted mt-1 line-clamp-2"
                >
                  {{ assignment.description }}
                </p>
                <div class="flex items-center gap-3 mt-2 flex-wrap">
                  <span
                    v-if="assignment.dueDate"
                    class="text-xs text-muted flex items-center gap-1"
                  >
                    <UIcon
                      name="i-lucide-calendar"
                      class="size-3"
                    />
                    Due: {{ new Date(assignment.dueDate).toLocaleDateString() }}
                  </span>
                  <span class="text-xs text-muted flex items-center gap-1">
                    <UIcon
                      name="i-lucide-paperclip"
                      class="size-3"
                    />
                    {{ assignment.submission?.files?.length || 0 }} file(s)
                  </span>
                </div>
              </div>
              <UBadge
                :color="assignment.submission?.isCompleted ? 'success' : 'neutral'"
                variant="subtle"
                class="shrink-0"
              >
                {{ assignment.submission?.isCompleted ? 'Completed' : 'In Progress' }}
              </UBadge>
            </div>
          </UCard>
        </NuxtLink>
      </div>
    </div>
  </UContainer>
</template>
