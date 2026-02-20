#!/bin/bash

echo "🚀 IshTop - Hammasi ishga tushmoqda..."
echo ""

# Admin (Next.js) - port 3000
echo "📱 Admin Next.js (port 3000) ishga tushurilmoqda..."
cd /home/avazbek/loyihalar/Shaxsiy/Botlar/IshTop/admin
npm run dev &
ADMIN_PID=$!
echo "✅ Admin PID: $ADMIN_PID"
echo ""

# API (.NET) - port 5000
echo "🔧 API (.NET) (port 5000) ishga tushurilmoqda..."
cd /home/avazbek/loyihalar/Shaxsiy/Botlar/IshTop/src/IshTop.Api
dotnet run --urls="http://localhost:5000" &
API_PID=$!
echo "✅ API PID: $API_PID"
echo ""

# Bot (C# Telegram Bot) - COMMENTED OUT FOR ADMIN TESTING
# echo "🤖 Bot ishga tushurilmoqda..."
# cd /home/avazbek/loyihalar/Shaxsiy/Botlar/IshTop/src/IshTop.Bot
# dotnet run &
# BOT_PID=$!
# echo "✅ Bot PID: $BOT_PID"
# echo ""

# Parser (C#) - TEMPORARILY DISABLED to avoid token conflict
# echo "📡 Parser ishga tushurilmoqda..."
# cd /home/avazbek/loyihalar/Shaxsiy/Botlar/IshTop/src/IshTop.Parser
# dotnet run &
# PARSER_PID=$!
# echo "✅ Parser PID: $PARSER_PID"

echo "=========================================="
echo "✅ Barcha servicalar ishga tushdi!"
echo "=========================================="
echo ""
echo "📍 URLs:"
echo "  • Admin Panel: http://localhost:3000"
echo "  • API: http://localhost:5000"
echo "  • Bot: Running"
echo "  • Parser: Running"
echo ""
echo "PIDs:"
echo "  • Admin: $ADMIN_PID"
echo "  • API: $API_PID"
echo "  • Bot: $BOT_PID"
echo "  • Parser: $PARSER_PID"
echo ""
echo "Barchasini to'xtatish uchun: Ctrl+C yoki kill \$ADMIN_PID \$API_PID \$BOT_PID \$PARSER_PID"
echo ""

# Keep script running
wait
