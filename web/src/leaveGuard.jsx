import { useEffect, useState } from 'react'
import { createPortal } from 'react-dom'
import { useNavigate } from 'react-router-dom'
import { useI18n } from './i18n'

/**
 * Hoiatus, kui lehelt lahkutakse saatmata tekstiga või külalise vestlusega.
 * Saidi linkidel (menüü, logo jne) avaneb saidi stiilis aken „Lahkuda lehelt?“;
 * brauseri sulgemisel/uuendamisel näitab brauser oma hoiatust (beforeunload – selle teksti brauser muuta ei luba).
 * Tagastab akna, mille leht ise kuvab: const leaveDialog = useLeaveWarning(...); … {leaveDialog}
 */
export function useLeaveWarning(active, message) {
  const { t } = useI18n()
  const navigate = useNavigate()
  const [target, setTarget] = useState(null)

  useEffect(() => {
    if (!active) return undefined

    const onUnload = (e) => {
      e.preventDefault()
      e.returnValue = ''
    }
    const onClick = (e) => {
      const a = e.target.closest?.('a[href]')
      if (!a || a.target === '_blank' || e.defaultPrevented || e.button !== 0 || e.ctrlKey || e.metaKey || e.shiftKey) return
      const url = new URL(a.href, window.location.href)
      if (url.origin !== window.location.origin || url.pathname === window.location.pathname) return
      e.preventDefault()
      e.stopPropagation()
      setTarget(url.pathname + url.search + url.hash)
    }

    window.addEventListener('beforeunload', onUnload)
    document.addEventListener('click', onClick, true)
    return () => {
      window.removeEventListener('beforeunload', onUnload)
      document.removeEventListener('click', onClick, true)
    }
  }, [active])

  // Escape = jään lehele
  useEffect(() => {
    if (!target) return undefined
    const onKey = (e) => e.key === 'Escape' && setTarget(null)
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [target])

  if (!target) return null
  return createPortal(
    <div className="dialog-backdrop" role="alertdialog" aria-modal="true" aria-label={t('leave.title')}
      onClick={(e) => e.target === e.currentTarget && setTarget(null)}>
      <div className="dialog">
        <p className="dialog-title">{t('leave.title')}</p>
        <p className="warning">{message}</p>
        <div className="actions">
          <button className="btn btn-dark" autoFocus onClick={() => setTarget(null)}>{t('leave.stay')}</button>
          <button className="btn" onClick={() => { const to = target; setTarget(null); navigate(to) }}>{t('leave.go')}</button>
        </div>
      </div>
    </div>,
    document.body,
  )
}
