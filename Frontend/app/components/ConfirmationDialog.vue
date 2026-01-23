<script lang="ts" setup>
defineProps<{
  title: string
  subTitle: string
  message: string
}>()

const open = ref(false)

defineExpose({
  open: () => {
    open.value = true
  },
  close: () => {
    open.value = false
  }
})

const emit = defineEmits<{
  (e: 'ok' | 'cancel'): void
}>()

function onOK() {
  emit('ok')
  open.value = false
}

function onCancel() {
  emit('cancel')
  open.value = false
}
</script>

<template>
  <UModal
    v-model:open="open"
    :title="title"
    :description="subTitle"
    :close="{ variant: 'subtle' }"
  >
    <template #body>
      <p class="pb-4">
        {{ message }}
      </p>
      <div class="flex flex-row justify-end gap-x-2">
        <UButton
          variant="soft"
          label="No"
          icon="i-lucide-x"
          @click="onCancel"
        />
        <UButton
          label="OK"
          icon="i-lucide-check"
          @click="onOK"
        />
      </div>
    </template>
  </UModal>
</template>
