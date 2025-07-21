#!/bin/bash

echo "📦 Publishing ZenithPiStartUp for Raspberry Pi..."

dotnet publish ZenithPiStartUp \
  -c Release \
  -r linux-arm64 \
  --self-contained true \
  -o ./ZenithDeploy

echo "🚚 Pushing to ZenithPi..."

scp -r ./ZenithDeploy/* pi@zenithpi.local:~/ZenithRun/

echo "🔄 Restarting Zenith on Pi..."

ssh pi@zenithpi.local << 'EOF'
  pkill -f ZenithPiStartUp || true
  cd ~/ZenithRun/
  ./ZenithPiStartUp &
  disown
EOF

echo 