import { Link } from 'react-router-dom'                 // קישור פנימי בלי רענון דף

export default function Logo({ size = 'md' }) {         // הלוגו של העסק: סימן-מילה "SHOW" בכתב מרווח, ו"הפקות" בפליז
  return (
    <Link to="/" className={`logo logo-${size}`} aria-label="SHOW הפקות - דף הבית">
      <span className="logo-text">
        <span className="logo-show">SHOW</span>{/* החלק הלועזי */}
        <span className="logo-sub">הפקות</span>{/* החלק העברי */}
      </span>
    </Link>
  )
}
