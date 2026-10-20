import { useState } from 'react'
import { candidaciesApi } from '../../api/services'
import { Badge, ConfirmModal, EmptyState, ErrorBox, Loader, PageHeader } from '../../components/ui'
import { useToast } from '../../components/ToastContext'
import { eventTypeLabels, formatDate, formatDateTime, formatMoney, getErrorMessage, outcomeLabels } from '../../utils/format'
import useLoad from '../../utils/useLoad'

const tones = { Open: 'violet', Won: 'green', Lost: 'red' }     // צבע התגית לפי תוצאה

export default function MyCandidacies() {               // המועמדויות של הספק והתוצאה של כל אחת
  const toast = useToast()
  const { data, loading, error, reload } = useLoad(() => candidaciesApi.mine(), [])   // GET /api/candidacies/my
  const [target, setTarget] = useState(null)            // מועמדות לביטול
  const [busy, setBusy] = useState(false)

  const withdraw = async () => {
    setBusy(true)
    try {
      await candidaciesApi.withdraw(target.id)          // DELETE /api/candidacies/{id}
      toast.success('המועמדות בוטלה')
      setTarget(null)
      await reload()
    } catch (err) {
      toast.error(getErrorMessage(err))
    } finally {
      setBusy(false)
    }
  }

  return (
    <>
      <PageHeader title="המועמדויות שלי" subtitle="מעקב אחר כל מה שהגשתם" />
      <ErrorBox message={error} onRetry={reload} />
      {loading ? <Loader /> : (data || []).length === 0 && !error ? (
        <EmptyState icon="📨" title="לא הגשתם עדיין מועמדויות" text="חפשו סעיפים פתוחים והגישו מועמדות." />
      ) : (
        <div className="stack">
          {(data || []).map((c) => (
            <div className="item-row" key={c.id}>
              <div>
                <strong>{c.categoryName}</strong> · {eventTypeLabels[c.eventType]} · {c.location}
                <div className="muted small">{formatDate(c.eventDate)} · הצעה: <span className="price">{formatMoney(c.proposedPrice)}</span> · הוגשה {formatDateTime(c.createdAtUtc)}</div>
                {c.message && <div className="muted small">"{c.message}"</div>}
              </div>
              <div className="row">
                <Badge tone={tones[c.outcome]}>{outcomeLabels[c.outcome]}</Badge>
                {c.outcome !== 'Won' && <button className="btn btn-danger btn-sm" onClick={() => setTarget(c)}>ביטול מועמדות</button>}
              </div>
            </div>
          ))}
        </div>
      )}
      {target && <ConfirmModal title="ביטול מועמדות" danger busy={busy} confirmText="ביטול מועמדות" onClose={() => setTarget(null)}
        text={`לבטל את המועמדות לסעיף ${target.categoryName} בתאריך ${formatDate(target.eventDate)}?`} onConfirm={withdraw} />}
    </>
  )
}
