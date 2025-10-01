using System;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Log.Console;
using SimpleTools.SimpleHooks.TestSimpleHooks.Repos;

namespace SimpleTools.SimpleHooks.TestSimpleHooks
{
    internal class TestBusinessDefinitionManager
    {
        public static void TestLoadDefinition1()
        {
            DefinitionManager manager = new(new Logger(), new EventDefRepo(), new ListenerDefRepo(), new EventDefListenerDefRepo(), new AppOptionsRepo(), new ConnectionRepo());

            manager.LoadDefinitions();

            Console.WriteLine("App Options");
            Console.WriteLine("------------");
            manager.AppOptions.ForEach(e => Console.WriteLine(e.Name));
            Console.WriteLine("++++++++++++");

            Console.WriteLine("Event Defs");
            Console.WriteLine("------------");
            manager.EventDefinitions.ForEach(e => Console.WriteLine(e.Name));
            Console.WriteLine("++++++++++++");

            Console.WriteLine("Listener Defs");
            Console.WriteLine("------------");
            manager.ListenerDefinitions.ForEach(e => Console.WriteLine(e.Name));
            Console.WriteLine("++++++++++++");

            Console.WriteLine("Event Def Listener Def Relations");
            Console.WriteLine("------------");
            manager.EventDefinitionListenerDefinitionRelations.ForEach(e => Console.WriteLine($"{e.EventDefinitionId} -> {e.ListenerDefinitionId}"));
            Console.WriteLine("++++++++++++");
        }
    }
}
