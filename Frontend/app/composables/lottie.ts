export const ANIMATION = {
  LEVEL_UP: {
    path: '/lottie/levelup.lottie',
    duration: 1500,
    speed: 1
  },
  STREAK: {
    path: '/lottie/streak.lottie',
    duration: 2300,
    speed: 1
  },
  BOOK_COMPLETE: {
    path: '/lottie/book-complete.lottie',
    duration: 2300,
    speed: 0.5
  }
}

export const useLottie = defineStore('lottie', () => {
  const shouldRender = ref(false)
  const animation = ref<typeof ANIMATION[keyof typeof ANIMATION]['path']>()
  const speed = ref(1.0)

  function showAnimation(src: typeof ANIMATION[keyof typeof ANIMATION]) {
    animation.value = src.path
    speed.value = src.speed
    shouldRender.value = true
    setTimeout(() => {
      shouldRender.value = false
    }, src.duration)
  }

  function hideAnimation() {
    shouldRender.value = false
    animation.value = undefined
  }

  return {
    shouldRender,
    animation,
    showAnimation,
    hideAnimation,
    speed
  }
})
