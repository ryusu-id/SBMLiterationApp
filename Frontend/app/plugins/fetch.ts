import { defineNuxtPlugin } from '#app'
import { useAuth } from '~/apis/api'

export default defineNuxtPlugin(() => {
  const config = useRuntimeConfig()

  // Use different URI for server-side vs client-side
  const baseURL = import.meta.server
    ? (config.backendApiUri as string || config.public.backendApiUri as string)
    : (config.public.backendApiUri as string)

  const api = $fetch.create({
    baseURL
  })

  const authedApi = $fetch.create({
    baseURL,
    async onRequest({ options }) {
      const authStore = useAuth()
      const token = authStore.getToken()
      if (token) {
        options.headers = options.headers || {}
        options.headers.set('Authorization', `Bearer ${token}`)
      }
    },
    async onResponseError({ request, options, response }): Promise<void> {
      const authStore = useAuth()

      // Only handle 401 errors and avoid infinite retry loops
      if (response.status !== 401 || options.retry === false) {
        return
      }

      // Try to refresh the token
      const refreshed = await authStore.requestRefreshToken()

      if (refreshed) {
        // Update the Authorization header with the new token
        options.headers = options.headers || {}
        options.headers.set('Authorization', `Bearer ${authStore.getToken()}`)

        // Prevent infinite retry loop
        options.retry = false

        // Retry the request with the new token
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        await authedApi(request, options as any)
        return
      }

      // If refresh failed, clear tokens and let the error propagate
      authStore.clearToken()
      // Don't navigate here - let the calling code handle it
    }
  })

  function useAuthedFetch<T>(
    request: Parameters<typeof useFetch<T>>[0],
    opts?: Parameters<typeof useFetch<T>>[1]
  ) {
    return useFetch(request, {
      ...opts,
      $fetch: useNuxtApp().$authedApi as typeof $fetch
    })
  }
  // Expose to useNuxtApp().$api
  return {
    provide: {
      backendApi: api,
      authedApi,
      useAuthedFetch
    }
  }
})
