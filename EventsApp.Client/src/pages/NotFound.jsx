import { Link } from 'react-router-dom'                 // קישור פנימי
import { EmptyState } from '../components/ui'           // מצב ריק

export default function NotFound() {                    // דף 404
  return (
    <EmptyState icon="🎭" title="העמוד לא נמצא" text="הכתובת שהוזנה אינה קיימת באתר.">
      <Link to="/" className="btn btn-primary">חזרה לדף הבית</Link>
    </EmptyState>
  )
}
