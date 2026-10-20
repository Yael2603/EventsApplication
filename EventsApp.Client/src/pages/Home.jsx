import { Link } from 'react-router-dom'                 // קישורים
import { useAuth, homePathFor } from '../auth/AuthContext'  // המשתמש המחובר
import { categoriesApi } from '../api/services'         // קטגוריות מהשרת
import useLoad from '../utils/useLoad'                  // טעינת נתונים
import { Loader } from '../components/ui'               // אינדיקציית טעינה

export default function Home() {                        // דף הבית הציבורי
  const { user } = useAuth()                            // האם מחובר ובאיזה תפקיד
  const { data: categories, loading } = useLoad(() => categoriesApi.list(), [])   // הקטגוריות (פתוח לכולם)

  return (
    <>
      <section className="hero photo photo-hero">{/* תמונת פתיחה: public/images/hero.jpg, ואם חסרה מוצגת תאורת במה */}
        <div className="hero-copy">
          <span className="rule" aria-hidden="true" />
          <h1>ערבים שנשארים<br />בזיכרון.</h1>
          <p className="lead">הפקת אירועים מקצה לקצה: תאריך, ספקים ותקציב במקום אחד, ובלי הפתעות ברגע האחרון.</p>
          <div className="hero-actions">
            {user ? (
              <Link to={homePathFor(user.role)} className="btn btn-primary btn-lg">לאזור האישי</Link>
            ) : (
              <>
                <Link to="/register" className="btn btn-primary btn-lg">להזמנת אירוע</Link>
                <Link to="/register-provider" className="link-line">הצטרפות כספק</Link>
              </>
            )}
          </div>
        </div>
        <div className="hero-strip">
          <em>לוח התאריכים מתעדכן בזמן אמת</em>
          <span className="sep" aria-hidden="true" />
          <span>יום אחד, אירוע אחד</span>
        </div>
      </section>

      <section className="intro">
        <h2>הפקה אחת.<br />תאריך אחד. אירוע אחד.</h2>
        <div>
          <p>לכל תאריך מוקצה אירוע אחד בלבד, כך שהצוות והספקים מתמקדים בערב שלכם. בוחרים תאריך פנוי, בונים את האירוע סעיף אחר סעיף, וספקים מאושרים מגישים הצעות.</p>
          {!user && <Link to="/register">להתחלה</Link>}
        </div>
      </section>

      <section className="gallery" aria-label="אירועים שהפקנו">{/* שלוש תמונות: wedding.jpg, corporate.jpg, table.jpg */}
        <figure className="photo photo-a"><figcaption><b>חופה בגן</b>חתונות ואירועים משפחתיים</figcaption></figure>
        <figure className="photo photo-b"><figcaption><b>ערב חברה</b>אירועי חברה וכנסים</figcaption></figure>
        <figure className="photo photo-c"><figcaption><b>שולחן חג</b>אירועים פרטיים</figcaption></figure>
      </section>

      <section className="index">
        <div className="index-head">
          <h2>תחומי הפקה</h2>
          <span className="muted">ספקים מאושרים בכל תחום</span>
        </div>
        {loading ? <Loader /> : (categories || []).map((c) => (
          <div key={c.id} className="index-row"><i aria-hidden="true">&mdash;</i><b>{c.name}</b></div>
        ))}
      </section>

      <section className="process" aria-label="איך זה עובד">
        <div><span className="n">I</span><h3>בוחרים תאריך</h3><p>הלוח מראה אילו ימים פנויים, בזמן אמת.</p></div>
        <div><span className="n">II</span><h3>בונים את האירוע</h3><p>סעיף לכל קטגוריה, עם תקציב לכל סעיף.</p></div>
        <div><span className="n">III</span><h3>ספקים מגישים הצעות</h3><p>הצוות בוחר ומשבץ את הספק המתאים.</p></div>
      </section>

      {!user && (
        <section className="band photo photo-b">
          <div>
            <h2>ספקים מוזמנים להצטרף</h2>
            <p>נרשמים, מאושרים על ידי הצוות, ומגישים מועמדות לסעיפים שמתאימים להם.</p>
          </div>
          <Link to="/register-provider" className="btn btn-lemon btn-lg">להרשמה כספק</Link>
        </section>
      )}
      <div className="home-end" />
    </>
  )
}
