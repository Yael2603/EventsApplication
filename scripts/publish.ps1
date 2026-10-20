# publish.ps1: מעלה ל-GitHub את העיצוב החדש ואת ה-README, ו-Render מתעדכן לבד.
# הרצה (מתיקיית הפרויקט): .\scripts\publish.ps1

$ErrorActionPreference = 'Stop'                          # עצירה מיידית בכל שגיאה
Set-Location (Split-Path $PSScriptRoot -Parent)          # מעבר לשורש הפרויקט, בלי קשר מאיפה הסקריפט הורץ

git config --global http.sslBackend schannel             # מונע את שגיאת התעודה של נטפרי ב-push

git add README.md EventsApp.Client scripts               # מוסיף רק את הקבצים הרלוונטיים (קובצי ה-txt הזרים נשארים בחוץ)

git diff --cached --quiet                                # בודק אם יש שינויים להעלאה (קוד יציאה 0 = אין)
if ($LASTEXITCODE -eq 0) {
    Write-Host 'אין שינויים חדשים להעלאה.' -ForegroundColor Yellow
} else {
    git commit -m "Luxury client redesign and deployment docs"   # יוצר commit עם ההודעה
    if ($LASTEXITCODE -ne 0) { throw 'ה-commit נכשל' }           # עוצר אם נכשל
}

git push                                                 # שולח ל-GitHub
if ($LASTEXITCODE -ne 0) { throw 'ה-push נכשל' }         # עוצר אם נכשל

Write-Host ''
Write-Host 'הועלה בהצלחה. Render יבנה את האתר מחדש (כמה דקות).' -ForegroundColor Green
git log --oneline -1                                     # מציג את ה-commit האחרון לאימות
