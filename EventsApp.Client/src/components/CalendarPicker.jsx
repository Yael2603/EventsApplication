import { useEffect, useMemo, useState } from 'react'    // כלי React
import { calendarApi } from '../api/services'           // קריאת ימי הלוח
import { getErrorMessage, toDateOnly } from '../utils/format'   // עזרים

const weekDays = ['א׳', 'ב׳', 'ג׳', 'ד׳', 'ה׳', 'ו׳', 'ש׳']     // כותרות ימי השבוע (ראשון עד שבת)

export default function CalendarPicker({ value, onChange, refreshKey = 0 }) {   // לוח חודשי לבחירת תאריך פנוי
  const today = useMemo(() => { const d = new Date(); d.setHours(0, 0, 0, 0); return d }, [])   // היום בחצות
  const [month, setMonth] = useState(() => new Date(today.getFullYear(), today.getMonth(), 1))  // החודש המוצג
  const [days, setDays] = useState({})                  // מפה: תאריך → האם תפוס (רק ימים שקיימים בלוח)
  const [loading, setLoading] = useState(false)         // טעינה
  const [error, setError] = useState('')                // שגיאה

  useEffect(() => {                                     // טוען את ימי החודש בכל מעבר חודש או רענון
    let cancelled = false                               // מונע עדכון אחרי שהרכיב ירד
    const from = toDateOnly(new Date(month.getFullYear(), month.getMonth(), 1))   // היום הראשון בחודש
    const to = toDateOnly(new Date(month.getFullYear(), month.getMonth() + 1, 0)) // היום האחרון בחודש
    setLoading(true)
    calendarApi.days(from, to)
      .then((list) => {
        if (cancelled) return
        const map = {}                                  // בונה מפה לחיפוש מהיר
        list.forEach((d) => { map[d.date] = d.isTaken })    // תאריך → תפוס?
        setDays(map)
        setError('')
      })
      .catch((err) => { if (!cancelled) setError(getErrorMessage(err)) })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }                   // ניקוי
  }, [month, refreshKey])

  const cells = useMemo(() => {                         // בונה את משבצות החודש
    const first = new Date(month.getFullYear(), month.getMonth(), 1)    // היום הראשון
    const total = new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate()   // כמה ימים בחודש
    const list = []
    for (let i = 0; i < first.getDay(); i++) list.push(null)    // משבצות ריקות לפני היום הראשון
    for (let d = 1; d <= total; d++) list.push(new Date(month.getFullYear(), month.getMonth(), d))   // הימים
    return list
  }, [month])

  const title = month.toLocaleDateString('he-IL', { month: 'long', year: 'numeric' })   // כותרת החודש

  return (
    <div className="calendar">
      <div className="calendar-head">
        <button type="button" className="icon-btn" onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() - 1, 1))}
                disabled={month <= new Date(today.getFullYear(), today.getMonth(), 1)} aria-label="חודש קודם">›</button>
        <strong>{title}</strong>
        <button type="button" className="icon-btn" onClick={() => setMonth(new Date(month.getFullYear(), month.getMonth() + 1, 1))} aria-label="חודש הבא">‹</button>
      </div>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="calendar-grid">
        {weekDays.map((w) => <div key={w} className="calendar-dow">{w}</div>)}
        {cells.map((date, i) => {
          if (!date) return <div key={`e${i}`} />       // משבצת ריקה
          const key = toDateOnly(date)                  // התאריך כטקסט
          const inCalendar = key in days                // האם קיים בלוח החברה
          const taken = days[key] === true              // תפוס
          const past = date <= today                    // היום והעבר אינם ניתנים להזמנה (כלל השרת)
          const disabled = past || taken || !inCalendar // לא ניתן לבחירה
          const cls = ['calendar-day',
            taken ? 'is-taken' : '',                    // צבע לתפוס
            value === key ? 'is-selected' : '',         // צבע לנבחר
            !taken && !disabled ? 'is-free' : '',       // צבע לפנוי
          ].join(' ')
          return (
            <button type="button" key={key} className={cls} disabled={disabled}
                    onClick={() => onChange(key)}
                    title={taken ? 'תפוס' : past ? 'תאריך שעבר' : !inCalendar ? 'לא זמין להזמנה' : 'פנוי'}>
              {date.getDate()}
            </button>
          )
        })}
      </div>
      <div className="calendar-legend">
        <span><i className="dot dot-free" /> פנוי</span>
        <span><i className="dot dot-taken" /> תפוס</span>
        <span><i className="dot dot-off" /> לא זמין</span>
        {loading && <span className="muted">טוען...</span>}
      </div>
    </div>
  )
}
