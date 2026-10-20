import { useEffect, useState } from 'react'             // כלי React
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'   // ניווט
import { useAuth } from '../auth/AuthContext'           // המשתמש המחובר
import { notificationsApi } from '../api/services'      // ספירת התראות
import { roleLabels } from '../utils/format'            // שם התפקיד בעברית
import Logo from './Logo'                               // הלוגו

const linksByRole = {                                   // תפריט הניווט לפי תפקיד
  Client: [
    { to: '/events', label: 'האירועים שלי' },
    { to: '/events/new', label: 'אירוע חדש' },
  ],
  Provider: [
    { to: '/provider/open', label: 'סעיפים פתוחים' },
    { to: '/provider/candidacies', label: 'המועמדויות שלי' },
    { to: '/provider/bookings', label: 'שיבוצים' },
    { to: '/provider/profile', label: 'פרופיל' },
  ],
  Admin: [
    { to: '/admin', label: 'לוח בקרה', end: true },
    { to: '/admin/events', label: 'אירועים' },
    { to: '/admin/providers', label: 'ספקים' },
    { to: '/admin/categories', label: 'קטגוריות' },
    { to: '/admin/deletion-requests', label: 'בקשות מחיקה' },
  ],
}

export default function Layout() {                      // המעטפת: תפריט עליון, תוכן ותחתית
  const { user, logout } = useAuth()                    // המשתמש והתנתקות
  const navigate = useNavigate()                        // ניווט בקוד
  const location = useLocation()                        // הכתובת הנוכחית (לרענון ספירה)
  const [unread, setUnread] = useState(0)               // מספר התראות שלא נקראו
  const [open, setOpen] = useState(false)               // תפריט נייד פתוח?

  useEffect(() => { setOpen(false) }, [location.pathname])    // סוגר תפריט נייד בכל ניווט

  useEffect(() => {                                     // טוען ספירת התראות בכל ניווט
    if (!user) { setUnread(0); return }                 // לא מחובר: אפס
    notificationsApi.list()
      .then((list) => setUnread(list.filter((n) => !n.isRead).length))   // סופר את שלא נקראו
      .catch(() => setUnread(0))                        // שגיאה בספירה לא מפריעה לאתר
  }, [user, location.pathname])

  const links = user ? linksByRole[user.role] || [] : []    // הקישורים של התפקיד

  const onLogout = () => { logout(); navigate('/') }    // מתנתק וחוזר לדף הבית

  return (
    <div className="app-shell">
      <header className="navbar">
        <div className="container navbar-inner">
          <Logo />
          <button className="nav-toggle" onClick={() => setOpen(!open)} aria-label="תפריט">☰</button>
          <nav className={`nav-links ${open ? 'is-open' : ''}`}>
            {links.map((l) => (
              <NavLink key={l.to} to={l.to} end={l.end} className={({ isActive }) => (isActive ? 'active' : '')}>
                {l.label}
              </NavLink>
            ))}
          </nav>
          <div className={`nav-user ${open ? 'is-open' : ''}`}>
            {user ? (
              <>
                <Link to="/notifications" className="bell" aria-label="התראות">
                  🔔{unread > 0 && <span className="bell-count">{unread}</span>}
                </Link>
                <span className="nav-name">
                  {user.fullName}<small>{roleLabels[user.role]}</small>
                </span>
                <button className="btn btn-ghost btn-sm" onClick={onLogout}>יציאה</button>
              </>
            ) : (
              <>
                <Link to="/login" className="btn btn-ghost btn-sm">התחברות</Link>
                <Link to="/register" className="btn btn-primary btn-sm">הרשמה</Link>
              </>
            )}
          </div>
        </div>
      </header>

      <main className="container main">
        <Outlet />{/* כאן מוצג הדף הנוכחי */}
      </main>

      <footer className="footer">
        <div className="container footer-inner">
          <Logo size="sm" />
          <span className="muted">© {new Date().getFullYear()} SHOW הפקות. כל הזכויות שמורות.</span>
        </div>
      </footer>
    </div>
  )
}
