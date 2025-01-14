#!/bin/bash

cat > /usr/share/nginx/html/config.json <<EOF
{
    "AppName": "${AppName}",
	"UserToken": "${UserToken}",
	"AdminToken": "${AdminToken}",
	"Realtime": "${Realtime}",
	"TimeOffset": "${TimeOffset}",
	"WebAPI__Port": "${WebAPI__Port}",
	"WebAPI__Host": "${WebAPI__Host}"
}
EOF

nginx -g "daemon off;"
