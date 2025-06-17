import { createI18n } from 'vue-i18n'
import en from './en'
import it from './it'

export default createI18n({
  legacy: false,
  locale: 'en',
  fallbackLocale: 'en',
  messages: {
    en,
    it
  }
}) 