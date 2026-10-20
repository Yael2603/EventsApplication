import { Link } from 'react-router-dom'                 // קישורים
import { useAuth, homePathFor } from '../auth/AuthContext'  // המשתמש המחובר
import { categoriesApi } from '../api/services'         // קטגוריות מהשרת
import useLoad from '../utils/useLoad'                  // טעינת נתונים
import { Loader } from '../components/ui'               // אינדיקציית טעינה

const icons = ['🎶', '🍽️', '📸', '🎪', '💐', '🎤', '✨', '🎂']          // אייקונים לקטגוריות (בסיבוב)
const shapes = ['shape-circle', 'shape-arch', 'shape-squircle', 'shape-flower', 'shape-pill']   // צורות מגוונות לכל קטגוריה
const tints = ['t-lemon', 't-pink', 't-mint', 't-cobalt', 't-tangerine', 't-violet']          // צבעים מתחלפים

export default function Home() {                        // דף הבית הציבורי
  const { user } = useAuth()                            // האם מחובר ובאיזה תפקיד
  const { data: categories, loading } = useLoad(() => categoriesApi.list(), [])   // הקטגוריות (פתוח לכולם)

  return (
    <>
      <section className="hero stage">
        <span className="curtain curtain-l" aria-hidden="true" /><span className="curtain curtain-r" aria-hidden="true" /><span className="spot" aria-hidden="true" />
        <div className="hero-text">
          <h1>הערב שלכם. ההפקה שלנו.</h1>
          <p className="lead">
            בוחרים תאריך פנוי, בונים את האירוע סעיף אחר סעיף, ואנחנו מחברים אתכם לספקים הטובים ביותר: מהמוזיקה ועד הקייטרינג.
          </p>
          <div className="hero-actions">
            {user ? (
              <Link to={homePathFor(user.role)} className="btn btn-primary btn-lg">לאזור האישי</Link>
            ) : (
              <>
                <Link to="/register" className="btn btn-primary btn-lg">להזמנת אירוע</Link>
                <Link to="/register-provider" className="btn btn-ghost btn-lg">אני ספק</Link>
              </>
            )}
          </div>
        </div>
        <div className="hero-art" aria-hidden="true">
          <span className="art art-ticket"><small>כרטיס כניסה</small><b>SHOW</b><i>הערב הגדול</i><em>ערב אחד, זיכרון לכל החיים</em></span>
        </div>
      </section>

      <div className="marquee" aria-hidden="true">
        <div className="marquee-track">
          {[0, 1].map(k => ['חתונות', 'בר ובת מצווה', 'ברית', 'אירועי חברה', 'ימי הולדת', 'כנסים', 'חינה', 'הפקות פרטיות'].map(w => <span key={k + w}>{w}</span>))}
        </div>
      </div>

      <section>
        <div className="section-title">
          <h2>שלושה צעדים לאירוע</h2>
          <p className="muted">כל אירוע עובר את אותו מסלול</p>
        </div>
        <div className="grid grid-3 steps">
          <div className="card step t-lemon"><div className="step-num">1</div><h3>בוחרים תאריך</h3><p>הלוח מציג בזמן אמת אילו ימים פנויים. יום אחד, אירוע אחד.</p></div>
          <div className="card step t-pink"><div className="step-num">2</div><h3>בונים את האירוע</h3><p>מוסיפים סעיפים לפי קטגוריה, עם תקציב לכל סעיף.</p></div>
          <div className="card step t-mint"><div className="step-num">3</div><h3>הצוות משבץ ספקים</h3><p>ספקים מאושרים מגישים מועמדות, ואנחנו בוחרים את המתאים ביותר.</p></div>
        </div>
      </section>

      <section>
        <div className="section-title">
          <h2>מה אפשר להזמין</h2>
          <p className="muted">הקטגוריות שבהן עובדים הספקים שלנו</p>
        </div>
        {loading ? <Loader /> : (
          <div className="cat-grid">
            {(categories || []).map((c, i) => (
              <div key={c.id} className={`cat-card ${tints[i % tints.length]}`}>
                <div className={`cat-icon ${shapes[i % shapes.length]}`}>{icons[i % icons.length]}</div>
                <div className="cat-name">{c.name}</div>
              </div>
            ))}
          </div>
        )}
      </section>

      {!user && (
        <section className="cta-band">
          <div>
            <h2>מפיקים? הצטרפו כספקים</h2>
            <p>נרשמים, מאושרים על ידי הצוות, ומגישים מועמדות לסעיפים שמתאימים לכם.</p>
          </div>
          <Link to="/register-provider" className="btn btn-lemon btn-lg">להרשמה כספק</Link>
        </section>
      )}
    </>
  )
}
