# add the simplehooks api scopes
dotnet SimpleIdentityServer.CLI.dll scope add --name "samplelistener_api.sample" --display-name "post data to sample listener api" --resources "samplelistener_api"

# add resource api client to introspect its tokens created by clients
dotnet SimpleIdentityServer.CLI.dll app add --client-id "samplelistener_api" --client-secret "P@ssw0rdP@ssw0rd" --display-name "sample listener resource api" --permissions "ept:introspection"

# add client application to post data to sample api
dotnet SimpleIdentityServer.CLI.dll app add --client-id "client-sample" --client-secret "P@ssw0rdP@ssw0rd" --display-name "sample-listener api client to post data to sample" --permissions "ept:token" --permissions "ept:introspection" --permissions "gt:client_credentials" --permissions "scp:samplelistener_api.sample"
