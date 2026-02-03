<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'

interface QuizChoice {
  id: number
  choice: string
  answer: string
}

interface QuizQuestion {
  id: number
  questionSeq: number
  question: string
  correctAnswer: string
  choices: QuizChoice[]
}

const props = defineProps<{
  dailyReadId: number | null
}>()

const quiz = ref<QuizQuestion[] | null>(null)
const pending = ref(false)
const toast = useToast()
const quizFile = ref<File | null>(null)

async function handleQuizFileUpload(files: File[]) {
  if (!files || !files[0] || files.length === 0) return
  quizFile.value = files[0]
  toast.add({
    title: 'Quiz file selected',
    description: `${files[0].name} ready to upload`,
    color: 'success'
  })
}

function getQuizFile() {
  return quizFile.value
}

function clearQuizFile() {
  quizFile.value = null
}

async function downloadQuizTemplate() {
  try {
    const response = await $authedFetch('/daily-reads/quiz/template', {
      responseType: 'blob'
    })

    // Create download link
    const url = window.URL.createObjectURL(new Blob([response as Blob]))
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', 'quiz-template.xlsx')
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.URL.revokeObjectURL(url)

    toast.add({
      title: 'Template downloaded',
      color: 'success'
    })
  } catch (error) {
    handleResponseError(error)
  }
}

async function fetchQuiz() {
  if (!props.dailyReadId) {
    quiz.value = null
    return
  }

  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<QuizQuestion[]>>(
      `/daily-reads/${props.dailyReadId}/quiz/review`
    )

    if (response.data && Array.isArray(response.data)) {
      quiz.value = response.data
    } else {
      quiz.value = null
    }
  } catch {
    // Quiz might not exist yet, that's okay
    quiz.value = null
  } finally {
    pending.value = false
  }
}

watch(() => props.dailyReadId, () => {
  fetchQuiz()
}, { immediate: true })

defineExpose({
  refresh: fetchQuiz,
  getQuizFile,
  clearQuizFile
})
</script>

<template>
  <div class="flex flex-col h-[calc(100vh-12rem)] max-h-[800px]">
    <div class="mb-4 flex-shrink-0">
      <div class="flex items-center justify-between mb-2">
        <h3 class="text-lg font-semibold">
          Quiz Questions
        </h3>
        <UButton
          icon="i-heroicons-arrow-down-tray"
          size="xs"
          color="neutral"
          variant="outline"
          @click="downloadQuizTemplate"
        >
          Template
        </UButton>
      </div>
      <p class="text-sm text-gray-500">
        {{ dailyReadId ? 'Current quiz for this daily read' : 'Save daily read first to add quiz' }}
      </p>

      <div class="mt-4 space-y-3">
        <UInput
          type="file"
          accept=".xlsx,.xls"
          @change="(e) => handleQuizFileUpload(Array.from((e.target as HTMLInputElement).files || []))"
        />

        <div
          v-if="quizFile"
          class="text-sm text-gray-600 dark:text-gray-400"
        >
          Selected file: <span class="font-medium">{{ quizFile.name }}</span>
        </div>

        <p class="text-xs text-gray-500">
          Upload your quiz questions file. The quiz will be uploaded after saving the daily read.
        </p>
      </div>
    </div>

    <div
      v-if="pending"
      class="flex items-center justify-center py-12 flex-1"
    >
      <UIcon
        name="i-heroicons-arrow-path"
        class="animate-spin text-4xl"
      />
    </div>

    <div
      v-else-if="!dailyReadId"
      class="flex flex-col items-center justify-center py-12 text-center flex-1"
    >
      <UIcon
        name="i-heroicons-document-text"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500">
        Create the daily read first to add quiz questions
      </p>
    </div>

    <div
      v-else-if="!quiz || quiz.length === 0"
      class="flex flex-col items-center justify-center py-12 text-center flex-1"
    >
      <UIcon
        name="i-heroicons-document-text"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500">
        No quiz uploaded yet
      </p>
    </div>

    <div
      v-else
      class="flex-1 overflow-y-auto space-y-4 min-h-0"
    >
      <div
        v-for="question in quiz"
        :key="question.id"
        class="p-4 border border-gray-200 dark:border-gray-700 rounded-lg"
      >
        <div class="font-medium mb-3">
          {{ question.questionSeq }}. {{ question.question }}
        </div>
        <div class="space-y-2 ml-4">
          <div
            v-for="choice in question.choices"
            :key="choice.id"
            class="flex items-start gap-2"
            :class="choice.choice === question.correctAnswer ? 'text-green-600 dark:text-green-400 font-medium' : 'text-gray-600 dark:text-gray-400'"
          >
            <UIcon
              :name="choice.choice === question.correctAnswer ? 'i-heroicons-check-circle' : 'i-heroicons-minus-circle'"
              class="size-4 mt-0.5 flex-shrink-0"
            />
            <span>{{ choice.answer }}</span>
          </div>
        </div>
        <div class="mt-3 pt-3 border-t border-gray-200 dark:border-gray-700">
          <span class="text-xs font-medium text-gray-500 dark:text-gray-400">Correct Answer:</span>
          <span class="text-xs font-semibold text-green-600 dark:text-green-400 ml-2">{{ question.correctAnswer }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
