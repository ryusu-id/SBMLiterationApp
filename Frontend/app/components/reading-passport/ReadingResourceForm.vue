<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'

const props = defineProps<{
  loading?: boolean
  journal?: boolean
}>()

const schema = z.object({
  title: z.string().min(1, 'Title is required'),
  isbn: props.journal ? z.string().optional() : z.string().min(1, 'ISBN is required'),
  authors: z.array(z.string().min(1, 'Author name is required')).min(1, 'At least one author is required'),
  publishYear: z.string().regex(/^\d{4}$/, 'Publish year must be a 4-digit year'),
  bookCategory: z.string().min(1, 'Book category is required'),
  page: z.coerce.number().min(1, 'Page count must be at least 1'),
  resourceLink: z.url('Must be a valid URL').optional()
})

export type ReadingResourceSchema = z.output<typeof schema>

const state = reactive({
  title: '',
  isbn: '',
  authors: [''],
  publishYear: '',
  bookCategory: '',
  page: NaN,
  resourceLink: ''
})

const emit = defineEmits<{
  (e: 'submit', data: Omit<ReadingResourceSchema, 'authors'> & { authors: string }): void
}>()

function addAuthor() {
  state.authors.push('')
}

function removeAuthor(index: number) {
  if (state.authors.length > 1) {
    state.authors.splice(index, 1)
  }
}

function setState(data: Partial<ReadingResourceSchema>) {
  if (data.title !== undefined) state.title = data.title
  if (data.isbn !== undefined) state.isbn = data.isbn
  if (data.authors !== undefined) state.authors = data.authors.length > 0 ? data.authors : ['']
  if (data.publishYear !== undefined) state.publishYear = data.publishYear
  if (data.bookCategory !== undefined) state.bookCategory = data.bookCategory
  if (data.page !== undefined) state.page = data.page
  if (data.resourceLink !== undefined) state.resourceLink = data.resourceLink
}

function resetState() {
  state.title = ''
  state.isbn = ''
  state.authors = ['']
  state.publishYear = ''
  state.bookCategory = ''
  state.page = NaN
  state.resourceLink = ''
}

defineExpose({
  setState,
  resetState
})

async function onSubmit(event: FormSubmitEvent<ReadingResourceSchema>) {
  emit('submit',
    {
      ...event.data,
      authors: event.data.authors
        .map(author => author.trim())
        .filter(author => author.length > 0)
        .join(', ')
    })
  // TODO: loading state must be implemented, maybe from props
}
</script>

<template>
  <UCard variant="soft">
    <UForm
      :schema="schema"
      :state="state"
      class="space-y-6"
      @submit="onSubmit"
    >
      <!-- Title field - full width -->
      <UFormField
        label="Title"
        name="title"
        required
      >
        <UInput
          v-model="state.title"
          placeholder="Enter book title"
          size="lg"
        />
      </UFormField>

      <!-- ISBN and Publish Year - responsive grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
        <UFormField
          v-if="!journal"
          label="ISBN"
          name="isbn"
          required
        >
          <UInput
            v-model="state.isbn"
            placeholder="Enter ISBN"
            size="lg"
          />
        </UFormField>

        <UFormField
          label="Publish Year"
          name="publishYear"
          required
        >
          <UInput
            v-model="state.publishYear"
            type="text"
            placeholder="Enter publish year"
            size="lg"
            maxlength="4"
          />
        </UFormField>
      </div>

      <!-- Authors field - dynamic array -->
      <div class="space-y-3">
        <div class="flex items-center justify-between">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-200">
            Authors <span class="text-red-500">*</span>
          </label>
          <UButton
            type="button"
            color="primary"
            variant="soft"
            icon="i-heroicons-plus"
            @click="addAuthor"
          >
            Add Author
          </UButton>
        </div>

        <div
          v-for="(author, index) in state.authors"
          :key="index"
          class="flex gap-2 items-start"
        >
          <div class="flex-1">
            <UFormField
              :name="`authors[${index}]`"
              required
            >
              <UInput
                v-model="state.authors[index]"
                :placeholder="`Author ${index + 1} name`"
                size="lg"
              />
            </UFormField>
          </div>
          <UButton
            v-if="state.authors.length > 1"
            type="button"
            size="lg"
            color="error"
            variant="soft"
            icon="i-heroicons-trash"
            square
            @click="removeAuthor(index)"
          />
        </div>
        <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
          Add multiple authors as needed. At least one author is required.
        </p>
      </div>

      <!-- Book Category and Page Count - responsive grid -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4 md:gap-6">
        <UFormField
          label="Book Category"
          name="bookCategory"
          required
        >
          <UInput
            v-model="state.bookCategory"
            placeholder="Enter book category"
            size="lg"
          />
        </UFormField>

        <UFormField
          label="Page Count"
          name="page"
          required
        >
          <UInput
            v-model="state.page"
            type="number"
            placeholder="Enter page count"
            size="lg"
          />
        </UFormField>
      </div>

      <!-- Resource Link - full width -->
      <UFormField
        :label="journal ? 'DOI Link' : 'Google Books Link'"
        name="resourceLink"
      >
        <UInput
          v-model="state.resourceLink"
          type="url"
          placeholder="https://example.com/resource"
          size="lg"
        />
      </UFormField>

      <!-- Submit button -->
      <div class="flex justify-end pt-4">
        <UButton
          type="submit"
          size="lg"
          class="px-8"
          :loading
        >
          Save
        </UButton>
      </div>
    </UForm>
  </UCard>
</template>
