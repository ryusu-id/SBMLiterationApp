const STARRY_NIGHT_KEY = 'starry_night_enable'
export const useStarryNight = defineStore('starryNight', () => {
  const enabled = ref(false)
  function init() {
    const saved = localStorage.getItem(STARRY_NIGHT_KEY)
    if (saved !== null) {
      enabled.value = saved === 'true'
    }
  }
  if (import.meta.client) {
    watch(
      enabled,
      (val) => {
        localStorage.setItem(STARRY_NIGHT_KEY, val.toString())
      }
    )
  }

  return {
    enabled,
    init
  }
})
