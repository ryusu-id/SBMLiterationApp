<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'
import { useAuth, $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'
import type { UserProfile } from '~/pages/profile.vue'

defineProps<{
  collapsed?: boolean
}>()

const auth = useAuth()
const router = useRouter()

const user = ref({
  name: auth.getFullname() || 'User',
  avatar: {
    src: 'https://github.com/benjamincanac.png',
    alt: auth.getFullname() || 'User'
  }
})

async function fetchProfile() {
  try {
    const response = await $authedFetch<ApiResponse<UserProfile>>('/auth/site')
    if (response.data) {
      if (response.data.pictureUrl) {
        user.value.avatar.src = response.data.pictureUrl
      }
      user.value.name = response.data.fullname
      user.value.avatar.alt = response.data.fullname
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  }
}

onMounted(() => {
  fetchProfile()
})

const items = computed<DropdownMenuItem[][]>(() => ([[{
  type: 'label',
  label: user.value.name,
  avatar: user.value.avatar
}], [{
  label: 'Profile',
  icon: 'i-lucide-user'
},
{
  label: 'Log out',
  icon: 'i-lucide-log-out',
  onSelect() {
    auth.clearToken()
    router.push({ name: 'LandingPage' })
  }
}
]]))
</script>

<template>
  <ClientOnly>
    <UDropdownMenu
      :items="items"
      :content="{ align: 'center', collisionPadding: 12 }"
      :ui="{ content: collapsed ? 'w-48' : 'w-(--reka-dropdown-menu-trigger-width)' }"
    >
      <UButton
        v-bind="{
          ...user,
          label: collapsed ? undefined : user?.name,
          trailingIcon: collapsed ? undefined : 'i-lucide-chevrons-up-down'
        }"
        color="neutral"
        variant="ghost"
        block
        :square="collapsed"
        class="data-[state=open]:bg-elevated"
        :ui="{
          trailingIcon: 'text-dimmed'
        }"
      />

      <template #chip-leading="{ item }">
        <div class="inline-flex items-center justify-center shrink-0 size-5">
          <span
            class="rounded-full ring ring-bg bg-(--chip-light) dark:bg-(--chip-dark) size-2"
            :style="{
              '--chip-light': `var(--color-${(item as any).chip}-500)`,
              '--chip-dark': `var(--color-${(item as any).chip}-400)`
            }"
          />
        </div>
      </template>
    </UDropdownMenu>
  </ClientOnly>
</template>
