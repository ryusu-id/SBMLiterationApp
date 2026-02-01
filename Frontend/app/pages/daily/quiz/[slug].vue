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
  dailyReadId: string
  answers: {
    questionId: number
    answer: string
  }[]
}

const route = useRoute()
const router = useRouter()
const toast = useToast()

const slug = computed(() => route.params.slug as string)

const quiz = ref<QuizQuestion[]>([])
const currentQuestionIndex = ref(0)
const selectedAnswer = ref<string>('')
const userAnswers = ref<{ questionId: number, answer: string }[]>([])
const pending = ref(false)
const submitting = ref(false)
const quizCompleted = ref(false)
const score = ref(0)
const earnedExp = ref(0)

const currentQuestion = computed(() => quiz.value[currentQuestionIndex.value])
const isLastQuestion = computed(() => currentQuestionIndex.value === quiz.value.length - 1)
const progress = computed(() => ((currentQuestionIndex.value + 1) / quiz.value.length) * 100)

onMounted(async () => {
  try {
    pending.value = true
    const response = await $authedFetch<ApiResponse<QuizQuestion[]>>(
      `/daily-reads/${slug.value}/quiz`
    )

    if (response.data && Array.isArray(response.data)) {
      quiz.value = response.data.sort((a, b) => a.questionSeq - b.questionSeq)
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
    questionId: currentQuestion.value.id,
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
      dailyReadId: slug.value,
      answers: userAnswers.value
    }

    const response = await $authedFetch<ApiResponse<{
      score: number
      totalQuestions: number
      earnedExp: number
      passed: boolean
    }>>('/daily-reads/quiz/submit', {
      method: 'POST',
      body: submission
    })

    if (response.data) {
      score.value = response.data.score
      earnedExp.value = response.data.earnedExp
      quizCompleted.value = true

      if (response.data.passed) {
        toast.add({
          title: 'Congratulations! 🎉',
          description: `You passed the quiz! Earned ${response.data.earnedExp} EXP`,
          color: 'success'
        })
      } else {
        toast.add({
          title: 'Quiz Completed',
          description: `You scored ${response.data.score}/${response.data.totalQuestions}. Keep practicing!`,
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
      v-else-if="quizCompleted"
      class="space-y-6"
    >
      <UCard>
        <div class="text-center space-y-6 py-8">
          <div class="flex justify-center">
            <div class="w-32 h-32 rounded-full bg-primary-100 dark:bg-primary-900 flex items-center justify-center">
              <UIcon
                :name="score >= quiz.length * 0.7 ? 'i-heroicons-trophy' : 'i-heroicons-check-badge'"
                class="size-16 text-primary-600"
              />
            </div>
          </div>

          <div>
            <h1 class="text-4xl font-bold mb-2">
              Quiz Completed!
            </h1>
            <p class="text-xl text-gray-600">
              Your Score: {{ score }} / {{ quiz.length }}
            </p>
          </div>

          <div
            v-if="earnedExp > 0"
            class="flex items-center justify-center gap-2 text-lg"
          >
            <UIcon
              name="i-lucide-star"
              class="size-6 text-yellow-500"
            />
            <span class="font-semibold">+{{ earnedExp }} EXP</span>
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
                {{ currentQuestion.questionSeq }}
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
                    <span class="font-medium text-gray-700 dark:text-gray-300">
                      {{ choice.choice }}.
                    </span>
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
