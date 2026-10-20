# הכנה/איפוס מלא למחשב חדש או נקי: יוצר את מסד הנתונים המקומי מחדש (מוחק את הקיים!) ומגדיר את כל ה-User Secrets:
# Jwt:Key, מחרוזת חיבור ופרטי מנהל. הרצה מתיקיית הפתרון:  .\scripts\reset-all.ps1
# הסיסמאות נקלטות בלי הצגה ונשמרות רק ב-Docker וב-User Secrets, לא בקבצי הפרויקט.
$ErrorActionPreference = "Stop"
function Read-Secret($prompt) {
    $s = Read-Host $prompt -AsSecureString
    return [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s))
}
$P = "EventsApp.API"
docker info *> $null
if ($LASTEXITCODE -ne 0) { throw "Docker is not running. Open Docker Desktop and wait until the engine is running." }

Write-Host "This DELETES the local database container 'eventsapp-db' (if it exists) and ALL its data." -ForegroundColor Red
$answer = Read-Host "Type YES to continue"
if ($answer -ne "YES") { Write-Host "Cancelled."; exit 0 }

Write-Host "Make sure the API is stopped in Visual Studio (Shift+F5) before continuing." -ForegroundColor Yellow
Read-Host "Press Enter when the API is stopped"

docker rm -f eventsapp-db *> $null
docker volume rm eventsapp-data *> $null

$dbPw = Read-Secret "New Postgres password"
docker run --name eventsapp-db -e POSTGRES_PASSWORD=$dbPw -e POSTGRES_DB=eventsapp -p 5432:5432 -v eventsapp-data:/var/lib/postgresql/data -d postgres:16 | Out-Null
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventsapp;Username=postgres;Password=$dbPw" --project $P | Out-Null

$b = New-Object byte[] 48; (New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($b)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($b)) --project $P | Out-Null

$adminEmail = Read-Host "New admin email"
$adminPw = Read-Secret "New admin password (at least 8 characters)"
$adminName = Read-Host "Admin full name"
dotnet user-secrets set "Seed:AdminEmail" $adminEmail --project $P | Out-Null
dotnet user-secrets set "Seed:AdminPassword" $adminPw --project $P | Out-Null
dotnet user-secrets set "Seed:AdminFullName" $adminName --project $P | Out-Null

Start-Sleep -Seconds 8
Write-Host "Secrets set (names only):" -ForegroundColor Green
dotnet user-secrets list --project $P | ForEach-Object { "  " + ($_ -split ' = ')[0] }
docker ps --filter "name=eventsapp-db" --format "eventsapp-db: {{.Status}}"
Write-Host "Done. Start the API now (F5); tables and the admin are created on startup. Then run .\scripts\smoke-test.ps1" -ForegroundColor Green
