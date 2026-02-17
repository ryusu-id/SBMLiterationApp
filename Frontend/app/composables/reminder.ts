export function useReminder() {
  // Type definition for Periodic Background Sync
  interface PeriodicSyncManager {
    register: (tag: string, options?: { minInterval: number }) => Promise<void>
    getTags: () => Promise<string[]>
    unregister: (tag: string) => Promise<void>
  }

  interface ServiceWorkerRegistrationWithSync extends ServiceWorkerRegistration {
    periodicSync: PeriodicSyncManager
  }

  const SENT_REMINDERS_KEY = 'sentRemindersToday'
  const REMINDER_TIMES = [
    { hour: 9, minute: 0 },
    { hour: 12, minute: 0 },
    { hour: 15, minute: 0 },
    { hour: 20, minute: 0 }
  ]

  const REMINDER_MESSAGES = [
    'You\'re not behind. You\'re just here when you\'re here. 😊',
    'Progress doesn\'t expire. 📈',
    'Reading waits. No rush. 👍',
    'Showing up late still counts as showing up. 🫶',
    'Some books take time. Some take patience. 🌱',
    'Slow reading is still reading. ☕️',
    'This habit is taking shape. 💪',
    'You\'re becoming your own kind of reader. 🥳',
    'You can keep it simple today. 😎',
    'This isn\'t about finishing books. It\'s about staying curious. 🚀'
  ]

  /**
   * Initialize the reminder system on app launch
   */
  function initializeReminder() {
    if (import.meta.server) return

    // Request notification permission if not already granted
    requestNotificationPermission()

    // Register periodic background sync
    registerPeriodicSync()

    // Set up periodic check for reminders (fallback for foreground)
    setupReminderCheck()
  }

  /**
   * Register periodic background sync for notifications
   */
  async function registerPeriodicSync() {
    if (import.meta.server) return

    try {
      if (!('serviceWorker' in navigator)) {
        return
      }

      const registration = await navigator.serviceWorker.ready

      // Check if periodic sync is supported
      if (!('periodicSync' in registration)) {
        console.warn('Periodic Background Sync not supported')
        return
      }

      const status = await navigator.permissions.query({
        name: 'periodic-background-sync' as PermissionName
      })

      if (status.state === 'granted') {
        // Register periodic sync - minimum interval is usually 12 hours
        // Browser will decide actual interval based on user engagement
        const syncRegistration = registration as ServiceWorkerRegistrationWithSync
        await syncRegistration.periodicSync.register('reading-reminder-check', {
          minInterval: 15 * 60 * 1000 // 15 minutes in milliseconds
        })
        console.log('Periodic background sync registered')
      }
    } catch (error) {
      console.warn('Failed to register periodic sync:', error)
    }
  }

  /**
   * Sync sent reminders to cache for service worker access
   */
  async function syncSentRemindersToCache() {
    if (import.meta.server) return

    try {
      const sentData = {
        date: getTodayString(),
        times: getSentRemindersToday()
      }

      const cache = await caches.open('reminder-cache')
      await cache.put('sent-reminders', new Response(JSON.stringify(sentData)))
    } catch (error) {
      console.error('Failed to sync reminders to cache:', error)
    }
  }

  /**
   * Get a random reminder message
   */
  function getRandomMessage(): string {
    const randomIndex = Math.floor(Math.random() * REMINDER_MESSAGES.length)
    return REMINDER_MESSAGES[randomIndex] || 'Time to read!'
  }

  /**
   * Get today's date string
   */
  function getTodayString(): string {
    return new Date().toDateString()
  }

  /**
   * Get sent reminders for today from localStorage
   */
  function getSentRemindersToday(): string[] {
    if (import.meta.server) return []

    const stored = localStorage.getItem(SENT_REMINDERS_KEY)
    if (!stored) return []

    const data = JSON.parse(stored)
    // Check if it's from today, otherwise reset
    if (data.date !== getTodayString()) {
      return []
    }

    return data.times || []
  }

  /**
   * Mark a specific time as sent for today
   */
  function markReminderAsSent(hour: number, minute: number) {
    if (import.meta.server) return

    const timeString = `${hour}:${minute.toString().padStart(2, '0')}`
    const sentTimes = getSentRemindersToday()

    if (!sentTimes.includes(timeString)) {
      sentTimes.push(timeString)
    }

    localStorage.setItem(SENT_REMINDERS_KEY, JSON.stringify({
      date: getTodayString(),
      times: sentTimes
    }))

    // Sync to cache for service worker
    syncSentRemindersToCache()
  }

  /**
   * Check if reminder for specific time has been sent today
   */
  function isReminderSentForTime(hour: number, minute: number): boolean {
    const timeString = `${hour}:${minute.toString().padStart(2, '0')}`
    const sentTimes = getSentRemindersToday()
    return sentTimes.includes(timeString)
  }

  /**
   * Request notification permission from the user
   */
  async function requestNotificationPermission() {
    if (import.meta.server) return false

    if (!('Notification' in window)) {
      console.warn('This browser does not support notifications')
      return false
    }

    if (Notification.permission === 'granted') {
      return true
    }

    if (Notification.permission !== 'denied') {
      const permission = await Notification.requestPermission()
      return permission === 'granted'
    }

    return false
  }

  /**
   * Check if we should send a reminder now and return the time to send for
   */
  function shouldSendReminder(): { hour: number, minute: number } | null {
    if (import.meta.server) return null

    const now = new Date()
    const currentHour = now.getHours()
    const currentMinute = now.getMinutes()

    // Check each reminder time
    for (const reminderTime of REMINDER_TIMES) {
      // Check if current time is past this reminder time
      const isPastReminderTime
        = currentHour > reminderTime.hour
          || (currentHour === reminderTime.hour && currentMinute >= reminderTime.minute)

      // If we're past the time and haven't sent for this time yet
      if (isPastReminderTime && !isReminderSentForTime(reminderTime.hour, reminderTime.minute)) {
        return reminderTime
      }
    }

    return null
  }

  /**
   * Send a push notification reminder
   */
  async function sendReminderNotification(hour?: number, minute?: number) {
    if (import.meta.server) return

    if (Notification.permission !== 'granted') {
      console.warn('Notification permission not granted')
      return
    }

    const message = getRandomMessage()

    // Check if service worker is available
    if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
      // Send message to service worker to show notification
      navigator.serviceWorker.controller.postMessage({
        type: 'SHOW_REMINDER',
        title: 'SIGMA 📚',
        body: message,
        icon: '/favicon.svg',
        tag: 'reading-reminder',
        data: {
          url: '/'
        }
      })
    } else {
      // Fallback to regular notification if service worker is not available
      new Notification('SIGMA 📚', {
        body: message,
        icon: `${window.location.origin}/favicon.svg`,
        tag: 'reading-reminder',
        data: {
          url: '/'
        }
      })
    }

    // Mark this reminder time as sent
    if (hour !== undefined && minute !== undefined) {
      markReminderAsSent(hour, minute)
    }
  }

  /**
   * Set up periodic check for reminders
   */
  function setupReminderCheck() {
    if (import.meta.server) return

    // Check immediately
    const reminderTime = shouldSendReminder()
    if (reminderTime) {
      sendReminderNotification(reminderTime.hour, reminderTime.minute)
    }

    // Set up interval to check every 15 minutes
    const checkInterval = 15 * 60 * 1000 // 15 minutes
    setInterval(() => {
      const reminderTime = shouldSendReminder()
      if (reminderTime) {
        sendReminderNotification(reminderTime.hour, reminderTime.minute)
      }
    }, checkInterval)
  }

  /**
   * Clear sent reminders (useful for testing or reset)
   */
  async function clearSentReminders() {
    if (import.meta.server) return
    localStorage.removeItem(SENT_REMINDERS_KEY)

    // Also clear from cache
    try {
      const cache = await caches.open('reminder-cache')
      await cache.delete('sent-reminders')
    } catch (error) {
      console.error('Failed to clear cache:', error)
    }
  }

  /**
   * Get time until next reminder in milliseconds
   */
  function getTimeUntilNextReminder(): number | null {
    if (import.meta.server) return null

    const now = new Date()
    let closestTime: Date | null = null

    // Check all reminder times
    for (const reminderTime of REMINDER_TIMES) {
      const targetTime = new Date()
      targetTime.setHours(reminderTime.hour, reminderTime.minute, 0, 0)

      // If this time has passed today, check tomorrow
      if (now > targetTime) {
        targetTime.setDate(targetTime.getDate() + 1)
      }

      // Keep track of the closest upcoming reminder
      if (!closestTime || targetTime < closestTime) {
        closestTime = targetTime
      }
    }

    return closestTime ? closestTime.getTime() - now.getTime() : null
  }

  return {
    initializeReminder,
    getSentRemindersToday,
    requestNotificationPermission,
    registerPeriodicSync,
    shouldSendReminder,
    sendReminderNotification,
    clearSentReminders,
    getTimeUntilNextReminder,
    getRandomMessage,
    syncSentRemindersToCache
  }
}
