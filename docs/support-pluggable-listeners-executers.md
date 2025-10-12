# support pluggable listener executers

## Overview

allow simple hooks to call authenticated endpoints. allow multiple authentication methods using plugins approach. first will support bearer token.

## technical details

the listener execution logic in Business/InstanceManager.cs method ExecuteListener will be refactored to be based on plugins.
every listener definition will have a type that will state how listener execution is handled by which plugin.

when the SimpleHooks.Server starts, it will load all the listener plugins defined in listener definition type table `Path` field - example `plugins/TypeA/TypeAListener.dll`. it will create an instance of every plugin and store it in a dictionary with key as type name and value as plugin instance.

when a listener is executed, the InstanceManager.cs will look up the plugin instance from the dictionary using the listener definition type. it will then call the ExecuteAsync method on the plugin instance, passing in the event data, listener headers, timeout, and options.

the plugin will handle the execution logic and return the result to the InstanceManager.cs.

when the InstanceManager.cs receives the result, it will process the result and save the logs based on its configuration.

every plugin has its own folder under the server listener-plugins folder. the plugin folder contains the dll and its dependencies. for example:

```text
plugins/TypeA/TypeAListener.dll
plugins/TypeA/dependency1.dll 
plugins/TypeA/dependency2.dll 
```

for now, two types: Anonymous and TypeA.

an anonymous listener plugin will have the same logic as current listener execution logic. this logic will be removed from InstanceManager.cs and moved to the anonymous listener plugin.

the plugin must implement `IListenerExecute` interface that will create in a new project named SimpleHooks.ListenerExecuterInterfaces and its default namespace will be SimpleTools.SimpleHooks.SimpleExecuterInterfaces. the interface has a method ExecuteAsync which takes event data as `string`, listener headers as `List<string>`, timeout as `int`, and options as json `string`. and returns a `Task<ListenerResult>`. `ListenerResult` has Succeeded as `bool`, Message as `string` and Logs as `OrderedList<LogModel>`.

the parameters for ExecuteAsync are as follows:

- event data: the data to be processed by the listener
- listener headers: any headers that should be included in the listener request. it is read from listener definition
- timeout: the maximum time in minutes to wait for the listener to complete. it is read from listener definition
- options: any additional options for the listener execution such as auth parameters. it is read from listener definition `Type_Options` field which carries an environment variable name. the SimpleHooks.Server will read the environment variable value and pass it to the plugin as options parameter. this way, the auth parameters are not stored in listener definition directly for security reasons. the options parameter is a json string that can be deserialized to a dictionary or a custom object in the plugin.
- listener result: the result of the listener execution. it contains whether the execution succeeded, any message, and any logs.

In TypeA listener plugin, the ExecuteAsync method will first read the options parameter and deserialize it to get the following auth parameters:

1. Identity Provider Url
2. Client Id
3. Client Secret
4. Scope: optional

then it will call the identity provider url with client id, client secret, and scope if exists to get the token. then it will add the token to listener headers as `Authorization: Bearer <token>`. then it will call the listener endpoint with event data, listener headers, and timeout. finally, it will return the listener result.

## technology stack

- .net 8
- HttpClient.Simple for making http calls
- System.Text.Json for json serialization and deserialization
