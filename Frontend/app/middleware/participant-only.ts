import { useAuth } from '~/apis/api'

export default defineNuxtRouteMiddleware(() => {
  const auth = useAuth()

  if (!auth.getRoles().includes('admin')) {
    return
  }

  return navigateTo('/admin')
})
