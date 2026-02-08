import { useAuth } from '~/apis/api'

interface PersistedReadingReportState {
  readingResourceId: number
  slug: string
  title: string
  currentPage: number
  maxPage: number
  latestPageProgress: number
  timeSpent: number
  insight: string
  lastUpdated: string
}

export function usePersistedReadingReport() {
  const STORAGE_PREFIX = 'reading-report-'
  const AUTH_PREFIX = 'auth-reading-report-user-full-name'

  function init() {
    if (import.meta.client) {
      const auth = useAuth()
      const fullname = auth.getFullname()
      if (fullname && localStorage.getItem(AUTH_PREFIX) !== fullname) {
        // Clear all reading data if user has changed
        for (let i = 0; i < localStorage.length; i++) {
          const key = localStorage.key(i)
          if (key && key.startsWith(STORAGE_PREFIX)) {
            localStorage.removeItem(key)
          }
        }
        localStorage.setItem(AUTH_PREFIX, fullname)
      }
      if (fullname)
        localStorage.setItem(AUTH_PREFIX, fullname)
    }
  }
  init()

  function getStorageKey(readingResourceId: number | string): string {
    return `${STORAGE_PREFIX}${readingResourceId}`
  }

  function getReportState(readingResourceId: number): PersistedReadingReportState | null {
    if (typeof window === 'undefined') return null

    const key = getStorageKey(readingResourceId)
    const stored = localStorage.getItem(key)

    if (!stored) return null

    try {
      return JSON.parse(stored) as PersistedReadingReportState
    } catch {
      return null
    }
  }

  function saveReportState(state: PersistedReadingReportState): void {
    if (typeof window === 'undefined') return

    const key = getStorageKey(state.readingResourceId)
    localStorage.setItem(key, JSON.stringify(state))
  }

  function initReportState(
    readingResourceId: number,
    slug: string,
    title: string,
    maxPage: number,
    latestPageProgress: number
  ): PersistedReadingReportState {
    const state: PersistedReadingReportState = {
      readingResourceId,
      slug,
      title,
      currentPage: latestPageProgress,
      maxPage,
      latestPageProgress,
      timeSpent: 0,
      insight: '',
      lastUpdated: new Date().toISOString()
    }
    saveReportState(state)
    return state
  }

  function updateReportState(
    readingResourceId: number,
    updates: Partial<Pick<PersistedReadingReportState, 'currentPage' | 'timeSpent' | 'insight'>>
  ): void {
    const state = getReportState(readingResourceId)
    if (!state) return

    if (updates.currentPage !== undefined) state.currentPage = updates.currentPage
    if (updates.timeSpent !== undefined) state.timeSpent = updates.timeSpent
    if (updates.insight !== undefined) state.insight = updates.insight

    state.lastUpdated = new Date().toISOString()
    saveReportState(state)
  }

  function clearReportState(readingResourceId: number | string): void {
    if (typeof window === 'undefined') return

    const key = getStorageKey(readingResourceId)
    localStorage.removeItem(key)
  }

  function getUnfinishedReports(): PersistedReadingReportState[] {
    if (typeof window === 'undefined') return []

    const unfinished: PersistedReadingReportState[] = []

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && key.startsWith(STORAGE_PREFIX)) {
        const stored = localStorage.getItem(key)
        if (stored) {
          try {
            const state = JSON.parse(stored) as PersistedReadingReportState
            // Include report if it has some data (insight or time spent)
            if (state.insight.length > 0 || state.timeSpent > 0) {
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

  function clearAllReports(): void {
    if (typeof window === 'undefined') return

    const keysToRemove: string[] = []

    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key && key.startsWith(STORAGE_PREFIX)) {
        keysToRemove.push(key)
      }
    }

    keysToRemove.forEach(key => localStorage.removeItem(key))
  }

  return {
    getReportState,
    initReportState,
    updateReportState,
    clearReportState,
    getUnfinishedReports,
    clearAllReports,
    init
  }
}

export type { PersistedReadingReportState }
