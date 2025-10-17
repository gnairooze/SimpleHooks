using System;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Log.Console;
using SimpleTools.SimpleHooks.Repo.SQL;
using SimpleTools.SimpleHooks.TestSimpleHooks.Repos;

namespace SimpleTools.SimpleHooks.TestSimpleHooks
{
    internal class TestBusinessDefinitionManager
    {
        public static void TestLoadDefinition1()
        {
            var logger = new Logger();

            DefinitionManager manager = new(logger, new EventDefRepo(), new ListenerDefRepo(), new EventDefListenerDefRepo(), new AppOptionsRepo(), new ListenerTypeDataRepo(), new ConnectionRepo(), new ListenerPluginManager(logger));

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
