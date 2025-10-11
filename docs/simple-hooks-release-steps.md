# simple hooks release steps

1. readme file with the new version
2. update SimpleHooks.Web assembly with the new version
3. update SimpleHooks.AuthApi assembly with the new version
4. update SimpleHooks.Server assembly with the new version
5. update SimpleHooks.Assist assembly with the new version
6. commit and push the changes to the main branch
7. publish SimpleHooks.Web for win-x64
8. publish SimpleHooks.AuthApi for win-x64
9. publish SimpleHooks.Server for win-x64
10. publish SimpleHooks.Assist for win-x64
11. Create release with new tag named as the new version.
12. Compress the published SimpleHooks.Web and add it in the release
13. Compress the published SimpleHooks.AuthApi and add it in the release
14. Compress the published SimpleHooks.Server and add it in the release
15. Compress the published SimpleHooks.Assist and add it in the release
16. publish the release
17. build docker image for SimpleHooks.Web with tag named web-new_version
18. push docker image with tag web-new_version
19. build docker image for SimpleHooks.AuthApi with tag named authapi-new_version
20. push docker image with tag authapi-new_version
21. build docker image for SimpleHooks.Server with tag named proc-new_version
22. push docker image with tag proc-new_version
23. tag docker image with web-new_version web-latest
24. push docker image web-latest
25. tag docker image with authapi-new_version authapi-latest
26. push docker image authapi-latest
27. tag docker image with proc-new_version proc-latest
28. push docker image proc-latest
