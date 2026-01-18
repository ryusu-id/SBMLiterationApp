import { defineNuxtPlugin } from '#app'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()

  const api = $fetch.create({
    baseURL: config.public.backendApiUri as string
  })

  // Expose to useNuxtApp().$api
  return {
    provide: {
      backendApi: api
    }
  }
})
