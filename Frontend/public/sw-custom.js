// Custom service worker code for handling reminder notifications

self.addEventListener('message', (event) => {
  if (event.data && event.data.type === 'SHOW_REMINDER') {
    const { title, body, icon, tag, data } = event.data
    
    self.registration.showNotification(title, {
      body,
      icon: icon ? `${self.location.origin}${icon}` : undefined,
      tag,
      data,
      requireInteraction: false,
      vibrate: [200, 100, 200],
      actions: [
        {
          action: 'open',
          title: 'Open App'
        },
        {
          action: 'dismiss',
          title: 'Dismiss'
        }
      ]
    })
  }
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  
  if (event.action === 'open' || !event.action) {
    const urlToOpen = event.notification.data?.url || '/'
    
    event.waitUntil(
      clients.matchAll({ type: 'window', includeUncontrolled: true })
        .then((clientList) => {
          // Check if there's already a window open
          for (const client of clientList) {
            if (client.url === urlToOpen && 'focus' in client) {
              return client.focus()
            }
          }
          // If no window is open, open a new one
          if (clients.openWindow) {
            return clients.openWindow(urlToOpen)
          }
        })
    )
  }
  // If action is 'dismiss', just close the notification (already done above)
})

self.addEventListener('push', (event) => {
  // Handle push notifications from server if needed in the future
  if (event.data) {
    const data = event.data.json()
    
    const options = {
      body: data.body || 'You have a new notification',
      badge: `${self.location.origin}${data.badge || '/favicon-96x96.png'}`,
      tag: data.tag || 'general-notification',
      data: data.data || {},
      requireInteraction: false,
      vibrate: [200, 100, 200]
    }
    
    event.waitUntil(
      self.registration.showNotification(data.title || 'SIGMA', options)
    )
  }
})

// Periodic Background Sync for reminder notifications
self.addEventListener('periodicsync', (event) => {
  if (event.tag === 'reading-reminder-check') {
    event.waitUntil(checkAndSendReminders())
  }
})

// Check if any reminders need to be sent
async function checkAndSendReminders() {
  try {
    const REMINDER_TIMES = [
      { hour: 9, minute: 0 },
      { hour: 12, minute: 0 },
      { hour: 15, minute: 0 },
      { hour: 20, minute: 0 },
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

    // Get cached data from IndexedDB or use storage API
    const cache = await caches.open('reminder-cache')
    const sentRemindersResponse = await cache.match('sent-reminders')
    let sentData = { date: '', times: [] }
    
    if (sentRemindersResponse) {
      sentData = await sentRemindersResponse.json()
    }

    const today = new Date().toDateString()
    const now = new Date()
    const currentHour = now.getHours()
    const currentMinute = now.getMinutes()

    // Reset if it's a new day
    if (sentData.date !== today) {
      sentData = { date: today, times: [] }
    }

    // Check each reminder time
    for (const reminderTime of REMINDER_TIMES) {
      const timeString = `${reminderTime.hour}:${reminderTime.minute.toString().padStart(2, '0')}`
      
      // Check if we're past this time and haven't sent it yet
      const isPastTime = currentHour > reminderTime.hour 
        || (currentHour === reminderTime.hour && currentMinute >= reminderTime.minute)
      
      if (isPastTime && !sentData.times.includes(timeString)) {
        // Send notification
        const randomIndex = Math.floor(Math.random() * REMINDER_MESSAGES.length)
        const message = REMINDER_MESSAGES[randomIndex]
        
        await self.registration.showNotification('SIGMA 📚', {
          body: message,
          badge: `${self.location.origin}/favicon-96x96.png`,
          tag: 'reading-reminder',
          requireInteraction: false,
          vibrate: [200, 100, 200],
          data: { url: '/' }
        })

        // Mark as sent
        sentData.times.push(timeString)
      }
    }

    // Save updated sent data
    await cache.put('sent-reminders', new Response(JSON.stringify(sentData)))
    
  } catch (error) {
    console.error('Error in checkAndSendReminders:', error)
  }
}
