# מכין מסד נתונים מקומי ומגדיר את מחרוזת החיבור ב-User Secrets, בפקודה אחת.
# הרצה מתיקיית הפתרון:  .\scripts\setup-db.ps1
# הסיסמה נקלטת בלי הצגה, נשמרת רק ב-Docker וב-User Secrets, ולא נכתבת לאף קובץ בפרויקט.
$ErrorActionPreference = "Stop"
docker info *> $null
if ($LASTEXITCODE -ne 0) { throw "Docker is not running. Open Docker Desktop and wait until the engine is running." }

$exists = docker ps -a --filter "name=^eventsapp-db$" --format "{{.Names}}"
if ($exists -eq "eventsapp-db") {
    Write-Host "Container eventsapp-db exists. Starting it..." -ForegroundColor Cyan
    docker start eventsapp-db | Out-Null
    Write-Host "Started. Enter the SAME password that was used when it was created." -ForegroundColor Yellow
} else {
    Write-Host "Container eventsapp-db does not exist. Creating it..." -ForegroundColor Cyan
}
$sec = Read-Host "Postgres password" -AsSecureString
$pw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
if ($exists -ne "eventsapp-db") {
    docker run --name eventsapp-db -e POSTGRES_PASSWORD=$pw -e POSTGRES_DB=eventsapp -p 5432:5432 -v eventsapp-data:/var/lib/postgresql/data -d postgres:16 | Out-Null
}
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventsapp;Username=postgres;Password=$pw" --project EventsApp.API | Out-Null
Start-Sleep -Seconds 5
docker ps --filter "name=eventsapp-db" --format "eventsapp-db: {{.Status}}"
Write-Host "Done. Restart the API (Shift+F5, then F5) and tell Claude." -ForegroundColor Green
