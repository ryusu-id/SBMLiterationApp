<script lang="ts" setup>
defineProps<{
  title?: string
}>()
</script>

<template>
  <UDashboardNavbar :title>
    <template #leading>
      <UDashboardSidebarCollapse />
    </template>

    <template #right>
      <UColorModeButton />
    </template>
  </UDashboardNavbar>
</template>
