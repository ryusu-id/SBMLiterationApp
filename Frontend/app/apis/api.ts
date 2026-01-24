export interface ApiResponse<T = never> {
  errorCode?: string
  errorDescription?: string
  data?: T
  message: string
}

export function $authedFetch<T>(
  request: Parameters<typeof $fetch<T>>[0],
  opts?: Parameters<typeof $fetch<T>>[1]
): ReturnType<typeof $fetch<T>> {
  const authStore = useAuth()
  const api = useNuxtApp().$backendApi
  let triedRefresh = false

  return new Promise((resolve, reject) => {
    api<T>(request, {
      ...opts,
      headers: {
        Authorization: `Bearer ${authStore.getToken()}`
      },
      onResponse(param) {
        if (triedRefresh && param.response.status === 401)
          reject(param)

        if (param.response.status >= 200 && param.response.status < 300)
          resolve(param.response._data)
      },
      async onResponseError({ response }) {
        if (triedRefresh && response.status === 401)
          reject(response)

        triedRefresh = true
        const refreshed = await authStore.requestRefreshToken()
        console.log(refreshed)
        if (refreshed) {
          await api<T>(request, {
            ...opts,
            headers: {
              Authorization: `Bearer ${authStore.getToken()}`
            },
            onResponse(param) {
              resolve(param.response._data)
            },
            onResponseError(param) {
              reject(param)
            }
          })
        } else {
          authStore.clearToken()
          useRouter().push('/')
        }
      }
    })
  })
}

export const useAuth = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const refreshToken = ref<string | null>(null)

  // Load token from localStorage on initialization
  if (import.meta.client) {
    token.value = localStorage.getItem('auth_token')
    refreshToken.value = localStorage.getItem('refresh_token')
  }

  function getToken() {
    if (!token.value && import.meta.client) {
      token.value = localStorage.getItem('auth_token')
    }
    return token.value
  }

  function getRefreshToken() {
    if (!refreshToken.value && import.meta.client) {
      refreshToken.value = localStorage.getItem('refresh_token')
    }
    return refreshToken.value
  }

  function setToken(newToken: string) {
    token.value = newToken
    if (import.meta.client) {
      localStorage.setItem('auth_token', newToken)
    }
  }

  function setRefreshToken(newRefreshToken: string) {
    refreshToken.value = newRefreshToken
    if (import.meta.client) {
      localStorage.setItem('refresh_token', newRefreshToken)
    }
  }

  async function requestRefreshToken() {
    const $api = useNuxtApp().$backendApi as typeof $fetch
    try {
      const result = await $api<{ accessToken: string, refreshToken: string }>('/auth/refresh', {
        method: 'POST',
        body: {
          accessToken: getToken(),
          refreshToken: getRefreshToken()
        }
      })

      if (result.accessToken && result.refreshToken) {
        setToken(result.accessToken)
        setRefreshToken(result.refreshToken)
        return true
      }
      return false
    } catch {
      return false
    }
  }

  function clearToken() {
    token.value = null
    refreshToken.value = null
    if (import.meta.client) {
      localStorage.removeItem('auth_token')
      localStorage.removeItem('refresh_token')
    }
  }

  function parseJwt(token: string) {
    const payload = token.split('.')[1]
    if (payload === undefined) return null
    const decodedPayload = atob(payload)
    return JSON.parse(decodedPayload)
  }

  function getRoles() {
    const jwtClaims = token.value ? parseJwt(token.value) : null

    if ('role' in jwtClaims) {
      return jwtClaims['role'] as string[]
    }
    return []
  }

  return {
    getToken,
    getRefreshToken,
    setToken,
    clearToken,
    requestRefreshToken,
    setRefreshToken,
    refreshToken,
    token,
    getRoles
  }
})

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function handleResponseError(error: any) {
  const toast = useToast()
  const statusCode = error?.response?.status || error?.statusCode
  const description
    = error?.response?._data?.message
      || error?.response?._data?.error
      || error?.message
      || 'An unknown error occurred'

  toast.add({
    title: statusCode ? `Error ${statusCode}` : 'Error',
    description,
    color: 'error',
    icon: 'i-lucide-triangle-alert'
  })
}
