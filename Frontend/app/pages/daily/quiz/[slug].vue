<script setup lang="ts">
import { $authedFetch, handleResponseError, type ApiResponse } from '~/apis/api'

definePageMeta({
  middleware: ['auth', 'participant-only']
})

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

interface QuizSubmission {
  answers: {
    questionSeq: number
    answer: string
  }[]
}

interface QuizResult {
  totalQuestions: number
  minimalCorrectAnswers: number
  correctAnswers: number
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const slug = computed(() => route.params.slug as string)

const quiz = ref<QuizQuestion[]>([])
const currentQuestionIndex = ref(0)
const selectedAnswer = ref<string>('')
const userAnswers = ref<{ questionSeq: number, answer: string }[]>([])
const pending = ref(false)
const submitting = ref(false)
const quizCompleted = ref(false)
const quizResult = ref<QuizResult | null>(null)
const passed = ref(false)

const currentQuestion = computed(() => quiz.value[currentQuestionIndex.value])
const isLastQuestion = computed(() => currentQuestionIndex.value === quiz.value.length - 1)
const progress = computed(() => ((currentQuestionIndex.value + 1) / quiz.value.length) * 100)

function shuffleArray<T>(array: T[]): T[] {
  const shuffled = [...array]
  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [shuffled[i], shuffled[j]] = [shuffled[j]!, shuffled[i]!]
  }
  return shuffled
}

onMounted(async () => {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<QuizQuestion[]>>(
      `/daily-reads/${slug.value}/quiz`
    )

    if (response.data && Array.isArray(response.data)) {
      // Shuffle questions and their choices
      const shuffledQuestions = shuffleArray(response.data).map(question => ({
        ...question,
        choices: shuffleArray(question.choices)
      }))
      quiz.value = shuffledQuestions
    } else {
      handleResponseError(response)
      router.push('/dashboard')
    }
  } catch (err) {
    handleResponseError(err)
    router.push('/dashboard')
  } finally {
    pending.value = false
  }
})

function selectAnswer(choice: string) {
  selectedAnswer.value = choice
}

async function nextQuestion() {
  if (!selectedAnswer.value) {
    toast.add({
      title: 'Please select an answer',
      color: 'error'
    })
    return
  }

  if (!currentQuestion.value) return

  // Store the answer
  userAnswers.value.push({
    questionSeq: currentQuestion.value.questionSeq,
    answer: selectedAnswer.value
  })

  if (isLastQuestion.value) {
    await submitQuiz()
  } else {
    currentQuestionIndex.value++
    selectedAnswer.value = ''
  }
}

async function submitQuiz() {
  try {
    submitting.value = true

    const submission: QuizSubmission = {
      answers: userAnswers.value
    }

    await $authedFetch(`/daily-reads/${slug.value}/quiz/submit`, {
      method: 'POST',
      body: submission
    })

    // Fetch the quiz result
    pending.value = true
    const response = await $authedFetch<ApiResponse<QuizResult>>(
      `/daily-reads/${slug.value}/quiz/result`
    )

    if (response.data) {
      quizResult.value = response.data
      quizCompleted.value = true

      passed.value = response.data.correctAnswers >= response.data.minimalCorrectAnswers

      if (passed.value) {
        toast.add({
          title: 'Congratulations! 🎉',
          description: `You passed the quiz! Score: ${response.data.correctAnswers}/${response.data.totalQuestions}`,
          color: 'success'
        })
      } else {
        toast.add({
          title: 'Quiz Completed',
          description: `You scored ${response.data.correctAnswers}/${response.data.totalQuestions}. Keep practicing!`,
          color: 'warning'
        })
      }
    } else {
      handleResponseError(response)
    }
  } catch (err) {
    handleResponseError(err)
  } finally {
    submitting.value = false
    pending.value = false
  }
}

function backToDashboard() {
  router.push('/dashboard')
}
</script>

