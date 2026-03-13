/**
 * usePushNotification
 *
 * Manages browser push subscription lifecycle:
 *  - autoSubscribe()  — silently subscribe after login; swallows errors
 *  - unsubscribe()    — remove subscription from browser + backend
 *
 * The composable does NOT manage UI state — call it from app.vue (auto)
 * or from pages that need manual subscription control.
 */
import { $authedFetch } from '~/apis/api'

function urlBase64ToUint8Array(base64String: string): Uint8Array<ArrayBuffer> {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4)
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/')
  const rawData = atob(base64)
  const bytes = new Uint8Array(rawData.length)
  for (let i = 0; i < rawData.length; i++) {
    bytes[i] = rawData.charCodeAt(i)
  }
  return bytes
}

export const usePushNotification = () => {
  /**
   * Silently subscribe this browser to push and save to backend.
   * Called once on app mount after the user is logged in.
   * All errors are swallowed — this must never break app startup.
   */
  async function autoSubscribe(): Promise<void> {
    if (import.meta.server) return
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return
    if (!('Notification' in window)) return

    // Only proceed if permission is already granted — don't prompt automatically
    if (Notification.permission !== 'granted') return

    try {
      // Get VAPID public key from backend
      const vapidRes = await $authedFetch<{
        data?: { publicKey: string, isConfigured: boolean }
      }>('/test-items/push-notification/vapid-key').catch(() => null)

      if (!vapidRes?.data?.isConfigured || !vapidRes?.data?.publicKey) return

      const vapidPublicKey = vapidRes.data.publicKey
      const applicationServerKey = urlBase64ToUint8Array(vapidPublicKey)

      const reg = await navigator.serviceWorker.ready
      const existing = await reg.pushManager.getSubscription()

      let subscription: PushSubscription
      if (existing) {
        subscription = existing
      } else {
        subscription = await reg.pushManager.subscribe({
          userVisibleOnly: true,
          applicationServerKey
        })
      }

      const subJson = subscription.toJSON()
      if (!subJson.endpoint || !subJson.keys?.p256dh || !subJson.keys?.auth) return

      await $authedFetch('/push/subscribe', {
        method: 'POST',
        body: {
          endpoint: subJson.endpoint,
          p256dh: subJson.keys.p256dh,
          auth: subJson.keys.auth,
          userAgent: navigator.userAgent
        }
      }).catch(() => {
        // Silently swallow — it may already be saved
      })
    } catch {
      // Silent — never crash the app over push notification setup
    }
  }

  /**
   * Unsubscribe from push in the browser and notify the backend.
   * Call this on logout or when the user explicitly opts out.
   */
  async function unsubscribe(): Promise<void> {
    if (import.meta.server) return
    if (!('serviceWorker' in navigator)) return

    try {
      const reg = await navigator.serviceWorker.ready
      const sub = await reg.pushManager.getSubscription()
      if (!sub) return

      const endpoint = sub.endpoint
      await sub.unsubscribe()

      await $authedFetch('/push/unsubscribe', {
        method: 'DELETE',
        body: { endpoint }
      }).catch(() => {})
    } catch {
      // Silent
    }
  }

  return { autoSubscribe, unsubscribe }
}
