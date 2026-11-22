import { useI18n } from '../i18n'

/** Allergeenide ja toitumise märgid (must-valged ikoonid). */
export const TAGS = {
  gluten: 'G',
  milk: 'M',
  egg: 'E',
  nuts: 'N',
  fish: 'K',
  shellfish: 'C',
  vegetarian: 'VG',
  vegan: 'V',
}

export default function DishTags({ tags }) {
  const { t } = useI18n()
  if (!tags?.length) return null
  return (
    <span className="dish-tags">
      {tags.map((tag) => (
        <span key={tag} className={`dtag dtag-${tag}`} title={t(`tag.${tag}`)} aria-label={t(`tag.${tag}`)}>
          {TAGS[tag] ?? tag[0].toUpperCase()}
        </span>
      ))}
    </span>
  )
}

/** Märkide seletus (menüü lehel). */
export function TagLegend() {
  const { t } = useI18n()
  return (
    <div className="tag-legend">
      {Object.entries(TAGS).map(([tag, letter]) => (
        <span key={tag}>
          <span className={`dtag dtag-${tag}`}>{letter}</span> {t(`tag.${tag}`)}
        </span>
      ))}
    </div>
  )
}
