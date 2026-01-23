<script setup lang="ts">
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin'
})
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Book Recommendation" />
    </template>

    <template #body>
      <h1 class="text-2xl font-bold mb-4">
        Book Recommendation
      </h1>
      <p>Welcome to the admin dashboard!</p>
    </template>
  </UDashboardPanel>
</template>
