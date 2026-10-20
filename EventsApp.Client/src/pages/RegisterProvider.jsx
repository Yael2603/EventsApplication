import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authApi, categoriesApi } from '../api/services'
import { homePathFor, useAuth } from '../auth/AuthContext'
import { getErrorMessage } from '../utils/format'
import useLoad from '../utils/useLoad'

export default function RegisterProvider() {            // הרשמת ספק (הסטטוס ההתחלתי: ממתין לאישור מנהל)
  const { login } = useAuth()
  const navigate = useNavigate()
  const { data: categories } = useLoad(() => categoriesApi.list(), [])   // הקטגוריות לבחירה
  const [form, setForm] = useState({
    fullName: '', email: '', password: '', businessName: '', description: '',
    phone: '', city: '', categoryId: '', startingPrice: '', acceptTerms: false,
  })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })   // שדה טקסט

  const submit = async (e) => {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      const auth = await authApi.registerProvider({     // POST /api/auth/register-provider
        ...form,
        categoryId: Number(form.categoryId),            // השרת מצפה למספר
        startingPrice: Number(form.startingPrice),      // השרת מצפה למספר
        description: form.description || null,          // שדה אופציונלי
      })
      login(auth)
      navigate(homePathFor(auth.role), { replace: true })
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="auth-wrap wide">
      <div className="card auth-card">
        <h1>הרשמה כספק</h1>
        <p className="muted">לאחר ההרשמה הצוות שלנו יאשר את הפרופיל, ואז אפשר להגיש מועמדויות</p>
        {error && <div className="alert alert-error">{error}</div>}
        <form onSubmit={submit}>
          <div className="form-grid">
            <div className="field"><label htmlFor="fullName">שם מלא</label>
              <input id="fullName" required maxLength={100} value={form.fullName} onChange={set('fullName')} /></div>
            <div className="field"><label htmlFor="businessName">שם העסק</label>
              <input id="businessName" required maxLength={150} value={form.businessName} onChange={set('businessName')} /></div>
            <div className="field"><label htmlFor="email">אימייל</label>
              <input id="email" type="email" required maxLength={200} value={form.email} onChange={set('email')} /></div>
            <div className="field"><label htmlFor="password">סיסמה</label>
              <input id="password" type="password" required minLength={6} maxLength={100} value={form.password} onChange={set('password')} /></div>
            <div className="field"><label htmlFor="phone">טלפון</label>
              <input id="phone" required maxLength={30} value={form.phone} onChange={set('phone')} /></div>
            <div className="field"><label htmlFor="city">עיר</label>
              <input id="city" required maxLength={100} value={form.city} onChange={set('city')} /></div>
            <div className="field"><label htmlFor="categoryId">קטגוריה</label>
              <select id="categoryId" required value={form.categoryId} onChange={set('categoryId')}>
                <option value="">בחירת קטגוריה</option>
                {(categories || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select></div>
            <div className="field"><label htmlFor="startingPrice">מחיר התחלתי (₪)</label>
              <input id="startingPrice" type="number" min="0" step="1" required value={form.startingPrice} onChange={set('startingPrice')} /></div>
            <div className="field full"><label htmlFor="description">תיאור העסק</label>
              <textarea id="description" maxLength={1000} value={form.description} onChange={set('description')} /></div>
          </div>
          <label className="checkbox">
            <input type="checkbox" checked={form.acceptTerms} onChange={(e) => setForm({ ...form, acceptTerms: e.target.checked })} />
            <span>קראתי ואני מאשר/ת את תנאי השימוש</span>
          </label>
          <div className="spacer" />
          <button className="btn btn-primary btn-block btn-lg" disabled={busy || !form.acceptTerms}>{busy ? 'נרשם...' : 'יצירת חשבון ספק'}</button>
        </form>
        <div className="divider" />
        <p className="muted small">כבר רשומים? <Link to="/login">התחברות</Link></p>
      </div>
    </div>
  )
}
