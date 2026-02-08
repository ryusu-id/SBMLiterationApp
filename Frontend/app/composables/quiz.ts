import { useAuth } from '~/apis/api'

interface QuizAnswer {
  questionSeq: number
  answer: string
  questionLastUpdate?: string
}

interface PersistedQuizState {
  slug: string
  title?: string
  date: string
  totalQuestions: number
  answeredQuestions: number
  userAnswers: QuizAnswer[]
  answeredQuestionSeqs: number[]
  lastUpdated: string
}

export function usePersistedQuiz() {
  const STORAGE_PREFIX = 'quiz-'
  const AUTH_PREFIX = 'auth-quiz-user-full-name'

  function init() {
    if (import.meta.client) {
      const auth = useAuth()

      const fullname = auth.getFullname()
      if (fullname && localStorage.getItem(AUTH_PREFIX) !== fullname) {
      // Clear all quiz data if user has changed
        for (let i = 0; i < localStorage.length; i++) {
          const key = localStorage.key(i)
          if (key && key.startsWith(STORAGE_PREFIX)) {
            localStorage.removeItem(key)
          }
        }
      }
      if (fullname)
        localStorage.setItem(AUTH_PREFIX, fullname)
    }

    cleanupStaleQuizzes()
  }
  init()

  function getStorageKey(slug: string): string {
    return `${STORAGE_PREFIX}${slug}`
  }

  function getQuizState(slug: string): PersistedQuizState | null {
    if (typeof window === 'undefined') return null

    const key = getStorageKey(slug)
    const stored = localStorage.getItem(key)

    if (!stored) return null

    try {
      return JSON.parse(stored) as PersistedQuizState
    } catch {
      return null
    }
  }

  function saveQuizState(state: PersistedQuizState): void {
    if (typeof window === 'undefined') return

    const key = getStorageKey(state.slug)
    localStorage.setItem(key, JSON.stringify(state))
  }

  function initQuizState(slug: string, totalQuestions: number, date: string, title?: string): PersistedQuizState {
    const state: PersistedQuizState = {
      slug,
      title,
      date,
      totalQuestions,
      answeredQuestions: 0,
      userAnswers: [],
      answeredQuestionSeqs: [],
      lastUpdated: new Date().toISOString()
    }
    saveQuizState(state)
    return state
  }

  function saveAnswer(slug: string, questionSeq: number, answer: string, questionLastUpdate?: string): void {
    const state = getQuizState(slug)
    if (!state) return

    // Check if this question was already answered
    if (!state.answeredQuestionSeqs.includes(questionSeq)) {
      state.userAnswers.push({ questionSeq, answer, questionLastUpdate })
      state.answeredQuestionSeqs.push(questionSeq)
      state.answeredQuestions = state.userAnswers.length
    } else {
      // Update existing answer
      const index = state.userAnswers.findIndex(a => a.questionSeq === questionSeq)
      if (index !== -1) {
        state.userAnswers[index]!.answer = answer
        state.userAnswers[index]!.questionLastUpdate = questionLastUpdate
      }
    }

    state.lastUpdated = new Date().toISOString()
    saveQuizState(state)
  }

  function clearQuizState(slug: string): void {
    if (typeof window === 'undefined') return

    const key = getStorageKey(slug)
    localStorage.removeItem(key)
  }

  function getUnfinishedQuizzes(): PersistedQuizState[] {
    if (typeof window === 'undefined') return []

    const unfinished: PersistedQuizState[] = []

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && key.startsWith(STORAGE_PREFIX)) {
        const stored = localStorage.getItem(key)
        if (stored) {
          try {
            const state = JSON.parse(stored) as PersistedQuizState
            if (state.answeredQuestions < state.totalQuestions) {
              unfinished.push(state)
            }
          } catch {
            // Skip invalid entries
          }
        }
      }
    }

    // Sort by last updated, most recent first
    return unfinished.sort((a, b) =>
      new Date(b.lastUpdated).getTime() - new Date(a.lastUpdated).getTime()
    )
  }

  function getUnansweredQuestions<T extends { questionSeq: number }>(
    slug: string,
    allQuestions: T[]
  ): T[] {
    const state = getQuizState(slug)
    if (!state) return allQuestions

    // Filter out answered questions
    const unanswered = allQuestions.filter(
      q => !state.answeredQuestionSeqs.includes(q.questionSeq)
    )

    return unanswered
  }

  function getTodayDate(): string {
    const today = new Date()
    const year = today.getFullYear()
    const month = String(today.getMonth() + 1).padStart(2, '0')
    const day = String(today.getDate()).padStart(2, '0')
    return `${year}-${month}-${day}`
  }

  function isToday(dateString: string): boolean {
    return dateString === getTodayDate()
  }

  function cleanupStaleQuizzes(questions?: { seq: number, lastUpdate: string }[]): void {
    if (typeof window === 'undefined') return

    const keysToRemove: string[] = []

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && key.startsWith(STORAGE_PREFIX)) {
        const stored = localStorage.getItem(key)
        if (stored) {
          try {
            const state = JSON.parse(stored) as PersistedQuizState
            // Remove if not today OR if lastUpdate doesn't match
            if (!isToday(state.date)) {
              keysToRemove.push(key)
            }

            const persistedQuestionToRemove: number[] = []
            for (const question of state.userAnswers) {
              const originalQuestion = questions?.find(q => q.seq === question.questionSeq)
              if (originalQuestion && question.questionLastUpdate !== originalQuestion.lastUpdate) {
                persistedQuestionToRemove.push(question.questionSeq)
              }
            }

            if (persistedQuestionToRemove.length > 0) {
              // Remove outdated answers
              state.userAnswers = state.userAnswers.filter(
                a => !persistedQuestionToRemove.includes(a.questionSeq)
              )
              state.answeredQuestionSeqs = state.answeredQuestionSeqs.filter(
                seq => !persistedQuestionToRemove.includes(seq)
              )
              state.answeredQuestions = state.userAnswers.length

              // If no answers left, remove the entire quiz state
              if (state.answeredQuestions === 0) {
                keysToRemove.push(key)
              } else {
                // Otherwise, save the updated state
                saveQuizState(state)
              }
            }
          } catch {
            // Remove invalid entries
            keysToRemove.push(key)
          }
        }
      }
    }

    keysToRemove.forEach(key => localStorage.removeItem(key))
  }

  function validateQuizDate(slug: string): boolean {
    const state = getQuizState(slug)
    if (!state) return true

    // Invalid if not today OR if lastUpdate doesn't match
    if (!isToday(state.date)) {
      clearQuizState(slug)
      return false
    }

    return true
  }

  return {
    getQuizState,
    initQuizState,
    saveAnswer,
    clearQuizState,
    getUnfinishedQuizzes,
    getUnansweredQuestions,
    cleanupStaleQuizzes,
    validateQuizDate,
    getTodayDate,
    init
  }
}

export type { PersistedQuizState, QuizAnswer }
