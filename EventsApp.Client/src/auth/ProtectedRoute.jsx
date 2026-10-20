import { Navigate, useLocation } from 'react-router-dom'    // ניווט וקריאת הכתובת הנוכחית
import { useAuth } from './AuthContext'                 // המשתמש המחובר

export default function ProtectedRoute({ roles, children }) {   // עוטף דף שמוגבל לתפקידים
  const { user } = useAuth()                            // מי מחובר
  const location = useLocation()                        // היכן המשתמש ניסה להגיע
  if (!user) return <Navigate to="/login" state={{ from: location.pathname }} replace />   // לא מחובר: להתחברות
  if (roles && !roles.includes(user.role)) return <Navigate to="/" replace />   // תפקיד לא מתאים: לדף הבית
  return children                                       // מותר: מציג את הדף
}
