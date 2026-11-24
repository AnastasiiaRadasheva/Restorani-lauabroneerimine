import { useEffect } from 'react'

/**
 * Hoiatus, kui lehelt lahkutakse saatmata tekstiga.
 * Töötab nii brauseri sulgemisel/uuendamisel (beforeunload) kui ka saidi linkidel (menüü, logo jne).
 */
export function useLeaveWarning(active, message) {
  useEffect(() => {
    if (!active) return undefined

    const onUnload = (e) => {
      e.preventDefault()
      e.returnValue = ''
    }
    const onClick = (e) => {
      const a = e.target.closest?.('a[href]')
      if (!a || a.target === '_blank' || e.defaultPrevented) return
      const url = new URL(a.href, window.location.href)
      if (url.origin !== window.location.origin || url.pathname === window.location.pathname) return
      if (!window.confirm(message)) {
        e.preventDefault()
        e.stopPropagation()
      }
    }

    window.addEventListener('beforeunload', onUnload)
    document.addEventListener('click', onClick, true)
    return () => {
      window.removeEventListener('beforeunload', onUnload)
      document.removeEventListener('click', onClick, true)
    }
  }, [active, message])
}
