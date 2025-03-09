#!/bin/bash

cat > /usr/share/nginx/html/config.json <<EOF
{
    "AppName": "${AppName}",
	"UserToken": "${UserToken}",
	"AdminToken": "${AdminToken}",
	"Realtime": "${Realtime}",
	"TimeOffset": "${TimeOffset}",
	"WebAPIPort": "${WebAPIPort}",
	"WebAPIHost": "${WebAPIHost}",
	"AuthToken": "${AuthToken}",
	"Context": "${Context}",
	"LazyLoading": "${LazyLoading}"
}
EOF

nginx -g "daemon off;"
