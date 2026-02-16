// load dotenv variables
// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui',
    '@nuxt/image',
    '@nuxt/icon',
    '@pinia/nuxt',
    '@nuxtjs/google-fonts',
    'nuxt-icons',
    '@vite-pwa/nuxt'
  ],

  plugins: ['~/plugins/fetch', '~/plugins/audio.client'],
  $development: {
    runtimeConfig: {
      backendApiUri: 'http://10.8.0.3:8000/api',
      // backendApiUri: 'https://api.staging.ryusu.id/api',
      public: {
        backendApiUri: 'http://10.8.0.3:8000/api'
        // backendApiUri: 'https://api.staging.ryusu.id/api'
      }
    }
  },
  $production: {
    runtimeConfig: {
      // backendApiUri: 'https://api.staging.ryusu.id/api',
      backendApiUri: 'http://puretco-app-service/api',
      public: {
        backendApiUri: 'https://api.staging.ryusu.id/api'
      }
    }
  },

  devtools: {
    enabled: true
  },

  app: {
    head: {
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'icon', type: 'image/png', href: '/favicon-96x96.png', sizes: '96x96' },
        { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' },
        { rel: 'shortcut icon', href: '/favicon.ico' },
        { rel: 'apple-touch-icon', sizes: '180x180', href: '/apple-touch-icon.png' }
      ],
      title: 'SIGMA'
    }
  },

  css: ['~/assets/css/main.css'],

  devServer: {
    host: '0.0.0.0',
    port: 3000
  },

  compatibilityDate: '2025-01-15',

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  },

  googleFonts: {
    families: {
      Poppins: [300, 400, 500, 600, 700]
    },
    display: 'swap'
  },
  icon: {
    serverBundle: {
      collections: ['lucide', 'simple-icons']
    }
  },
  pwa: {
    manifest: {
      id: 'com.pure-tco.sigma',
      short_name: 'SIGMA',
      name: 'SIGMA',
      lang: 'en',
      display: 'standalone',
      orientation: 'portrait',
      background_color: '#ffffff',
      icons: [
        {
          src: '/web-app-manifest-192x192.png',
          sizes: '192x192',
          type: 'image/png',
          purpose: 'maskable'
        },
        {
          src: '/web-app-manifest-512x512.png',
          sizes: '512x512',
          type: 'image/png',
          purpose: 'maskable'
        }
      ]
    },
    workbox: {
      importScripts: ['/sw-custom.js'],
      additionalManifestEntries: [
        { url: '/lottie/book-complete.lottie', revision: '2' },
        { url: '/lottie/levelup.lottie', revision: '1' },
        { url: '/lottie/streak.lottie', revision: '1' }
      ]
    },
    client: {
      installPrompt: true,
      periodicSyncForUpdates: 15 * 60 * 1000 // Check for every 15 minutes
    }
    // devOptions: {
    //   enabled: true,
    //   type: 'module'
    // }
  }
})
