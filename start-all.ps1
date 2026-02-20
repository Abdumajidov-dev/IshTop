# IshTop Start Script
Write-Host "🚀 IshTop - Hammasi ishga tushmoqda..." -ForegroundColor Green

# Start Admin Panel
Write-Host "📱 Admin Next.js (port 3000) ishga tushurilmoqda..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd admin; npm run dev"

# Start API
Write-Host "🔧 API (.NET) (port 5000) ishga tushurilmoqda..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/IshTop.Api; dotnet run --urls=http://localhost:5000"

# Start Bot (Uncomment when ready)
# Write-Host "🤖 Bot ishga tushurilmoqda..." -ForegroundColor Cyan
# Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd src/IshTop.Bot; dotnet run"

Write-Host "✅ Barcha servicalar ishga tushdi!" -ForegroundColor Green
Write-Host "📍 Admin Panel: http://localhost:3000"
Write-Host "📍 API: http://localhost:5000"
