import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'
import { useI18n } from '../i18n'

/**
 * Parooli loomine lingi kaudu. Kui administraator lõi kliendile konto (broneering telefoni teel),
 * saab klient e-kirja/SMS-iga lingi, loob siin parooli ja näeb kohe oma broneeringuid.
 */
export default function SetPassword() {
  const { token } = useParams()
  const { t } = useI18n()
  const { signIn } = useAuth()
  const navigate = useNavigate()
  const [info, setInfo] = useState(null)
  const [password, setPassword] = useState('')
  const [repeat, setRepeat] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  useEffect(() => {
    api(`/api/auth/password/${token}`).then(setInfo).catch((e) => setError(e.message))
  }, [token])

  async function save(e) {
    e.preventDefault()
    setError('')
    if (password.length < 6) return setError(t('pw.short'))
    if (password !== repeat) return setError(t('pw.mismatch'))
    setBusy(true)
    try {
      signIn(await api('/api/auth/password', { method: 'POST', body: { token, password } }))
      navigate('/minu')
    } catch (err) {
      setError(err.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth">
      <div className="container narrow">
        <form className="auth-card" onSubmit={save} noValidate>
          <div className="head">
            <span className="diamond" aria-hidden="true" />
            <h1>{t('pw.title')}</h1>
          </div>
          {!info && !error && <p className="loading">{t('common.loading')}</p>}
          {info && (
            <>
              <p className="muted">{t('pw.lead', { name: info.name })}</p>
              <p className="small">E-mail: <strong>{info.email}</strong></p>
              <label className="field">{t('pw.password')}<input type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} /></label>
              <label className="field">{t('pw.repeat')}<input type="password" autoComplete="new-password" value={repeat} onChange={(e) => setRepeat(e.target.value)} /></label>
            </>
          )}
          {error && <p className="alert alert-error">{error}</p>}
          {info && <button className="btn btn-dark btn-pill btn-big btn-block" disabled={busy}>{t('pw.save')}</button>}
        </form>
      </div>
    </div>
  )
}
