<script lang="ts" setup>
import { $authedFetch } from '~/apis/api'
import type { ApiResponse } from '~/apis/api'

interface GroupMember {
  fullname: string
  nim: string
  programStudy: string
  campus: string
  class: string
  pictureUrl: string
}

interface MyGroup {
  id: number
  name: string
  description?: string | null
  members: GroupMember[]
}

const myGroup = ref<MyGroup | null>(null)
const pending = ref(false)

async function fetchMyGroup() {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<MyGroup | null>>('/groups/my')
    myGroup.value = response.data ?? null
  } catch {
    // silently ignore — not being in a group is valid
  } finally {
    pending.value = false
  }
}

onMounted(() => {
  fetchMyGroup()
})
</script>

<template>
  <div
    v-if="pending"
    class="flex items-center justify-center py-6"
  >
    <UIcon
      name="i-heroicons-arrow-path"
      class="animate-spin text-4xl"
    />
  </div>

  <UCard
    v-else-if="myGroup"
    :ui="{
      body: 'space-y-6'
    }"
  >
    <div class="flex items-center justify-between">
      <h2 class="text-xl font-semibold">
        Group Information
      </h2>
      <UBadge
        color="primary"
        variant="subtle"
        size="lg"
      >
        {{ myGroup.members.length }} member{{ myGroup.members.length !== 1 ? 's' : '' }}
      </UBadge>
    </div>

    <div class="space-y-2">
      <p class="text-sm font-medium text-muted">
        Group Name
      </p>
      <p class="text-xl font-bold">
        {{ myGroup.name }}
      </p>
    </div>

    <div
      v-if="myGroup.description"
      class="space-y-2"
    >
      <p class="text-sm font-medium text-muted">
        Description
      </p>
      <p class="font-semibold">
        {{ myGroup.description }}
      </p>
    </div>

    <div class="space-y-3">
      <p class="text-sm font-medium text-muted">
        Team Member
      </p>

      <div class="space-y-3">
        <div
          v-for="member in myGroup.members"
          :key="member.nim"
          class="rounded-2xl border border-muted p-4 space-y-3"
        >
          <div class="flex items-center gap-3">
            <UAvatar
              icon="i-lucide-user-circle"
              size="xl"
              :src="member.pictureUrl"
              :alt="member.fullname"
            />
            <div class="space-y-1">
              <p class="text-lg font-bold leading-tight">
                {{ member.fullname }}
              </p>
              <UBadge
                color="primary"
                variant="subtle"
              >
                NIM : {{ member.nim }}
              </UBadge>
            </div>
          </div>

          <div class="flex flex-col sm:flex-row sm:divide-x divide-y sm:divide-y-0 divide-muted border border-muted rounded-xl overflow-hidden text-left sm:text-center text-sm">
            <div class="flex-1 py-2 px-3 min-w-0">
              <p class="text-xs text-muted">
                Class
              </p>
              <p class="font-medium break-words hyphens-auto">
                {{ member.class }}
              </p>
            </div>
            <div class="flex-1 py-2 px-3 min-w-0">
              <p class="text-xs text-muted">
                Campus
              </p>
              <p class="font-medium break-words hyphens-auto">
                {{ member.campus }}
              </p>
            </div>
            <div class="flex-1 py-2 px-3 min-w-0">
              <p class="text-xs text-muted">
                Study Program
              </p>
              <p class="font-medium break-words hyphens-auto">
                {{ member.programStudy }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </UCard>
</template>
