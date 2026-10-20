// ===== תוויות בעברית לערכי ה-Enum שהשרת מחזיר כמחרוזות =====
export const eventTypeLabels = {          // סוג אירוע: מפתח באנגלית (כמו בשרת) וערך להצגה
  Wedding: 'חתונה',                       // חתונה
  Engagement: 'אירוסין',                  // אירוסין
  BarMitzvah: 'בר מצווה',                 // בר מצווה
  BatMitzvah: 'בת מצווה',                 // בת מצווה
  Brit: 'ברית',                           // ברית
  Corporate: 'אירוע עסקי',                // אירוע חברה
  Other: 'אחר',                           // כל השאר
}

export const providerStatusLabels = {     // סטטוס ספק
  Pending: 'ממתין לאישור',
  Approved: 'מאושר',
  Rejected: 'נדחה',
}

export const outcomeLabels = {            // תוצאת מועמדות (מחושבת בשרת)
  Open: 'ממתינה להחלטה',
  Won: 'נבחרת לסעיף',
  Lost: 'נבחר ספק אחר',
}

export const roleLabels = {               // תפקידים
  Client: 'לקוח',
  Provider: 'ספק',
  Admin: 'מנהל',
}

export const deletionStatusLabels = {     // סטטוס בקשת מחיקה
  Pending: 'ממתינה',
  Rejected: 'נדחתה',
}

// ===== עזרי תאריכים =====
export function parseDateOnly(value) {                  // ממיר "2026-12-01" לאובייקט Date מקומי
  const [y, m, d] = String(value).split('-').map(Number)    // מפצל לשנה, חודש ויום
  return new Date(y, m - 1, d)                          // חודשים ב-JS מתחילים מ-0
}

export function toDateOnly(date) {                      // ממיר Date לטקסט "YYYY-MM-DD" (כפי שהשרת מצפה)
  const y = date.getFullYear()                          // שנה
  const m = String(date.getMonth() + 1).padStart(2, '0')    // חודש עם אפס מוביל
  const d = String(date.getDate()).padStart(2, '0')     // יום עם אפס מוביל
  return `${y}-${m}-${d}`                               // מחבר לטקסט אחד
}

export function formatDate(value) {                     // תאריך להצגה למשתמש
  return parseDateOnly(value).toLocaleDateString('he-IL', {   // בפורמט עברי
    weekday: 'long', day: 'numeric', month: 'long', year: 'numeric',   // יום בשבוע, יום, חודש, שנה
  })
}

export function formatShortDate(value) {                // תאריך קצר, למשל 01/12/2026
  return parseDateOnly(value).toLocaleDateString('he-IL')   // פורמט עברי רגיל
}

export function formatDateTime(iso) {                   // תאריך ושעה מלאים (השרת מחזיר UTC)
  const text = /Z|[+-]\d\d:\d\d$/.test(iso) ? iso : iso + 'Z'   // אם חסר Z מוסיף אותו, כדי להמיר נכון לשעון המקומי
  return new Date(text).toLocaleString('he-IL', { dateStyle: 'short', timeStyle: 'short' })   // תאריך ושעה
}

export function isFutureDate(value) {                   // האם התאריך אחרי היום (כמו הכלל בשרת)
  const today = new Date()                              // עכשיו
  today.setHours(0, 0, 0, 0)                            // מאפס שעה, כדי להשוות ימים בלבד
  return parseDateOnly(value) > today                   // true אם התאריך אחרי היום
}

// ===== כסף =====
export function formatMoney(value) {                    // סכום בשקלים
  return new Intl.NumberFormat('he-IL', {               // מעצב לפי הכללים העבריים
    style: 'currency', currency: 'ILS', maximumFractionDigits: 0,   // מטבע שקל, בלי אגורות
  }).format(value)
}

// ===== הודעות שגיאה =====
export function getErrorMessage(err) {                  // מחלץ הודעה קריאה מכל שגיאה של Axios
  if (!err.response) return 'אין תקשורת עם השרת. יש לוודא שהשרת פועל'   // אין תשובה בכלל: השרת כבוי או CORS
  const data = err.response.data                        // גוף התשובה (ProblemDetails)
  if (data?.errors && typeof data.errors === 'object') {    // שגיאות ולידציה של [ApiController]
    const all = Object.values(data.errors).flat()       // מאחד את כל ההודעות לרשימה אחת
    if (all.length) return all.join(' | ')              // מציג את כולן
  }
  if (data?.detail) return data.detail                  // ההודעה מה-ExceptionMiddleware
  if (err.response.status === 401) return 'נדרשת התחברות'   // ללא גוף: לפי הסטטוס
  if (err.response.status === 403) return 'אין הרשאה לפעולה זו'   // 403
  return 'אירעה שגיאה. יש לנסות שוב'                    // ברירת מחדל
}

export function getStatus(err) {                        // קוד הסטטוס של השגיאה (או 0 אם אין)
  return err?.response?.status ?? 0                     // משמש לטיפול מיוחד ב-409
}
