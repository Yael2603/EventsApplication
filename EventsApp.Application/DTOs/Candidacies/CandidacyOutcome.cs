namespace EventsApp.Application.DTOs.Candidacies;

public enum CandidacyOutcome                        // תוצאת מועמדות (מחושבת, לא נשמרת ב-DB)
{
    Open,                                           // עוד לא נבחר ספק לסעיף
    Won,                                            // נבחרתי
    Lost                                            // נבחר ספק אחר
}
