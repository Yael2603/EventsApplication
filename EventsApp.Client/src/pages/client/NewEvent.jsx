import { useState } from 'react'                        // מצב מקומי
import { useNavigate } from 'react-router-dom'          // ניווט בקוד
import { categoriesApi, eventsApi } from '../../api/services'   // קריאות לשרת
import CalendarPicker from '../../components/CalendarPicker'    // לוח הבחירה
import { PageHeader, Loader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { eventTypeLabels, formatDate, getErrorMessage, getStatus } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const emptyItem = { categoryId: '', budget: '', details: '' }   // שורת סעיף ריקה

export default function NewEvent() {                    // יצירת אירוע חדש
  const navigate = useNavigate()
  const toast = useToast()
  const { data: categories, loading } = useLoad(() => categoriesApi.list(), [])   // קטגוריות לבחירה
  const [date, setDate] = useState('')                  // התאריך שנבחר
  const [refreshKey, setRefreshKey] = useState(0)       // מכריח את הלוח להיטען מחדש (אחרי 409)
  const [form, setForm] = useState({ type: 'Wedding', location: '', guestCount: '', notes: '' })   // פרטי האירוע
  const [items, setItems] = useState([{ ...emptyItem }])    // סעיפי האירוע (לפחות אחד)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const set = (key) => (e) => setForm({ ...form, [key]: e.target.value })   // עדכון שדה

  const setItem = (index, key, value) =>                // עדכון שדה בסעיף מסוים
    setItems(items.map((it, i) => (i === index ? { ...it, [key]: value } : it)))

  const chosenIds = items.map((i) => i.categoryId)      // קטגוריות שכבר נבחרו (למניעת כפילות)

  const submit = async (e) => {
    e.preventDefault()
    setError('')
    if (!date) { setError('יש לבחור תאריך פנוי בלוח'); return }   // בדיקה בצד הלקוח
    setBusy(true)
    try {
      const created = await eventsApi.create({          // POST /api/events
        date,
        type: form.type,
        location: form.location,
        guestCount: Number(form.guestCount),
        notes: form.notes || null,
        items: items.map((i) => ({                      // ממיר לפי ה-DTO של השרת
          categoryId: Number(i.categoryId),
          budget: Number(i.budget),
          details: i.details || null,
        })),
      })
      toast.success('האירוע נוצר בהצלחה!')
      navigate(`/events/${created.id}`)                 // לדף האירוע
    } catch (err) {
      if (getStatus(err) === 409) {                     // התאריך נתפס בינתיים (הזמנה כפולה)
        setDate('')                                     // מבטל את הבחירה
        setRefreshKey((k) => k + 1)                     // מרענן את הלוח כדי להציג אותו כתפוס
      }
      setError(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  if (loading) return <Loader />

  return (
    <>
      <PageHeader title="אירוע חדש" subtitle="בוחרים תאריך, ממלאים פרטים ומוסיפים סעיפים" />
      {error && <div className="alert alert-error">{error}</div>}
      <form onSubmit={submit}>
        <div className="grid grid-2">
          <div className="card">
            <h2>1. תאריך</h2>
            <CalendarPicker value={date} onChange={setDate} refreshKey={refreshKey} />
            <p className="muted" style={{ marginTop: 12 }}>{date ? `נבחר: ${formatDate(date)}` : 'יש ללחוץ על יום פנוי (ירוק)'}</p>
          </div>

          <div className="card">
            <h2>2. פרטי האירוע</h2>
            <div className="field"><label htmlFor="type">סוג האירוע</label>
              <select id="type" value={form.type} onChange={set('type')}>
                {Object.entries(eventTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select></div>
            <div className="field"><label htmlFor="location">מקום</label>
              <input id="location" required maxLength={200} value={form.location} onChange={set('location')} placeholder="למשל: אולמי הגן, נתניה" /></div>
            <div className="field"><label htmlFor="guests">מספר אורחים</label>
              <input id="guests" type="number" required min="1" max="5000" value={form.guestCount} onChange={set('guestCount')} /></div>
            <div className="field"><label htmlFor="notes">הערות</label>
              <textarea id="notes" maxLength={1000} value={form.notes} onChange={set('notes')} /></div>
          </div>
        </div>

        <div className="spacer" />
        <div className="card">
          <div className="row row-between">
            <h2>3. סעיפי האירוע</h2>
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => setItems([...items, { ...emptyItem }])}
                    disabled={items.length >= (categories || []).length}>+ הוספת סעיף</button>
          </div>
          <p className="muted">סעיף אחד לכל קטגוריה, עם תקציב. ספקים יגישו מועמדות לכל סעיף.</p>
          {items.map((item, index) => (
            <div className="item-builder" key={index}>
              <select required value={item.categoryId} onChange={(e) => setItem(index, 'categoryId', e.target.value)}>
                <option value="">קטגוריה</option>
                {(categories || []).map((c) => (
                  <option key={c.id} value={c.id} disabled={chosenIds.includes(String(c.id)) && String(c.id) !== item.categoryId}>{c.name}</option>
                ))}
              </select>
              <input type="number" required min="1" step="1" placeholder="תקציב ₪" value={item.budget} onChange={(e) => setItem(index, 'budget', e.target.value)} />
              <input maxLength={1000} placeholder="פרטים (אופציונלי)" value={item.details} onChange={(e) => setItem(index, 'details', e.target.value)} />
              <button type="button" className="icon-btn" aria-label="הסרת סעיף" disabled={items.length === 1}
                      onClick={() => setItems(items.filter((_, i) => i !== index))}>✕</button>
            </div>
          ))}
        </div>

        <div className="spacer" />
        <button className="btn btn-primary btn-lg" disabled={busy}>{busy ? 'יוצר אירוע...' : 'יצירת האירוע'}</button>
      </form>
    </>
  )
}
