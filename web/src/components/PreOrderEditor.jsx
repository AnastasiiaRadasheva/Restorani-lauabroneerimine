import { useState } from 'react'
import { api } from '../api'
import { useI18n } from '../i18n'
import { money } from '../utils'
import { useMenu } from './booking/GastronomySection'
import DishTags from './DishTags'
import { CloseIcon } from './Icons'

/**
 * Eeltellimuse muutmine pärast broneerimist (rohkem kui 24 h enne).
 * Valitud road on näha fotodega; "+" avab menüü, kust saab roa lisada.
 * Kui tellimus läheb kallimaks, makstakse vahe kaardiga; odavama puhul vahe tagastatakse.
 */
export default function PreOrderEditor({ r, onChanged }) {
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  if (!r.canModify) return null
  return (
    <>
      <button type="button" className="btn btn-small" onClick={() => setOpen(true)}>{t('po.change')}</button>
      {open && <Dialog r={r} onClose={() => setOpen(false)} onSaved={() => { setOpen(false); onChanged() }} />}
    </>
  )
}

function Dialog({ r, onClose, onSaved }) {
  const { t } = useI18n()
  const menu = useMenu(r.restaurantId)
  const [qty, setQty] = useState(() => Object.fromEntries(r.preOrder.map((p) => [p.menuItemId, p.quantity])))
  const [picking, setPicking] = useState(false)
  const [cat, setCat] = useState(null)
  const [card, setCard] = useState({ cardHolder: r.customerName, cardNumber: '4242 4242 4242 4242', expiry: '12/28', cvc: '123' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const price = (id) => menu.find((m) => m.id === Number(id))?.price ?? r.preOrder.find((p) => p.menuItemId === Number(id))?.unitPrice ?? 0
  const newTotal = Object.entries(qty).reduce((s, [id, q]) => s + q * price(id), 0)
  const oldTotal = r.preOrderTotal ?? r.preOrder.reduce((s, p) => s + p.total, 0)
  const diff = newTotal - oldTotal
  const due = r.isPaid ? diff : r.total - oldTotal + newTotal
  const change = (id, d) => setQty((q) => ({ ...q, [id]: Math.max(0, Math.min(50, (q[id] ?? 0) + d)) }))
  const chosen = Object.entries(qty).filter(([, q]) => q > 0).map(([id, q]) => ({ id: Number(id), q, m: menu.find((x) => x.id === Number(id)) }))
  const categories = [...new Set(menu.map((m) => m.category))]
  const current = cat ?? categories[0]

  async function save() {
    setBusy(true)
    try {
      await api(`/api/reservations/${r.id}/preorder`, {
        method: 'POST',
        body: { items: chosen.map((c) => ({ menuItemId: c.id, quantity: c.q })), card: due > 0 ? card : null },
      })
      onSaved()
    } catch (e) {
      setError(e.message)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="dialog-backdrop" role="presentation" onClick={onClose}>
      <div className="dialog po-dialog" role="dialog" aria-modal="true" onClick={(e) => e.stopPropagation()}>
        <div className="row-between">
          <p className="dialog-title">{picking ? t('po.pick') : t('po.title')}</p>
          <button type="button" className="btn btn-small" onClick={picking ? () => setPicking(false) : onClose} aria-label="close"><CloseIcon /></button>
        </div>

        {!picking && (
          <>
            {chosen.length === 0 && <p className="italic muted">{t('gastro.empty')}</p>}
            <div className="po-list">
              {chosen.map(({ id, q, m }) => (
                <div key={id} className="po-row">
                  {m?.imageUrl && <img src={m.imageUrl} alt="" />}
                  <span className="name">{m?.name ?? r.preOrder.find((p) => p.menuItemId === id)?.name}<span className="muted small d-block">{money(price(id))}</span></span>
                  <div className="stepper round">
                    <button type="button" onClick={() => change(id, -1)}>−</button>
                    <span className="value">{q}</span>
                    <button type="button" onClick={() => change(id, 1)}>+</button>
                  </div>
                </div>
              ))}
            </div>
            <button type="button" className="po-add" onClick={() => setPicking(true)}>
              <span className="plus">+</span> {t('po.add')}
            </button>

            <dl className="receipt-dl">
              <dt>{t('gastro.preorder')}</dt><dd>{money(newTotal)}</dd>
              {due > 0 && (<><dt>{t('po.due')}</dt><dd className="ox">{money(due)}</dd></>)}
              {r.isPaid && diff < 0 && (<><dt>{t('po.refund')}</dt><dd>{money(-diff)}</dd></>)}
            </dl>
            {due > 0 && (
              <div className="field-row">
                <label className="field">{t('details4.cardNumber')}<input value={card.cardNumber} onChange={(e) => setCard({ ...card, cardNumber: e.target.value })} /></label>
                <label className="field">{t('details4.expiry')}<input value={card.expiry} onChange={(e) => setCard({ ...card, expiry: e.target.value })} /></label>
                <label className="field">CVC<input value={card.cvc} maxLength={3} onChange={(e) => setCard({ ...card, cvc: e.target.value })} /></label>
              </div>
            )}
            {error && <p className="field-error">{error}</p>}
            <div className="actions">
              <button type="button" className="btn btn-dark" disabled={busy} onClick={save}>{due > 0 ? t('po.saveAndPay', { sum: money(due) }) : t('po.save')}</button>
              <button type="button" className="btn" onClick={onClose}>{t('po.cancel')}</button>
            </div>
          </>
        )}

        {picking && (
          <>
            <div className="cat-tabs">
              {categories.map((c) => (
                <button key={c} type="button" className={c === current ? 'chip caps on' : 'chip caps'} onClick={() => setCat(c)}>{c}</button>
              ))}
            </div>
            <div className="po-list pick">
              {menu.filter((m) => m.category === current).map((m) => (
                <button key={m.id} type="button" className="po-row" onClick={() => { change(m.id, 1); setPicking(false) }}>
                  {m.imageUrl && <img src={m.imageUrl} alt="" />}
                  <span className="name">{m.name}<span className="muted small d-block">{m.description}</span><DishTags tags={m.tags} /></span>
                  <span className="price">{money(m.price)}</span>
                </button>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  )
}
