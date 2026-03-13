<script setup lang="ts">
import { $authedFetch } from '~/apis/api'
import DashboardNavbar from '~/components/layout/DashboardNavbar.vue'

definePageMeta({
  layout: 'admin',
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  middleware: ['auth', 'admin-only'] as any[]
})

type PushTestStatus = 'idle' | 'requesting-permission' | 'subscribing' | 'sending' | 'success' | 'error'

type LogEntry = { step: string, ok: boolean, detail?: string }

const status = ref<PushTestStatus>('idle')
const errorMessage = ref<string | null>(null)
const notificationPermission = ref<NotificationPermission>('default')
const title = ref('SIGMA Test Push 🎉')
const body = ref('Push notification is working from the backend!')
const logs = ref<LogEntry[]>([])

// Broadcast state
const subscriberCount = ref<number | null>(null)
const isBroadcasting = ref(false)
const broadcastResult = ref<{ sent: number; failed: number; message: string } | null>(null)

onMounted(async () => {
  if ('Notification' in window) {
    notificationPermission.value = Notification.permission
  }
  await loadSubscriberCount()
})

const statusLabel: Record<PushTestStatus, string> = {
  'idle': 'Ready',
  'requesting-permission': 'Requesting notification permission...',
  'subscribing': 'Subscribing to push service...',
  'sending': 'Sending push via backend...',
  'success': 'Push sent! Check your notifications.',
  'error': 'Something went wrong — see log below.'
}

const isLoading = computed(() =>
  ['requesting-permission', 'subscribing', 'sending'].includes(status.value)
)

function log(step: string, ok: boolean, detail?: string) {
  logs.value.push({ step, ok, detail })
}

async function runPushTest() {
  errorMessage.value = null
  logs.value = []
  status.value = 'idle'

  try {
    // Step 1: Notification permission
    if (!('Notification' in window)) {
      throw new Error('This browser does not support the Notification API.')
    }

    if (Notification.permission !== 'granted') {
      status.value = 'requesting-permission'
      const permission = await Notification.requestPermission()
      notificationPermission.value = permission
      if (permission !== 'granted') {
        throw new Error(`Notification permission: ${permission}. Allow notifications in browser settings.`)
      }
    }
    log('Notification permission', true, Notification.permission)

    // Step 2: Get VAPID public key
    status.value = 'subscribing'
    let vapidPublicKey: string
    try {
      const vapidRes = await $authedFetch<{ data?: { publicKey: string, isConfigured: boolean } }>(
        '/test-items/push-notification/vapid-key'
      )
      if (!vapidRes?.data?.isConfigured || !vapidRes?.data?.publicKey) {
        throw new Error('VAPID keys are not configured in appsettings.json.')
      }
      vapidPublicKey = vapidRes.data.publicKey
      log('Fetch VAPID public key', true, `key: ${vapidPublicKey.substring(0, 20)}...`)
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : (e as { _data?: { errorDescription?: string } })?._data?.errorDescription ?? String(e)
      throw new Error(`Failed to fetch VAPID key: ${msg}`)
    }

    // Step 3: Service worker + push subscribe
    if (!('serviceWorker' in navigator)) {
      throw new Error('ServiceWorker is not available. Make sure the app is on HTTPS.')
    }
    if (!('PushManager' in window)) {
      throw new Error('PushManager is not available in this browser.')
    }

    const reg = await navigator.serviceWorker.ready
    log('Service worker ready', true)

    let subscription: PushSubscription
    try {
      subscription = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: vapidPublicKey
      })
      log('Browser push subscribe', true, subscription.endpoint.substring(0, 40) + '...')
    } catch (e: unknown) {
      throw new Error(`pushManager.subscribe failed: ${e instanceof Error ? e.message : String(e)}`)
    }

    const subJson = subscription.toJSON()

    // Step 4a: Save subscription to DB
    try {
      await $authedFetch('/push/subscribe', {
        method: 'POST',
        body: {
          endpoint: subJson.endpoint,
          p256dh: subJson.keys?.p256dh,
          auth: subJson.keys?.auth,
          userAgent: navigator.userAgent
        }
      })
      log('Saved subscription to DB', true)
      await loadSubscriberCount()
    } catch (e: unknown) {
      // Non-fatal — push still goes through even if DB save fails
      log('Save subscription to DB', false, 'skipped (may already exist)')
    }

    // Step 4b: Call backend to send push
    status.value = 'sending'
    try {
      await $authedFetch('/test-items/push-notification', {
        method: 'POST',
        body: {
          subscription: {
            endpoint: subJson.endpoint,
            p256dh: subJson.keys?.p256dh,
            auth: subJson.keys?.auth
          },
          title: title.value,
          body: body.value
        }
      })
      log('Backend sent push', true)
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : (e as { _data?: { errorDescription?: string } })?._data?.errorDescription ?? String(e)
      throw new Error(`Backend push failed: ${msg}`)
    }

    status.value = 'success'
  } catch (err: unknown) {
    status.value = 'error'
    const message = err instanceof Error ? err.message : String(err)
    errorMessage.value = message
    log('❌ Error', false, message)
  }
}

