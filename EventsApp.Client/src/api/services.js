import api from './client'                              // מופע Axios עם הטוקן

const data = (promise) => promise.then((res) => res.data)   // עוזר: מחזיר רק את גוף התשובה

export const authApi = {                                // התחברות והרשמה
  login: (body) => data(api.post('/api/auth/login', body)),                       // POST /api/auth/login
  register: (body) => data(api.post('/api/auth/register', body)),                 // הרשמת לקוח
  registerProvider: (body) => data(api.post('/api/auth/register-provider', body)),// הרשמת ספק
}

export const calendarApi = {                            // לוח השנה של החברה
  days: (from, to) => data(api.get('/api/calendar', { params: { from, to } })),   // ימים בטווח
}

export const categoriesApi = {                          // קטגוריות
  list: () => data(api.get('/api/categories')),                                   // פתוח לכולם
  create: (name) => data(api.post('/api/categories', { name })),                  // מנהל
  update: (id, name) => data(api.put(`/api/categories/${id}`, { name })),         // מנהל
  remove: (id) => api.delete(`/api/categories/${id}`),                            // מנהל (204)
}

export const eventsApi = {                              // אירועים
  create: (body) => data(api.post('/api/events', body)),                          // לקוח
  mine: () => data(api.get('/api/events/my')),                                    // האירועים שלי
  all: (page = 1, pageSize = 10) => data(api.get('/api/events', { params: { page, pageSize } })),   // מנהל: עמוד אחד ({ items, page, totalPages, ... })
  get: (id) => data(api.get(`/api/events/${id}`)),                                // אירוע אחד
  update: (id, body) => data(api.put(`/api/events/${id}`, body)),                 // עריכה
  addItem: (id, body) => data(api.post(`/api/events/${id}/items`, body)),         // הוספת סעיף
}

export const itemsApi = {                               // סעיפי אירוע
  open: () => data(api.get('/api/event-items/open')),                             // ספק: סעיפים פתוחים
  candidacies: (itemId) => data(api.get(`/api/event-items/${itemId}/candidacies`)),   // מנהל
  assign: (itemId, providerId) => data(api.put(`/api/event-items/${itemId}/provider`, { providerId })),   // מנהל: שיבוץ
  remove: (itemId) => api.delete(`/api/event-items/${itemId}`),                   // לקוח: הסרה
}

export const candidaciesApi = {                         // מועמדויות (ספק)
  apply: (body) => data(api.post('/api/candidacies', body)),                      // הגשה
  mine: () => data(api.get('/api/candidacies/my')),                               // שלי
  withdraw: (id) => api.delete(`/api/candidacies/${id}`),                         // ביטול
}

export const providersApi = {                           // ספקים (מנהל)
  list: (status) => data(api.get('/api/providers', { params: { status } })),      // לפי סטטוס
  setStatus: (id, status) => data(api.put(`/api/providers/${id}/status`, { status })),   // אישור/דחייה
}

export const profileApi = {                             // פרופיל הספק המחובר
  get: () => data(api.get('/api/providers/me')),                                  // הפרופיל
  update: (body) => data(api.put('/api/providers/me', body)),                     // עדכון
  bookings: () => data(api.get('/api/providers/me/bookings')),                    // שיבוצים
}

export const deletionApi = {                            // בקשות מחיקה
  create: (eventId, reason) => data(api.post(`/api/events/${eventId}/deletion-request`, { reason })),   // לקוח
  list: (status) => data(api.get('/api/deletion-requests', { params: { status } })),    // מנהל
  approve: (id) => api.patch(`/api/deletion-requests/${id}`, { decision: 'Approve' }),               // מנהל (204)
  reject: (id, adminNote) => data(api.patch(`/api/deletion-requests/${id}`, { decision: 'Reject', adminNote })),   // מנהל
}

export const notificationsApi = {                       // התראות
  list: () => data(api.get('/api/notifications')),                                // שלי
  markRead: (id) => api.patch(`/api/notifications/${id}`, { isRead: true }),      // סימון כנקראה
}

export const statsApi = {                               // סטטיסטיקות מנהל
  get: () => data(api.get('/api/admin/stats')),
}
