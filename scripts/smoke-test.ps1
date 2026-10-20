# בדיקת עשן מלאה מול השרת הרץ (כמו הבדיקה ב-Swagger). ASCII בלבד בכוונה, כדי להימנע מבעיות קידוד ב-PowerShell.
# הרצה: .\scripts\smoke-test.ps1   (השרת חייב לרוץ; יתבקשו אימייל וסיסמה של המנהל, הסיסמה לא מוצגת ולא נשמרת)
param([string]$Base = "http://localhost:8080")

$ErrorActionPreference = "Stop"
$results = New-Object System.Collections.ArrayList

function Call($method, $path, $body = $null, $token = $null) {          # קריאה אחת לשרת; מחזיר קוד סטטוס ונתונים
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $args2 = @{ Uri = "$Base$path"; Method = $method; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $body) {
        $args2["Body"] = [Text.Encoding]::UTF8.GetBytes(($body | ConvertTo-Json -Depth 6))
        $args2["ContentType"] = "application/json; charset=utf-8"
    }
    try {
        $r = Invoke-WebRequest @args2
        $data = $null
        if ($r.Content) { try { $data = $r.Content | ConvertFrom-Json } catch { } }
        return @{ Status = [int]$r.StatusCode; Data = $data }
    } catch {
        if ($_.Exception.Response) { return @{ Status = [int]$_.Exception.Response.StatusCode; Data = $null } }
        throw "No connection to $Base. Is the server running?"
    }
}

function Check($name, $expected, $actual) {                              # רושם תוצאה אחת
    $ok = ($actual -eq $expected)
    [void]$results.Add([pscustomobject]@{ Step = $name; Expected = $expected; Actual = $actual; Result = $(if ($ok) { "PASS" } else { "FAIL" }) })
    $color = if ($ok) { "Green" } else { "Red" }
    Write-Host ("{0,-4} {1}  (expected {2}, got {3})" -f $(if ($ok) { "PASS" } else { "FAIL" }), $name, $expected, $actual) -ForegroundColor $color
}

$suffix = Get-Random -Minimum 10000 -Maximum 99999
$pw = "Test-" + $suffix + "-pw"                                          # סיסמת בדיקה חד-פעמית למשתמשי הבדיקה בלבד

# --- קטגוריות ולוח ---
$cats = Call GET "/api/categories"
Check "GET categories (public)" 200 $cats.Status
$catId = $cats.Data[0].id

# --- הרשמות ---
$ca = Call POST "/api/auth/register" @{ fullName = "Client A $suffix"; email = "a$suffix@test.com"; password = $pw }
Check "register client A" 201 $ca.Status
$cb = Call POST "/api/auth/register" @{ fullName = "Client B $suffix"; email = "b$suffix@test.com"; password = $pw }
Check "register client B" 201 $cb.Status
$pr = Call POST "/api/auth/register-provider" @{ fullName = "Provider $suffix"; email = "p$suffix@test.com"; password = $pw; businessName = "Test Biz $suffix"; description = "test"; phone = "0500000000"; city = "Haifa"; categoryId = $catId; startingPrice = 1000; acceptTerms = $true }
Check "register provider" 201 $pr.Status
$tA = $ca.Data.token; $tB = $cb.Data.token; $tP = $pr.Data.token
$dup = Call POST "/api/auth/register" @{ fullName = "Client A $suffix"; email = "a$suffix@test.com"; password = $pw }
Check "register same email twice" 409 $dup.Status

# --- הרשאות לפי תפקיד ---
Check "no token -> calendar" 401 (Call GET "/api/calendar?from=2030-01-01&to=2030-01-31").Status
Check "client calls admin stats" 403 (Call GET "/api/admin/stats" $null $tA).Status
Check "provider creates event" 403 (Call POST "/api/events" @{ date = "2030-01-01"; type = "Wedding"; location = "x"; guestCount = 10; items = @(@{ categoryId = $catId; budget = 100 }) } $tP).Status

# --- בחירת תאריך פנוי ---
$from = (Get-Date).AddDays(2).ToString("yyyy-MM-dd"); $to = (Get-Date).AddDays(150).ToString("yyyy-MM-dd")
$cal = Call GET "/api/calendar?from=$from&to=$to" $null $tA
Check "GET calendar" 200 $cal.Status
$free = @($cal.Data | Where-Object { -not $_.isTaken })
if ($free.Count -lt 1) { throw "No free date found in the calendar range." }
$date = ($free[0].date).Substring(0, 10)
Write-Host "Using date $date" -ForegroundColor Cyan

