Console.WriteLine("=== Debug Started ===");

string url = "";
List<string> headers = [
    "",
    "" ];
int timeoutInMinutes = 3;
string eventData = "Test Event Data";
string typeOptionsValue = "{}";

RunAnonymousListener();
RunTypeAListener();

Console.WriteLine("=== Debug Completed ===");

return;
    
void RunAnonymousListener()
{
    var anonymousListener = new SimpleTools.SimpleHooks.ListenerPlugins.Anonymous.AnonymousListener()
    {
        Url = url,
        Headers = headers,
        Timeout = timeoutInMinutes
    };

    var listenerInstanceId = 1;

    var task = anonymousListener.ExecuteAsync(listenerInstanceId, eventData, typeOptionsValue);

    task.Wait();

    Console.WriteLine(task.Result.ToString());
}

void RunTypeAListener()
{
    var anonymousListener = new SimpleTools.SimpleHooks.ListenerPlugins.TypeA.TypeAListener()
    {
        Url = url,
        Headers = headers,
        Timeout = timeoutInMinutes
    };

    var listenerInstanceId = 1;

    var task = anonymousListener.ExecuteAsync(listenerInstanceId, eventData, typeOptionsValue);

    task.Wait();

    Console.WriteLine(task.Result.ToString());
}


