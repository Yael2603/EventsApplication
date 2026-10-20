import { Link } from 'react-router-dom'                 // קישור פנימי בלי רענון דף

export default function Logo({ size = 'md' }) {         // הלוגו של העסק "SHOW הפקות": ניצוץ צבעוני ושם בסגנון מדבקה
  return (
    <Link to="/" className={`logo logo-${size}`} aria-label="SHOW הפקות - דף הבית">
      <svg viewBox="0 0 64 64" className="logo-burst" aria-hidden="true">{/* פיצוץ צבעוני: סמל של הופעה */}
        <path d="M32 2l7 15 16-6-6 16 15 7-15 7 6 16-16-6-7 15-7-15-16 6 6-16-15-7 15-7-6-16 16 6z" fill="#FFD43B" stroke="#17153B" strokeWidth="3" strokeLinejoin="round" />
        <circle cx="32" cy="32" r="11" fill="#14183A" stroke="#17153B" strokeWidth="3" />
        <circle cx="32" cy="32" r="4" fill="#fff" />
      </svg>
      <span className="logo-text">
        <span className="logo-show">SHOW</span>{/* החלק הלועזי */}
        <span className="logo-sub">הפקות</span>{/* החלק העברי */}
      </span>
    </Link>
  )
}
