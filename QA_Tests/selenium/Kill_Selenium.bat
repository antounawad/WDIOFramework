rem for /f "tokens=5" %a in ('netstat -ano ^| findstr :'%1) do taskkill /PID %a /T /F
for /f "tokens=5" %a in ('netstat -ano ^| findstr :4015') do taskkill /PID %a /T /F