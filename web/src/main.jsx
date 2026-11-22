import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import { AuthProvider } from './auth'
import { I18nProvider } from './i18n'
import './styles/base.css'
import './styles/booking.css'
import './styles/visits.css'
import './styles/staff.css'
import './styles/evening.css'
import './styles/mobile.css'
import './styles/extra.css'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <I18nProvider>
        <AuthProvider>
          <App />
        </AuthProvider>
      </I18nProvider>
    </BrowserRouter>
  </StrictMode>,
)
