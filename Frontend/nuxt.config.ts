// load dotenv variables
// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui',
    '@nuxt/image',
    '@nuxt/icon',
    '@pinia/nuxt'
  ],

  plugins: [
    '~/plugins/fetch'
  ],

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    public: {
      backendApiUri: process.env.BACKEND_API_URI || 'http://localhost:8000/api'
    }
  },

  routeRules: {
    '/': { prerender: true },
    '/api/v1/**': {
      proxy: 'http://localhost:8000/api/**'
    }
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
  icon: {
    serverBundle: {
      collections: ['lucide', 'simple-icons']
    }
  }
})
