import { useEffect, useState } from 'react'

/** Broneerimise sammud lehe ülaosas (i. Teie õhtu, ii. Teie koht …). Aktiivne samm järgib kerimist. */
export default function StepsBar({ steps }) {
  const [active, setActive] = useState(steps[0]?.id)

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries.filter((e) => e.isIntersecting)
        if (visible.length) setActive(visible[0].target.id)
      },
      { rootMargin: '-40% 0px -55% 0px' },
    )
    steps.forEach((s) => {
      const el = document.getElementById(s.id)
      if (el) observer.observe(el)
    })
    return () => observer.disconnect()
  }, [steps.map((s) => s.id).join()]) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className="steps-bar">
      <div className="container">
        {steps.map((s, i) => (
          <a key={s.id} href={`#${s.id}`} className={active === s.id ? 'on' : ''}>
            <span className={active === s.id ? 'diamond ox' : 'diamond hollow'} />
            {['i', 'ii', 'iii', 'iv'][i]}. {s.label}
          </a>
        ))}
      </div>
    </div>
  )
}
