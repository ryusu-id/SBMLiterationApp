<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'

// TODO: add discriminator for display for Dashboard (with image) and non Dashboard (without image)
defineProps<{
  book: {
    id: number
    title: string
    author: string
    imageUrl: string
    totalPage: number
    category: string
    xp: number
  }
  googleBooks?: boolean
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const loading = ref(false)
const toast = useToast()

const dialog = useDialog()
async function addToReadList(book: { id: number }) {
  dialog.confirm({
    title: 'Add to Read List',
    subTitle: 'Bonus exp is aqcuired upon completion!',
    message: 'Are you sure you want to add this book to your reading list?',
    onOk: async () => {
      try {
        loading.value = true
        const result = await $authedFetch<ApiResponse>('/reading-resources/from-recommendation', {
          method: 'POST',
          body: {
            recommendationId: book.id
          }
        })

        if (result.errorCode || result.errorDescription) {
          handleResponseError(result)
          return
        }

        toast.add({
          title: 'Book added to your reading list!',
          color: 'success'
        })

        emit('refresh')
      } catch (error) {
        handleResponseError(error)
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<template>
  <!-- TODO: Style and space all of below component to match -->
  <UCard
    variant="soft"
    :ui="{
      root: 'rounded-[22px] w-fit bg-slate-100 dark:bg-slate-800',
      body: 'p-3 sm:p-3'
    }"
  >
    <div class="flex flex-col gap-x-4">
      <div
        class="w-[178px] sm:w-[240px] aspect-[3/4] shrink-0 overflow-hidden rounded-[12px] relative"
      >
        <UBadge
          v-if="!googleBooks && book.xp"
          class="absolute right-3 top-3 tracking-tight font-semibold rounded-lg text-[12px] shadow-sm"
        >
          +{{ book.xp }}xp
        </UBadge>
        <img
          :src="book.imageUrl"
          alt="Book Cover"
          class="w-full h-full object-cover"
        >
      </div>

      <div class="flex flex-col mt-[15px]">
        <div class="flex flex-col justify-between mb-[10px]">
          <h1
            class="text-[13px] sm:text-[15px] tracking-tight font-bold line-clamp-1 leading-none text-left max-w-[178px] sm:max-w-[240px]"
          >
            {{ book.title }}
          </h1>
          <h2
            class="text-[11px] sm:text-[13px] tracking-tight text-primary font-semibold line-clamp-1 leading-tight text-left"
          >
            {{ book.category }}
          </h2>
        </div>

        <div
          class="flex flex-row gap-x-2 items-center justify-between w-full mb-[10px]"
        >
          <div class="flex flex-col tracking-tight justify-start items-start">
            <p
              class="font-medium text-[10px] sm:text-[12px] leading-none "
            >
              Author
            </p>
            <p
              class="font-semibold text-[11px] sm:text-[13px] line-clamp-1 max-w-[150px] text-left"
            >
              {{ book.author }}
            </p>
          </div>
          <div class="flex flex-col tracking-tight text-right min-w-[50px]">
            <p
              class="font-medium text-[10px] sm:text-[12px] leading-none"
            >
              Total Page
            </p>
            <p class="font-semibold text-[11px] sm:text-[13px]">
              {{ book.totalPage }}
            </p>
          </div>
        </div>
        <div>
          <UButton
            v-if="!googleBooks"
            class="w-full font-semibold rounded-full text-center bg-primary text-[13px] justify-center"
            :loading="loading"
            @click="addToReadList(book)"
          >
            Add to Read List
          </UButton>
        </div>
      </div>
    </div>
  </UCard>
</template>
