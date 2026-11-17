/** Väikesed joonikoonid (SVG), et ei oleks vaja ikoonide teeki. */

export function Arrow({ width = 40 }) {
  return (
    <svg className="arrow" aria-hidden="true" width={width} height="12" viewBox={`0 0 ${width} 12`} fill="none" stroke="currentColor" strokeWidth="1">
      <path d={`M0 6h${width - 2}`} />
      <path d={`M${width - 7} 1l5 5-5 5`} />
    </svg>
  )
}

export function CalendarIcon() {
  return (
    <svg aria-hidden="true" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4">
      <rect x="3.5" y="5" width="17" height="15.5" rx="3" />
      <path d="M3.5 10h17M8 3v4M16 3v4" />
    </svg>
  )
}

export function CloseIcon() {
  return (
    <svg aria-hidden="true" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4">
      <path d="M5 5l14 14M19 5L5 19" />
    </svg>
  )
}

export function CheckIcon() {
  return (
    <svg aria-hidden="true" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.6">
      <path d="M5 12.5l4.5 4.5L19 7.5" />
    </svg>
  )
}

export function MenuIcon() {
  return (
    <svg aria-hidden="true" width="34" height="18" viewBox="0 0 34 18" fill="none" stroke="currentColor" strokeWidth="1.4">
      <path d="M0 1h34M0 17h22" />
    </svg>
  )
}

export function UserIcon() {
  return (
    <svg aria-hidden="true" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.3">
      <circle cx="12" cy="8" r="4" />
      <path d="M4 21c1.2-4 4.3-6 8-6s6.8 2 8 6" />
    </svg>
  )
}

export function ChevronDown() {
  return (
    <svg aria-hidden="true" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.4">
      <path d="M5 9l7 7 7-7" />
    </svg>
  )
}
