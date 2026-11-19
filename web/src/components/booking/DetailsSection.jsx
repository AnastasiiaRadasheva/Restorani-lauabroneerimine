import { useI18n } from '../../i18n'
import { money } from '../../utils'

/**
 * IV. Õhtu detailid: soovid, broneerija andmed ja võltsmakse.
 * children – lisaplokid (tähtpäev, kingitus, lisateenused), mis tulevad hiljem.
 * lines – summade read paremal kastis: [[nimi, summa-tekst, klass]].
 */
export default function DetailsSection({ user, wish, setWish, card, setCard, lines, total, onConfirm, busy, error, children }) {
  const { t } = useI18n()
  const set = (f) => (e) => setCard({ ...card, [f]: e.target.value })

  return (
    <section id="details" className="section alt">
      <div className="container">
        <div className="sec-head">
          <div className="title">
            <span className="num">IV.</span>
            <h2>{t('details4.title')}</h2>
          </div>
          <p>{t('details4.lead')}</p>
        </div>

        <div className="details4">
          <div className="details4-main">
            {children}
            <div className="block">
              <label htmlFor="wish" className="eyebrow ox">{t('details4.wish')}</label>
              <textarea
                id="wish"
                className="box"
                rows={4}
                maxLength={500}
                value={wish}
                onChange={(e) => setWish(e.target.value)}
                placeholder={t('details4.wishPlaceholder')}
              />
            </div>
          </div>

          <div className="details4-side">
            <div className="paper">
              <p className="card-title">{t('details4.who')}</p>
              <p className="muted small">{t('details4.whoNote')}</p>
              {user ? (
                <dl className="who">
                  <dt>{t('auth.name')}</dt>
                  <dd>{user.name}</dd>
                  <dt>{t('auth.email')}</dt>
                  <dd>{user.email}</dd>
                  {user.phone && (
                    <>
                      <dt>{t('details4.phone')}</dt>
                      <dd>{user.phone}</dd>
                    </>
                  )}
                </dl>
              ) : (
                <p className="italic">{t('details4.loginFirst')}</p>
              )}
            </div>

            <div className="paper total-card">
              {lines.map(([name, value, cls]) => (
                <div key={name} className={`total-line ${cls ?? ''}`}>
                  <span>{name}</span>
                  <span>{value}</span>
                </div>
              ))}
              <div className="grand">
                <span className="eyebrow">{t('details4.total')}</span>
                <span className="sum">{money(total)}</span>
              </div>

              {total > 0 && (
                <>
                  <p className="eyebrow">{t('details4.payDemo')}</p>
                  <label className="field">
                    {t('details4.cardNumber')}
                    <input inputMode="numeric" value={card.cardNumber} onChange={set('cardNumber')} placeholder="4242 4242 4242 4242" />
                  </label>
                  <div className="field-row">
                    <label className="field">
                      {t('details4.expiry')}
                      <input inputMode="numeric" value={card.expiry} onChange={set('expiry')} placeholder="12/28" />
                    </label>
                    <label className="field">
                      CVC
                      <input inputMode="numeric" maxLength={3} value={card.cvc} onChange={set('cvc')} placeholder="123" />
                    </label>
                  </div>
                </>
              )}

              {error && <p className="alert alert-error">{error}</p>}
              <button type="button" className="btn btn-dark btn-pill btn-big btn-block" onClick={onConfirm} disabled={busy}>
                {busy ? t('auth.wait') : user ? t('booking.confirm') : t('booking.loginToBook')}
              </button>
              <p className="muted small center">{t('details4.noCharge')}</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
