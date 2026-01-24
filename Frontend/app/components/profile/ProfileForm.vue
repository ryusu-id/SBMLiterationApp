<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

defineProps<{
  loading?: boolean
}>()

const schema = z.object({
  fullname: z.string().min(1, 'Full name is required'),
  nim: z.string().min(1, 'NIM is required').max(50, 'NIM must be at most 50 characters').regex(/^[a-zA-Z0-9]+$/, 'NIM must be alphanumeric'),
  programStudy: z.string().min(1, 'Program study is required').max(100, 'Program study must be at most 100 characters'),
  faculty: z.string().min(1, 'Faculty is required').max(100, 'Faculty must be at most 100 characters'),
  generationYear: z.string().min(4, 'Generation year must be 4 digits').max(4, 'Generation year must be 4 digits').regex(/^\d{4}$/, 'Generation year must be 4 digits')
})

export type ProfileFormSchema = z.output<typeof schema>

const isOpen = ref(false)

const state = reactive({
  fullname: '',
  nim: '',
  programStudy: '',
  faculty: '',
  generationYear: ''
})

const emit = defineEmits<{
  (e: 'submit', data: ProfileFormSchema): void
}>()

function setState(data: Partial<ProfileFormSchema>) {
  if (data.fullname !== undefined) state.fullname = data.fullname
  if (data.nim !== undefined) state.nim = data.nim
  if (data.programStudy !== undefined) state.programStudy = data.programStudy
  if (data.faculty !== undefined) state.faculty = data.faculty
  if (data.generationYear !== undefined) state.generationYear = data.generationYear
}

function resetState() {
  state.fullname = ''
  state.nim = ''
  state.programStudy = ''
  state.faculty = ''
  state.generationYear = ''
}

function open() {
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

async function onSubmit(event: FormSubmitEvent<ProfileFormSchema>) {
  emit('submit', event.data)
}
</script>

<template>
  <UModal
    v-model:open="isOpen"
    title="Edit Profile"
    description="Update your profile information"
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
        <!-- Full Name field -->
        <UFormField
          label="Full Name"
          name="fullName"
          required
        >
          <UInput
            v-model="state.fullname"
            placeholder="Enter your full name"
            size="lg"
            class="w-full"
          />
        </UFormField>

        <!-- NIM field -->
        <UFormField
          label="NIM (Student ID)"
          name="nim"
          required
        >
          <UInput
            v-model="state.nim"
            placeholder="Enter your NIM"
            size="lg"
            class="w-full"
            maxlength="50"
          />
        </UFormField>

        <!-- Program Study and Faculty - responsive grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
          <UFormField
            label="Program Study"
            name="programStudy"
            required
          >
            <UInput
              v-model="state.programStudy"
              placeholder="e.g., Computer Science"
              size="lg"
              class="w-full"
              maxlength="100"
            />
          </UFormField>

          <UFormField
            label="Faculty"
            name="faculty"
            required
          >
            <UInput
              v-model="state.faculty"
              placeholder="e.g., Engineering"
              size="lg"
              class="w-full"
              maxlength="100"
            />
          </UFormField>
        </div>

        <!-- Generation Year field -->
        <UFormField
          label="Generation Year"
          name="generationYear"
          required
        >
          <UInput
            v-model="state.generationYear"
            type="text"
            placeholder="e.g., 2020"
            size="lg"
            class="w-full"
            maxlength="4"
          />
        </UFormField>

        <!-- Submit button -->
        <div class="flex justify-end pt-4">
          <UButton
            type="submit"
            size="lg"
            class="px-8 w-full text-center flex justify-center"
            :loading
          >
            Save Profile
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>
