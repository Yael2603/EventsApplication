import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi } from '../api/services'
import { homePathFor, useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../utils/format'

export default function Register() {                    // הרשמת לקוח
  const { login } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ fullName: '', email: '', password: '' })   // ערכי הטופס
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })

  const submit = async (e) => {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      const auth = await authApi.register(form)         // POST /api/auth/register (התפקיד נקבע בשרת: לקוח)
      login(auth)                                       // מחובר מיד לאחר ההרשמה
      navigate(homePathFor(auth.role), { replace: true })
    } catch (err) {
      setError(getErrorMessage(err))                    // למשל 409: האימייל כבר קיים
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-wrap">
      <div className="card auth-card">
        <h1>הרשמה להזמנת אירוע</h1>
        <p className="muted">דקה אחת ואתם בפנים</p>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={submit}>
          <div className="field"><label htmlFor="fullName">שם מלא</label>
            <input id="fullName" required maxLength={100} value={form.fullName} onChange={set('fullName')} /></div>
          <div className="field"><label htmlFor="email">אימייל</label>
            <input id="email" type="email" required maxLength={200} autoComplete="email" value={form.email} onChange={set('email')} /></div>
          <div className="field"><label htmlFor="password">סיסמה</label>
            <input id="password" type="password" required minLength={6} maxLength={100} autoComplete="new-password" value={form.password} onChange={set('password')} />
            <span className="hint">לפחות 6 תווים</span></div>
          <button className="btn btn-primary btn-block btn-lg" disabled={busy}>{busy ? 'נרשם...' : 'יצירת חשבון'}</button>
        </form>
        <div className="divider" />
        <p className="muted small">כבר רשומים? <Link to="/login">התחברות</Link> · ספקים: <Link to="/register-provider">הרשמה כספק</Link></p>
      </div>
    </div>
  )
}
