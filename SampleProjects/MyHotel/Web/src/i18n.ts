import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import sv from './locales/sv.json';

i18n.use(initReactI18next).init({
  lng: 'sv',
  resources: { sv: { translation: sv } },
  interpolation: { escapeValue: false },
  initAsync: false,
});

export default i18n;