<template>
  <UContainer class="py-8">
    <div
      v-if="pending"
      class="flex items-center justify-center py-12"
    >
      <UIcon
        name="i-heroicons-arrow-path"
        class="animate-spin text-4xl"
      />
    </div>

    <!-- Quiz Completed -->
    <div
      v-else-if="quizCompleted && quizResult"
      class="space-y-6"
    >
      <UCard>
        <div class="text-center space-y-6 py-8">
          <div class="flex justify-center">
            <div
              class="w-32 h-32 rounded-full flex items-center justify-center"
              :class="passed ? 'bg-primary-100 dark:bg-primary-900' : 'bg-red-100 dark:bg-red-900'"
            >
              <UIcon
                :name="passed ? 'i-heroicons-trophy' : 'i-heroicons-x-circle'"
                class="size-16"
                :class="passed ? 'text-primary-600' : 'text-red-600'"
              />
            </div>
          </div>

          <div>
            <h1 class="text-4xl font-bold mb-2">
              Quiz Completed!
            </h1>
            <p class="text-xl text-gray-600">
              Your Score: {{ quizResult.correctAnswers }} / {{ quizResult.totalQuestions }}
            </p>
          </div>

          <div class="flex justify-center gap-4 pt-4">
            <UButton
              size="lg"
              color="neutral"
              variant="outline"
              @click="backToDashboard"
            >
              Back to Dashboard
            </UButton>
          </div>
        </div>
      </UCard>
    </div>

    <!-- Quiz Questions -->
    <div
      v-else-if="quiz.length > 0"
      class="space-y-6"
    >
      <!-- Progress Bar -->
      <div class="space-y-2">
        <div class="flex justify-between text-sm text-gray-600">
          <span>Question {{ currentQuestionIndex + 1 }} of {{ quiz.length }}</span>
          <span>{{ Math.round(progress) }}%</span>
        </div>
        <div class="w-full bg-gray-200 rounded-full h-2">
          <div
            class="bg-primary-600 h-2 rounded-full transition-all duration-300"
            :style="{ width: `${progress}%` }"
          />
        </div>
      </div>

      <!-- Question Card -->
      <UCard
        v-if="currentQuestion"
        variant="soft"
        :ui="{
          root: 'bg-transparent'
        }"
      >
        <div class="space-y-6">
          <div>
            <div class="flex items-start gap-3 mb-4">
              <div class="flex-shrink-0 w-10 h-10 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center font-bold text-primary-600">
                {{ currentQuestionIndex + 1 }}
              </div>
              <h2 class="text-2xl font-semibold pt-1">
                {{ currentQuestion.question }}
              </h2>
            </div>
          </div>

          <!-- Choices -->
          <div class="space-y-3">
            <div
              v-for="choice in currentQuestion.choices"
              :key="choice.id"
              class="relative"
            >
              <button
                class="w-full text-left p-4 rounded-lg border-2 transition-all duration-200"
                :class="[
                  selectedAnswer === choice.choice
                    ? 'border-primary-600 bg-primary-50 dark:bg-primary-900/20'
                    : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
                ]"
                @click="selectAnswer(choice.choice)"
              >
                <div class="flex items-center gap-3">
                  <div
                    class="flex-shrink-0 w-6 h-6 rounded-full border-2 transition-all duration-200 flex items-center justify-center"
                    :class="[
                      selectedAnswer === choice.choice
                        ? 'border-primary-600 bg-primary-600'
                        : 'border-gray-300 dark:border-gray-600'
                    ]"
                  >
                    <div
                      v-if="selectedAnswer === choice.choice"
                      class="w-3 h-3 rounded-full bg-white"
                    />
                  </div>
                  <div class="flex items-center gap-2">
                    <span class="text-gray-900 dark:text-gray-100">
                      {{ choice.answer }}
                    </span>
                  </div>
                </div>
              </button>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex justify-end pt-4">
            <UButton
              size="lg"
              :loading="submitting"
              :disabled="!selectedAnswer"
              @click="nextQuestion"
            >
              {{ isLastQuestion ? 'Submit Quiz' : 'Next Question' }}
              <template #trailing>
                <UIcon :name="isLastQuestion ? 'i-heroicons-check' : 'i-heroicons-arrow-right'" />
              </template>
            </UButton>
          </div>
        </div>
      </UCard>

      <!-- Info Alert -->
      <UAlert
        icon="i-heroicons-information-circle"
        color="primary"
        variant="subtle"
        title="Note"
        description="You cannot go back to previous questions. Make sure to choose your answer carefully."
      />
    </div>

    <div
      v-else
      class="flex flex-col items-center justify-center py-12"
    >
      <UIcon
        name="i-heroicons-exclamation-circle"
        class="size-16 text-gray-300 mb-4"
      />
      <p class="text-gray-500 text-center">
        No quiz available for this daily reading
      </p>
      <UButton
        class="mt-4"
        @click="backToDashboard"
      >
        Back to Dashboard
      </UButton>
    </div>
  </UContainer>
</template>
