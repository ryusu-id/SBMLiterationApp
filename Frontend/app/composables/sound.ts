export const useSound = defineStore('sound', () => {
  const audioCache = new Map<string, HTMLAudioElement>()
  const AUDIO_CONSTANT = {
    bookComplete: '/sounds/book-complete.wav',
    report: '/sounds/report.wav',
    streak: '/sounds/streak.wav'
  }

  const lottie = useLottie()

  const preloadSound = (soundPath: string): Promise<void> => {
    return new Promise((resolve, reject) => {
      if (audioCache.has(soundPath)) {
        resolve()
        return
      }

      const audio = new Audio(soundPath)
      audio.preload = 'auto'

      audio.addEventListener('canplaythrough', () => {
        audioCache.set(soundPath, audio)
        resolve()
      }, { once: true })

      audio.addEventListener('error', () => {
        reject(new Error(`Failed to load sound: ${soundPath}`))
      }, { once: true })

      audio.load()
    })
  }

  // eslint-disable-next-line @typescript-eslint/no-invalid-void-type
  const preloadSounds = (soundPaths: string[]): Promise<void[]> => {
    return Promise.all(soundPaths.map(preloadSound))
  }

  const playSound = (soundPath: string, volume = 1.0) => {
    const audio = audioCache.get(soundPath)
    if (!audio) {
      console.warn(`Sound not preloaded: ${soundPath}`)
      return
    }

    // Clone the audio to allow overlapping sounds
    const clone = audio.cloneNode(true) as HTMLAudioElement
    clone.volume = volume
    clone.play().catch(err => console.error('Error playing sound:', err))
  }

  function playBookCompleteSound() {
    playSound(AUDIO_CONSTANT.bookComplete)
    setTimeout(() => {
      lottie.showAnimation(ANIMATION.BOOK_COMPLETE)
    }, 800)
  }

  function playReportSound() {
    playSound(AUDIO_CONSTANT.report)
    lottie.showAnimation(ANIMATION.LEVEL_UP)
  }

  function playStreakSound() {
    playSound(AUDIO_CONSTANT.streak)
    lottie.showAnimation(ANIMATION.STREAK)
  }

  return {
    preloadSound,
    preloadSounds,
    playSound,
    AUDIO_CONSTANT,
    playBookCompleteSound,
    playReportSound,
    playStreakSound
  }
})
