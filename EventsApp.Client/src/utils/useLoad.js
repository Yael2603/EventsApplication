import { useCallback, useEffect, useState } from 'react'    // כלי React
import { getErrorMessage } from './format'              // הודעת שגיאה קריאה

export default function useLoad(fetcher, deps = []) {   // hook לטעינת נתונים מהשרת: fetcher היא פונקציה שמחזירה Promise
  const [data, setData] = useState(null)                // הנתונים שנטענו
  const [loading, setLoading] = useState(true)          // האם הטעינה בעיצומה
  const [error, setError] = useState('')                // הודעת שגיאה אם נכשל

  const load = useCallback(async () => {                // הפונקציה שמבצעת טעינה (גם לרענון ידני)
    setLoading(true)                                    // מציג טעינה
    setError('')                                        // מנקה שגיאה קודמת
    try {
      setData(await fetcher())                          // קורא לשרת ושומר את התוצאה
    } catch (err) {
      setError(getErrorMessage(err))                    // שומר הודעה להצגה
    } finally {
      setLoading(false)                                 // מסיים את מצב הטעינה
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps)

  useEffect(() => { load() }, [load])                   // טוען בעת הצגת הדף ובכל שינוי תלות

  return { data, loading, error, reload: load, setData }    // מחזיר גם reload ו-setData לעדכון מקומי
}
