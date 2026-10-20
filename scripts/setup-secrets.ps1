# משחזר את כל ה-User Secrets הדרושים להרצה מקומית, בבת אחת. הסיסמאות נקלטות בלי הצגה ולא נכתבות לקבצי הפרויקט.
# הרצה מתיקיית הפתרון:  .\scripts\setup-secrets.ps1
$ErrorActionPreference = "Stop"
function Read-Secret($prompt) {
    $s = Read-Host $prompt -AsSecureString
    return [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($s))
}
$P = "EventsApp.API"

# 1. מפתח JWT אקראי (לא מוצג)
$b = New-Object byte[] 48; (New-Object Security.Cryptography.RNGCryptoServiceProvider).GetBytes($b)
dotnet user-secrets set "Jwt:Key" ([Convert]::ToBase64String($b)) --project $P | Out-Null

# 2. מחרוזת חיבור: אותה סיסמה שבה נוצר הקונטיינר eventsapp-db
$dbPw = Read-Secret "Postgres password (the one used when eventsapp-db was created)"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=eventsapp;Username=postgres;Password=$dbPw" --project $P | Out-Null

# 3. המנהל
$email = Read-Host "Admin email"
$apw = Read-Secret "Admin password (at least 8 characters)"
$name = Read-Host "Admin full name"
dotnet user-secrets set "Seed:AdminEmail" $email --project $P | Out-Null
dotnet user-secrets set "Seed:AdminPassword" $apw --project $P | Out-Null
dotnet user-secrets set "Seed:AdminFullName" $name --project $P | Out-Null

Write-Host "Secrets set (names only):" -ForegroundColor Green
dotnet user-secrets list --project $P | ForEach-Object { "  " + ($_ -split ' = ')[0] }
docker ps -a --filter "name=eventsapp-db" --format "eventsapp-db: {{.Status}}"
Write-Host "Now restart the API (Shift+F5, then F5)." -ForegroundColor Green
