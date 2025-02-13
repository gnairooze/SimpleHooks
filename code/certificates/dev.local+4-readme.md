# how the certificate created

## certificate info

### certificate created for the following domains:

```
dev.local
*.dev.local
localhost
127.0.0.1
::1
```

### pfx passwod
YourSecurePassword123!

## steps to create the certificate
### 1. open powershell terminal as admin


### 2. install openssl
```shell
choco install openssl
```

### 3. install mkcert
```shell
choco install mkcert
```

### 4. install local certificate authority
```shell
mkcert -install
```

### 5. generate certificate
```shell
mkcert dev.local "*.dev.local" localhost 127.0.0.1 ::1
```

2 files will be created:
`dev.local+4.pem`
`dev.local+4-key.pem`

### 6. convert PEM files to PFX
```shell
openssl pkcs12 -export -in dev.local+4.pem -inkey dev.local+4-key.pem -out dev.local+4.pfx -passout pass:"YourSecurePassword123!"
```
