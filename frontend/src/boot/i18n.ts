import { boot } from 'quasar/wrappers'
import { createI18n } from 'vue-i18n'
import en from 'src/i18n/en'
import it from 'src/i18n/it'

const i18n = createI18n({
  legacy: false,
  locale: 'en',
  fallbackLocale: 'en',
  messages: {
    en,
    it
  }
})

export default boot(({ app }) => {
  app.use(i18n)
})

export { i18n } 