# --- הזמנה כפולה ---
$evBody = @{ date = $date; type = "Wedding"; location = "Test Hall"; guestCount = 150; items = @(@{ categoryId = $catId; budget = 5000; details = "test" }) }
$e1 = Call POST "/api/events" $evBody $tA
Check "client A creates event" 201 $e1.Status
$eventId = $e1.Data.id; $itemId = $e1.Data.items[0].id
$e2 = Call POST "/api/events" $evBody $tB
Check "client B same date (double booking)" 409 $e2.Status
$dayAfter = Call GET "/api/calendar?from=$date&to=$date" $null $tB
Check "day is marked taken" $true ($dayAfter.Data[0].isTaken)
Check "client B cannot read A's event" 403 (Call GET "/api/events/$eventId" $null $tB).Status
Check "client A reads own event" 200 (Call GET "/api/events/$eventId" $null $tA).Status

# --- ספק: לפני אישור ואחרי ---
Check "pending provider opens items" 403 (Call GET "/api/event-items/open" $null $tP).Status
$adminEmail = Read-Host "Admin email"
$sec = Read-Host "Admin password" -AsSecureString
$adminPw = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec))
$al = Call POST "/api/auth/login" @{ email = $adminEmail; password = $adminPw }
Check "admin login" 200 $al.Status
if ($al.Status -ne 200) { Write-Host "Admin login failed, stopping." -ForegroundColor Red; $results | Format-Table -AutoSize; exit 1 }
$tAdmin = $al.Data.token
Check "admin role is Admin" "Admin" $al.Data.role
$plist = Call GET "/api/providers?status=Pending" $null $tAdmin
Check "admin lists pending providers" 200 $plist.Status
$provId = ($plist.Data | Where-Object { $_.email -eq "p$suffix@test.com" }).id
Check "new provider is in the pending list" $true ($null -ne $provId)
Check "admin approves provider" 200 (Call PUT "/api/providers/$provId/status" @{ status = "Approved" } $tAdmin).Status

# --- מועמדות ושיבוץ ---
$open = Call GET "/api/event-items/open" $null $tP
Check "approved provider opens items" 200 $open.Status
Check "our item is in the open list" $true ($null -ne ($open.Data | Where-Object { $_.itemId -eq $itemId }))
$ap = Call POST "/api/candidacies" @{ eventItemId = $itemId; proposedPrice = 4500; message = "test" } $tP
Check "provider applies" 201 $ap.Status
Check "same provider applies twice" 409 (Call POST "/api/candidacies" @{ eventItemId = $itemId; proposedPrice = 4000 } $tP).Status
$cands = Call GET "/api/event-items/$itemId/candidacies" $null $tAdmin
Check "admin sees candidates" 200 $cands.Status
Check "admin assigns provider" 200 (Call PUT "/api/event-items/$itemId/provider" @{ providerId = $provId } $tAdmin).Status
$bk = Call GET "/api/providers/me/bookings" $null $tP
Check "provider sees the booking" $true ($null -ne ($bk.Data | Where-Object { $_.itemId -eq $itemId }))
Check "client cannot remove assigned item" 409 (Call DELETE "/api/event-items/$itemId" $null $tA).Status
Check "provider cannot withdraw after being chosen" 409 (Call DELETE "/api/candidacies/$($ap.Data.id)" $null $tP).Status

# --- בקשת מחיקה ---
Check "short reason is rejected" 400 (Call POST "/api/events/$eventId/deletion-request" @{ reason = "no" } $tA).Status
Check "client B requests deletion of A's event" 403 (Call POST "/api/events/$eventId/deletion-request" @{ reason = "I want to delete this event" } $tB).Status
$dr = Call POST "/api/events/$eventId/deletion-request" @{ reason = "Client cancelled the event - smoke test" } $tA
Check "client A requests deletion" 201 $dr.Status
Check "second pending request" 409 (Call POST "/api/events/$eventId/deletion-request" @{ reason = "Client cancelled the event - again" } $tA).Status
$drList = Call GET "/api/deletion-requests?status=Pending" $null $tAdmin
$drId = ($drList.Data | Where-Object { $_.eventId -eq $eventId }).id
Check "admin sees the request" $true ($null -ne $drId)
Check "admin approves deletion" 204 (Call PUT "/api/deletion-requests/$drId/approve" $null $tAdmin).Status

# --- אחרי המחיקה ---
$dayFree = Call GET "/api/calendar?from=$date&to=$date" $null $tB
Check "day is free again after deletion" $false ($dayFree.Data[0].isTaken)
Check "event no longer exists" 404 (Call GET "/api/events/$eventId" $null $tA).Status
$nA = Call GET "/api/notifications" $null $tA
$nP = Call GET "/api/notifications" $null $tP
Check "client A got a notification" $true (@($nA.Data).Count -ge 1)
Check "assigned provider got a notification" $true (@($nP.Data).Count -ge 1)
Check "client B now books the freed date" 201 (Call POST "/api/events" $evBody $tB).Status

Write-Host ""
$fail = @($results | Where-Object { $_.Result -eq "FAIL" })
Write-Host ("Done: {0} checks, {1} failed." -f $results.Count, $fail.Count) -ForegroundColor $(if ($fail.Count -eq 0) { "Green" } else { "Red" })
if ($fail.Count -gt 0) { $fail | Format-Table -AutoSize }
