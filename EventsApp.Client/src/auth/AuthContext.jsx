import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'   // כלי React
import { STORAGE_KEY } from '../api/client'             // המפתח המשותף ל-localStorage

const AuthContext = createContext(null)                 // ההקשר שיחזיק את המשתמש המחובר

function readSaved() {                                  // קורא משתמש שמור (אם יש) בעת טעינת האתר
  try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null') } catch { return null }
}

export function AuthProvider({ children }) {            // עוטף את האפליקציה ומספק את המשתמש לכל הדפים
  const [user, setUser] = useState(readSaved)           // המצב ההתחלתי נקרא מהאחסון

  const login = useCallback((auth) => {                 // נקרא אחרי התחברות או הרשמה מוצלחת
    const saved = {                                     // שומרים רק את מה שנדרש
      token: auth.token, userId: auth.userId, fullName: auth.fullName, email: auth.email, role: auth.role,
    }
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(saved)) } catch { /* אחסון חסום: נשאר בזיכרון */ }
    setUser(saved)                                      // מעדכן את כל הרכיבים
  }, [])

  const logout = useCallback(() => {                    // התנתקות
    try { localStorage.removeItem(STORAGE_KEY) } catch { /* אין מה לעשות */ }
    setUser(null)                                       // מוחק את המשתמש מהמצב
  }, [])

  useEffect(() => {                                     // מאזין לאירוע שה-interceptor שולח בעת 401
    window.addEventListener('auth:logout', logout)      // מתנתק אוטומטית כשהטוקן פג
    return () => window.removeEventListener('auth:logout', logout)   // ניקוי כשהרכיב יורד
  }, [logout])

  const value = useMemo(() => ({ user, login, logout }), [user, login, logout])   // ערך יציב להקשר
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>    // מספק לילדים
}

export function useAuth() {                             // hook לשימוש בדפים
  return useContext(AuthContext)                        // מחזיר { user, login, logout }
}

export function homePathFor(role) {                     // לאן מנווטים אחרי התחברות לפי תפקיד
  if (role === 'Admin') return '/admin'                 // מנהל: לוח בקרה
  if (role === 'Provider') return '/provider/open'      // ספק: סעיפים פתוחים
  return '/events'                                      // לקוח: האירועים שלו
}
