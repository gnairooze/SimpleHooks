using System;

namespace TestSimpleHooks
{
    internal class TestBusinessDefinitionManager
    {
        public static void TestLoadDefinition1()
        {
            Business.DefinitionManager manager = new(new Log.Console.Logger(), new Repos.EventDefRepo(), new Repos.ListenerDefRepo(), new Repos.EventDefListenerDefRepo(), new Repos.AppOptionsRepo(), new Repos.ConnectionRepo());

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
