export const usePWA = () => {
  const isPWA = () => {
    // Check if running in standalone mode
    if (window.matchMedia('(display-mode: standalone)').matches) {
      return true
    }

    // Check iOS standalone mode
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    if ((window.navigator as any).standalone === true) {
      return true
    }

    // Check if document.referrer is the app's start_url (Android)
    if (document.referrer.includes('android-app://')) {
      return true
    }

    return false
  }

  const isStandalone = computed(() => {
    if (import.meta.server) return false
    return isPWA()
  })

  const displayMode = () => {
    if (import.meta.server) return 'browser'

    if (window.matchMedia('(display-mode: standalone)').matches) {
      return 'standalone'
    }
    if (window.matchMedia('(display-mode: fullscreen)').matches) {
      return 'fullscreen'
    }
    if (window.matchMedia('(display-mode: minimal-ui)').matches) {
      return 'minimal-ui'
    }
    if (window.matchMedia('(display-mode: window-controls-overlay)').matches) {
      return 'window-controls-overlay'
    }
    return 'browser'
  }

  return {
    isPWA: isStandalone,
    displayMode: computed(() => displayMode())
  }
}
