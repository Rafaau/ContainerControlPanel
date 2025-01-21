#!/bin/bash

cat > /usr/share/nginx/html/config.json <<EOF
{
    "AppName": "${AppName}",
	"UserToken": "${UserToken}",
	"AdminToken": "${AdminToken}",
	"Realtime": "${Realtime}",
	"TimeOffset": "${TimeOffset}",
	"WebAPIPort": "${WebAPIPort}",
	"WebAPIHost": "${WebAPIHost}"	
}
EOF

nginx -g "daemon off;"
