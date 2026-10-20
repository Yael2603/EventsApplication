import axios from 'axios'                               // ספריית הקריאות ל-HTTP

export const STORAGE_KEY = 'show_auth'                  // המפתח ב-localStorage שבו נשמרים פרטי ההתחברות

const api = axios.create({                              // מופע Axios יחיד לכל האפליקציה
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080',   // כתובת השרת מקובץ ה-env
})

api.interceptors.request.use((config) => {              // רץ לפני כל בקשה שיוצאת לשרת
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null')   // קורא את המשתמש השמור
    if (saved?.token) config.headers.Authorization = `Bearer ${saved.token}` // מוסיף את הטוקן לכותרת
  } catch { /* אם האחסון לא זמין, ממשיכים בלי טוקן */ }
  config.headers['X-Correlation-ID'] = crypto.randomUUID()   // מזהה בקשה, כמו שה-CorrelationIdMiddleware מצפה
  return config                                         // מחזיר את הבקשה כדי שתישלח
})

api.interceptors.response.use(                          // רץ על כל תשובה שחוזרת
  (res) => res,                                         // תשובה תקינה: עוברת כמו שהיא
  (err) => {                                            // תשובת שגיאה
    const hadToken = (() => {                           // האם נשלח טוקן בבקשה
      try { return !!JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null')?.token } catch { return false }
    })()
    if (err.response?.status === 401 && hadToken) {     // 401 עם טוקן: הטוקן פג או לא תקין
      window.dispatchEvent(new Event('auth:logout'))    // מודיע ל-AuthContext להתנתק
    }
    return Promise.reject(err)                          // מעביר את השגיאה הלאה לטיפול בדף
  },
)

export default api                                      // מייצא לשימוש בשכבת השירותים
