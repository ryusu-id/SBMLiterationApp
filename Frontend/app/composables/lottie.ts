export const ANIMATION = {
  LEVEL_UP: {
    path: '/lottie/levelup.lottie',
    duration: 1500
  },
  STREAK: {
    path: '/lottie/streak.lottie',
    duration: 2300
  },
  BOOK_COMPLETE: {
    path: '/lottie/book-complete.lottie',
    duration: 4300
  }
}

export const useLottie = defineStore('lottie', () => {
  const shouldRender = ref(false)
  const animation = ref<typeof ANIMATION[keyof typeof ANIMATION]['path']>()

  function showAnimation(src: typeof ANIMATION[keyof typeof ANIMATION]) {
    animation.value = src.path
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
    hideAnimation
  }
})
