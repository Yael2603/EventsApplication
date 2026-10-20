import { useEffect, useState } from 'react'
import { profileApi } from '../../api/services'
import { Badge, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { getErrorMessage, providerStatusLabels } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const tones = { Pending: 'gold', Approved: 'green', Rejected: 'red' }   // צבע לפי סטטוס

export default function ProviderProfile() {             // פרופיל הספק (ללא קטגוריה וסטטוס: אינם ניתנים לעריכה)
  const toast = useToast()
  const { data, loading, error, reload, setData } = useLoad(() => profileApi.get(), [])   // GET /api/providers/me
  const [form, setForm] = useState(null)
  const [busy, setBusy] = useState(false)

  useEffect(() => {                                     // ממלא את הטופס כשהנתונים מגיעים
    if (data) setForm({ businessName: data.businessName, description: data.description || '', phone: data.phone, city: data.city, startingPrice: data.startingPrice })
  }, [data])

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })

  const submit = async (e) => {
    e.preventDefault()
    setBusy(true)
    try {
      const updated = await profileApi.update({ ...form, description: form.description || null, startingPrice: Number(form.startingPrice) })   // PUT /api/providers/me
      setData(updated)
      toast.success('הפרופיל עודכן')
    } catch (err) {
      toast.error(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <Loader />
  if (error) return <ErrorBox message={error} onRetry={reload} />
  if (!data || !form) return null

  return (
    <>
      <PageHeader title="הפרופיל שלי" subtitle={data.email}>
        <Badge tone={tones[data.status]}>{providerStatusLabels[data.status]}</Badge>
      </PageHeader>
      {data.status === 'Pending' && <div className="alert alert-warn">הפרופיל ממתין לאישור המנהל. עד לאישור לא ניתן להגיש מועמדויות.</div>}
      {data.status === 'Rejected' && <div className="alert alert-error">הפרופיל נדחה. לפרטים יש לפנות לצוות.</div>}
      <form className="card" onSubmit={submit}>
        <div className="form-grid">
          <div className="field"><label>שם העסק</label><input required maxLength={150} value={form.businessName} onChange={set('businessName')} /></div>
          <div className="field"><label>קטגוריה</label><input value={data.categoryName} disabled /><span className="hint">הקטגוריה אינה ניתנת לשינוי</span></div>
          <div className="field"><label>טלפון</label><input required maxLength={30} value={form.phone} onChange={set('phone')} /></div>
          <div className="field"><label>עיר</label><input required maxLength={100} value={form.city} onChange={set('city')} /></div>
          <div className="field"><label>מחיר התחלתי (₪)</label><input type="number" required min="1" step="1" value={form.startingPrice} onChange={set('startingPrice')} /></div>
          <div className="field full"><label>תיאור</label><textarea maxLength={1000} value={form.description} onChange={set('description')} /></div>
        </div>
        <button className="btn btn-primary" disabled={busy}>{busy ? 'שומר...' : 'שמירת שינויים'}</button>
      </form>
    </>
  )
}
