import { useEffect, useState } from 'react'
import { ingredientsOf } from '../dishInfo'
import { useI18n } from '../i18n'
import { money } from '../utils'
import { TAGS } from './DishTags'
import { CloseIcon } from './Icons'

const ALLERGENS = ['gluten', 'milk', 'egg', 'nuts', 'fish', 'shellfish', 'sulphites']
const DIET = ['vegetarian', 'vegan']

/** Roa kaart: foto, kirjeldus, koostis ja allergeenid. */
export function DishDialog({ item, onClose }) {
  const { t, lang } = useI18n()
  const ingredients = ingredientsOf(item, lang)
  const tags = item.tags ?? []
  const allergens = tags.filter((x) => ALLERGENS.includes(x))
  const diet = tags.filter((x) => DIET.includes(x))

  useEffect(() => {
    const onKey = (e) => e.key === 'Escape' && onClose()
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="dialog-backdrop" role="dialog" aria-modal="true" aria-label={item.name} onClick={(e) => e.target === e.currentTarget && onClose()}>
      <div className="dialog dish-dialog">
        <button type="button" className="btn btn-small dish-close" onClick={onClose} aria-label={t('details.close')}><CloseIcon /></button>
        {item.imageUrl && <img className="dish-big" src={item.imageUrl} alt="" />}
        <div className="row-between">
          <p className="dialog-title">{item.name}</p>
          {item.price != null && <span className="dish-price">{money(item.price)}</span>}
        </div>
        {item.description && <p className="italic muted">{item.description}</p>}

        <div>
          <p className="eyebrow">{t('dish.ingredients')}</p>
          {ingredients ? (
            <ul className="dish-ingredients">{ingredients.map((x) => <li key={x}>{x}</li>)}</ul>
          ) : (
            <p className="small muted">{t('dish.noIngredients')}</p>
          )}
        </div>

        <div>
          <p className="eyebrow">{t('dish.allergens')}</p>
          {item.tags == null ? (
            <p className="small muted">{t('dish.askAllergens')}</p>
          ) : allergens.length ? (
            <ul className="dish-allergens">
              {allergens.map((x) => <li key={x}><span className={`dtag dtag-${x}`}>{TAGS[x]}</span> {t(`tag.${x}`)}</li>)}
            </ul>
          ) : (
            <p className="small">{t('dish.noAllergens')}</p>
          )}
          {diet.length > 0 && (
            <p className="small dish-diet">{diet.map((x) => <span key={x}><span className={`dtag dtag-${x}`}>{TAGS[x]}</span> {t(`tag.${x}`)}</span>)}</p>
          )}
        </div>
        <p className="small muted">{t('dish.note')}</p>
      </div>
    </div>
  )
}

/**
 * Klõpsatav roog: foto ja/või nimi avavad roa kaardi. Kasutatakse igal pool, kus roogi näidatakse.
 * children – mida näidata (nt foto ja nimi); item – menüü rida (name, description, imageUrl, tags, price).
 */
export default function DishLink({ item, children, className = '' }) {
  const { t } = useI18n()
  const [open, setOpen] = useState(false)
  return (
    <>
      <button type="button" className={`dish-link ${className}`} onClick={() => setOpen(true)} title={t('dish.more')} aria-label={`${item.name} – ${t('dish.more')}`}>
        {children}
      </button>
      {open && <DishDialog item={item} onClose={() => setOpen(false)} />}
    </>
  )
}
