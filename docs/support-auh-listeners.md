# support authenticated listeners

## Overview

allow simple hooks to call authenticated endpoints. allow multiple authentication methods. first will support bearer token.

## authentication parameters

1. Identity Provider Url
2. Client Id
3. Client Secret
4. Scope: optional

## business

check for auth type in listener config. if null, the anonymous call of the listener will be executed.
if bearer token, call identity provider url with client id, client secret, and scope if exists to get token. then add the token to request header of the listener. then call the listener.

for now, if other auth type, return error "auth type not supported".
