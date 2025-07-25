@echo off
REM FlightBoard Environment Configuration Script
REM Easily switch between HTTP/HTTPS ports and reverse proxy configurations

echo 🔧 FlightBoard Environment Configuration
echo ==========================================

:menu
echo.
echo Choose your configuration:
echo 1. Local Development HTTP (port 5183)
echo 2. Local Development HTTPS (port 7022)
echo 3. Auto-detect (recommended for local testing)
echo 4. Reverse Proxy (API on same domain)
echo 5. Production (HTTPS)
echo 6. Custom Configuration
echo 7. Exit
echo.
set /p choice="Enter your choice (1-7): "

if "%choice%"=="1" goto http_config
if "%choice%"=="2" goto https_config
if "%choice%"=="3" goto auto_config
if "%choice%"=="4" goto proxy_config
if "%choice%"=="5" goto prod_config
if "%choice%"=="6" goto custom_config
if "%choice%"=="7" goto exit
echo Invalid choice. Please try again.
goto menu

:http_config
echo.
echo 🔨 Setting up Local HTTP Configuration (port 5183)...
set API_URL=http://localhost:5183
set SIGNALR_URL=http://localhost:5183/flighthub
set USE_PROXY=false
goto apply_config

:https_config
echo.
echo 🔒 Setting up Local HTTPS Configuration (port 7022)...
set API_URL=https://localhost:7022
set SIGNALR_URL=https://localhost:7022/flighthub
set USE_PROXY=false
goto apply_config

:auto_config
echo.
echo 🤖 Setting up Auto-Detection (recommended)...
echo This will automatically detect the correct port based on how you access the frontend.
set API_URL=
set SIGNALR_URL=
set USE_PROXY=false
goto apply_config

:proxy_config
echo.
echo � Setting up Reverse Proxy Configuration...
echo This assumes your API is served from the same domain as the frontend.
set API_URL=
set SIGNALR_URL=
set USE_PROXY=true
goto apply_config

:prod_config
echo.
echo �🚀 Setting up Production Configuration (HTTPS)...
set /p domain="Enter your domain (e.g., api.yourdomain.com): "
if "%domain%"=="" (
    echo ❌ Domain cannot be empty
    goto menu
)
set API_URL=https://%domain%
set SIGNALR_URL=https://%domain%/flighthub
set USE_PROXY=false
goto apply_config

:custom_config
echo.
echo ⚙️ Custom Configuration...
set /p API_URL="Enter API URL (leave empty for auto-detection): "
set /p SIGNALR_URL="Enter SignalR URL (leave empty for auto-detection): "
set /p USE_PROXY="Use reverse proxy? (true/false, default false): "
if "%USE_PROXY%"=="" set USE_PROXY=false
goto apply_config

:apply_config
echo.
echo 📝 Applying configuration:
if "%API_URL%"=="" (
    echo    API URL: Auto-detection enabled
) else (
    echo    API URL: %API_URL%
)
if "%SIGNALR_URL%"=="" (
    echo    SignalR URL: Auto-detection enabled
) else (
    echo    SignalR URL: %SIGNALR_URL%
)
echo    Reverse Proxy: %USE_PROXY%
echo.

REM Update Consumer App .env
echo # FlightBoard Consumer App Environment Variables > ..\src\frontend\consumer\.env
echo # Configuration applied by configure-env.cmd >> ..\src\frontend\consumer\.env
echo # >> ..\src\frontend\consumer\.env
if not "%API_URL%"=="" (
    echo REACT_APP_API_URL=%API_URL% >> ..\src\frontend\consumer\.env
) else (
    echo # REACT_APP_API_URL= # Auto-detection enabled >> ..\src\frontend\consumer\.env
)
if not "%SIGNALR_URL%"=="" (
    echo REACT_APP_SIGNALR_URL=%SIGNALR_URL% >> ..\src\frontend\consumer\.env
) else (
    echo # REACT_APP_SIGNALR_URL= # Auto-detection enabled >> ..\src\frontend\consumer\.env
)
if "%USE_PROXY%"=="true" (
    echo REACT_APP_USE_REVERSE_PROXY=true >> ..\src\frontend\consumer\.env
) else (
    echo # REACT_APP_USE_REVERSE_PROXY=false >> ..\src\frontend\consumer\.env
)
echo REACT_APP_ENV=development >> ..\src\frontend\consumer\.env
echo REACT_APP_DEBUG=true >> ..\src\frontend\consumer\.env

REM Update Backoffice App .env
echo # FlightBoard Backoffice App Environment Variables > ..\src\frontend\backoffice\.env
echo # Configuration applied by configure-env.cmd >> ..\src\frontend\backoffice\.env
echo # >> ..\src\frontend\backoffice\.env
if not "%API_URL%"=="" (
    echo REACT_APP_API_URL=%API_URL% >> ..\src\frontend\backoffice\.env
) else (
    echo # REACT_APP_API_URL= # Auto-detection enabled >> ..\src\frontend\backoffice\.env
)
if not "%SIGNALR_URL%"=="" (
    echo REACT_APP_SIGNALR_URL=%SIGNALR_URL% >> ..\src\frontend\backoffice\.env
) else (
    echo # REACT_APP_SIGNALR_URL= # Auto-detection enabled >> ..\src\frontend\backoffice\.env
)
if "%USE_PROXY%"=="true" (
    echo REACT_APP_USE_REVERSE_PROXY=true >> ..\src\frontend\backoffice\.env
) else (
    echo # REACT_APP_USE_REVERSE_PROXY=false >> ..\src\frontend\backoffice\.env
)
echo REACT_APP_ENV=development >> ..\src\frontend\backoffice\.env
echo REACT_APP_DEBUG=true >> ..\src\frontend\backoffice\.env

echo ✅ Configuration applied successfully!
echo.
echo � Configuration Tips:
if "%API_URL%"=="" (
    echo    • Auto-detection is enabled - frontend will automatically choose:
    echo      - HTTPS localhost:7022 when frontend is accessed via HTTPS
    echo      - HTTP localhost:5183 when frontend is accessed via HTTP
)
if "%USE_PROXY%"=="true" (
    echo    • Reverse proxy mode enabled - API calls will go to same domain as frontend
)
echo    • Check browser console for actual detected configuration
echo    • Restart frontend applications for changes to take effect
echo.
pause
goto menu

:exit
echo.
echo 👋 Configuration complete. Happy coding!
exit /b 0
