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
    'nuxt-icons'
  ],

  plugins: ['~/plugins/fetch'],

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    public: {
      //backendApiUri: 'https://api.staging.ryusu.id/api'
      backendApiUri: 'http://localhost:8000/api'
    }
  },

  routeRules: {
    '/': { prerender: true }
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
  }
})
