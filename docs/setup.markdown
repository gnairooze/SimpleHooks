---
layout: default
title: How to Setup Simple Hooks
---

## Prerequistes

1. dot net 5.0 framework

## How to Setup Simple Hooks

Simple Hooks contains 2 components:

1. Web API to trigger events. You need to host it under web server that support dot net 5.0 (IIS, Apache2, Nginx ...)
2. Console Application to run in a scheduler (windows task scheduler or cron demon).
3. SQL Server with DB for Simple Hooks storage. Run the scripts in the [repository path](https://github.com/gnairooze/SimpleHooks/tree/main/code/SQL).

---

### See also

[Home](/index)
