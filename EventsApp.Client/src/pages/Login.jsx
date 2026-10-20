import { useState } from 'react'                        // מצב מקומי
import { Link, useLocation, useNavigate } from 'react-router-dom'   // ניווט
import { authApi } from '../api/services'               // קריאת התחברות
import { homePathFor, useAuth } from '../auth/AuthContext'  // שמירת המשתמש
import { getErrorMessage } from '../utils/format'       // הודעת שגיאה

export default function Login() {                       // דף התחברות
  const { login } = useAuth()                           // פונקציית שמירת המשתמש
  const navigate = useNavigate()                        // ניווט בקוד
  const location = useLocation()                        // מאיפה הגיע המשתמש
  const [form, setForm] = useState({ email: '', password: '' })   // ערכי הטופס
  const [error, setError] = useState('')                // שגיאה להצגה
  const [busy, setBusy] = useState(false)               // מניעת שליחה כפולה

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })   // מעדכן שדה אחד

  const submit = async (e) => {                         // שליחת הטופס
    e.preventDefault()                                  // מונע רענון דף
    setBusy(true); setError('')
    try {
      const auth = await authApi.login(form)            // POST /api/auth/login
      login(auth)                                       // שומר טוקן ומשתמש
      navigate(location.state?.from || homePathFor(auth.role), { replace: true })   // לדף המבוקש או לאזור האישי
    } catch (err) {
      setError(getErrorMessage(err))                    // מציג שגיאה (כולל 401 על סיסמה שגויה)
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-wrap">
      <div className="card auth-card">
        <h1>ברוכים השבים</h1>
        <p className="muted">התחברו כדי להמשיך לאזור האישי</p>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={submit}>
          <div className="field"><label htmlFor="email">אימייל</label>
            <input id="email" type="email" required autoComplete="email" value={form.email} onChange={set('email')} placeholder="name@example.com" /></div>
          <div className="field"><label htmlFor="password">סיסמה</label>
            <input id="password" type="password" required autoComplete="current-password" value={form.password} onChange={set('password')} /></div>
          <button className="btn btn-primary btn-block btn-lg" disabled={busy}>{busy ? 'מתחבר...' : 'התחברות'}</button>
        </form>
        <div className="divider" />
        <p className="muted small">אין לכם חשבון? <Link to="/register">הרשמה כלקוח</Link> או <Link to="/register-provider">הרשמה כספק</Link></p>
      </div>
    </div>
  )
}