async function loadSubscriberCount() {
  try {
    const res = await $authedFetch<{ data?: { count: number } }>('/push/subscriber-count')
    subscriberCount.value = res?.data?.count ?? null
  } catch {
    subscriberCount.value = null
  }
}

async function runBroadcast() {
  isBroadcasting.value = true
  broadcastResult.value = null
  try {
    const res = await $authedFetch<{ data?: { sent: number; failed: number; message: string } }>(
      '/test-items/push-notification/broadcast',
      {
        method: 'POST',
        body: {
          title: title.value,
          body: body.value
        }
      }
    )
    broadcastResult.value = res?.data ?? null
    await loadSubscriberCount()
  } catch (e: unknown) {
    const msg = e instanceof Error ? e.message : String(e)
    broadcastResult.value = { sent: 0, failed: 0, message: `Broadcast failed: ${msg}` }
  } finally {
    isBroadcasting.value = false
  }
}
</script>

<template>
  <UDashboardPanel>
    <template #header>
      <DashboardNavbar title="Push Notification Test" />
    </template>

    <template #body>
      <div class="max-w-lg mx-auto mt-8 flex flex-col gap-6">
        <!-- Info card -->
        <UCard>
          <template #header>
            <div class="flex items-center gap-2">
              <UIcon
                name="i-lucide-bell-ring"
                class="text-primary-500 size-5"
              />
              <span class="font-semibold">Web Push Test</span>
            </div>
          </template>

          <p class="text-sm text-muted mb-4">
            This page tests the full Web Push (VAPID) flow end-to-end.
            Clicking the button will:
          </p>
          <ol class="text-sm text-muted list-decimal list-inside space-y-1">
            <li>Request notification permission from your browser</li>
            <li>Fetch the VAPID public key from the backend</li>
            <li>Subscribe this browser to push</li>
            <li>Tell the backend to send a push notification to you</li>
          </ol>
        </UCard>

        <!-- Notification permission badge -->
        <div class="flex items-center gap-2">
          <span class="text-sm text-muted">Notification permission:</span>
          <UBadge
            :color="notificationPermission === 'granted' ? 'success' : notificationPermission === 'denied' ? 'error' : 'warning'"
            variant="subtle"
          >
            {{ notificationPermission }}
          </UBadge>
          <UBadge
            v-if="notificationPermission === 'denied'"
            color="error"
            variant="soft"
          >
            Go to browser settings to re-allow
          </UBadge>
        </div>

        <!-- Message inputs -->
        <UCard>
          <template #header>
            <span class="font-semibold text-sm">Notification message</span>
          </template>
          <div class="flex flex-col gap-3">
            <UFormField label="Title">
              <UInput
                v-model="title"
                placeholder="Notification title"
                class="w-full"
              />
            </UFormField>
            <UFormField label="Body">
              <UTextarea
                v-model="body"
                placeholder="Notification body"
                :rows="2"
                class="w-full"
              />
            </UFormField>
          </div>
        </UCard>

        <!-- Action button -->
        <UButton
          type="button"
          :loading="isLoading"
          :disabled="isLoading || notificationPermission === 'denied'"
          icon="i-lucide-send"
          size="lg"
          class="w-full justify-center"
          @click="runPushTest"
        >
          {{ isLoading ? statusLabel[status] : 'Subscribe & Send Test Push (to me only)' }}
        </UButton>

        <!-- Broadcast section -->
        <UCard>
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2">
                <UIcon
                  name="i-lucide-radio"
                  class="text-primary-500 size-5"
                />
                <span class="font-semibold text-sm">Broadcast to All Subscribers</span>
              </div>
              <div class="flex items-center gap-2 text-sm text-muted">
                <UIcon
                  name="i-lucide-users"
                  class="size-4"
                />
                <span v-if="subscriberCount !== null">
                  {{ subscriberCount }} subscriber{{ subscriberCount === 1 ? '' : 's' }}
                </span>
                <span v-else>Loading...</span>
                <UButton
                  type="button"
                  icon="i-lucide-refresh-cw"
                  size="xs"
                  variant="ghost"
                  @click="loadSubscriberCount"
                />
              </div>
            </div>
          </template>

          <p class="text-sm text-muted mb-4">
            Sends the notification above to <strong>all users</strong> who have subscribed via push.
            Users auto-subscribe when they visit the app with notifications granted.
          </p>

          <UButton
            type="button"
            :loading="isBroadcasting"
            :disabled="isBroadcasting || (subscriberCount !== null && subscriberCount === 0)"
            icon="i-lucide-megaphone"
            color="warning"
            size="md"
            class="w-full justify-center"
            @click="runBroadcast"
          >
            {{ isBroadcasting ? 'Broadcasting...' : 'Broadcast Push Notification' }}
          </UButton>

          <!-- Broadcast result -->
          <div
            v-if="broadcastResult"
            class="mt-4"
          >
            <UAlert
              :color="broadcastResult.failed === 0 ? 'success' : broadcastResult.sent > 0 ? 'warning' : 'error'"
              :icon="broadcastResult.failed === 0 ? 'i-lucide-circle-check' : 'i-lucide-triangle-alert'"
              :title="broadcastResult.failed === 0 ? 'Broadcast complete!' : 'Broadcast partial'"
              :description="broadcastResult.message"
            />
            <div class="flex gap-4 mt-2 text-sm">
              <span class="text-success-500">
                <UIcon name="i-lucide-check" /> Sent: {{ broadcastResult.sent }}
              </span>
              <span
                v-if="broadcastResult.failed > 0"
                class="text-error-500"
              >
                <UIcon name="i-lucide-x" /> Failed/Expired: {{ broadcastResult.failed }}
              </span>
            </div>
          </div>
        </UCard>

        <!-- Step-by-step debug log -->
        <UCard v-if="logs.length > 0">
          <template #header>
            <span class="font-semibold text-sm">Debug log</span>
          </template>
          <ul class="space-y-1">
            <li
              v-for="(entry, i) in logs"
              :key="i"
              class="text-xs flex items-start gap-2"
            >
              <UIcon
                :name="entry.ok ? 'i-lucide-circle-check' : 'i-lucide-circle-x'"
                :class="entry.ok ? 'text-success-500' : 'text-error-500'"
                class="size-4 mt-0.5 shrink-0"
              />
              <span>
                <span class="font-medium">{{ entry.step }}</span>
                <span
                  v-if="entry.detail"
                  class="text-muted ml-1"
                >— {{ entry.detail }}</span>
              </span>
            </li>
          </ul>
        </UCard>

        <!-- Status feedback -->
        <UAlert
          v-if="status === 'success'"
          color="success"
          icon="i-lucide-circle-check"
          title="Push delivered"
          description="The backend sent the push notification. If you don't see it, check that notifications are not silenced on your device."
        />
        <UAlert
          v-else-if="status === 'error' && errorMessage"
          color="error"
          icon="i-lucide-triangle-alert"
          title="Failed"
          :description="errorMessage"
        />
      </div>
    </template>
  </UDashboardPanel>
</template>
