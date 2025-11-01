using System;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Log.Interface;
using SimpleTools.SimpleHooks.Models.Instance;
using SimpleTools.SimpleHooks.Repo.SQL;
using SimpleTools.SimpleHooks.HttpClient.Simple;

namespace SimpleTools.SimpleHooks.TestSimpleHooks
{
    internal class TestActualOperation
    {
        public static void TestAddEventInstance1()
        {
            string connectionString = "Data Source=.;Initial Catalog=SimpleHooks;Integrated Security=SSPI;";
            var logger = new Log.Console.Logger() { MinLogType = LogModel.LogTypes.Debug };

            Business.InstanceManager manager = new (
                logger,
                new SqlConnectionRepo() { ConnectionString = connectionString },
                new EventInstanceDataRepo(),
                new ListenerInstanceDataRepo(),
                new SimpleClient(),
                new EventDefinitionDataRepo(),
                new ListenerDefinitionDataRepo(),
                new EventIistenerDefinitionDataRepo(),
                new ListenerTypeDataRepo(),
                new ListenerPluginManager(logger),
                new AppOptionDataRepo());

            var instance = manager.Add(new EventInstance() { 
                Active = true,
                BusinessId = Guid.NewGuid(),
                CreateBy = "test.user",
                CreateDate = DateTime.UtcNow,
                EventData = "{ 'test' : 'value' }",
                EventDefinitionId = 1,
                ModifyBy = "test.user",
                ModifyDate = DateTime.UtcNow,
                Notes = "test notes",
                ReferenceName = "test-reference",
                ReferenceValue = "test-ref-value",
                Status = Models.Instance.Enums.EventInstanceStatus.InQueue
            });

            Console.WriteLine(instance.ToString());
        }

        public static void TestAddEventInstance2()
        {
            string connectionString = "Data Source=.;Initial Catalog=SimpleHooks;Integrated Security=SSPI;";

            var logger = new Log.SQL.Logger()
            {
                MinLogType = LogModel.LogTypes.Debug,
                ConnectionString = connectionString,
                FunctionName = "SimpleHooks_Log_Add"
            };

            Business.InstanceManager manager = new(
                logger,
                new SqlConnectionRepo() { ConnectionString = connectionString },
                new EventInstanceDataRepo(),
                new ListenerInstanceDataRepo(),
                new SimpleClient(),
                new EventDefinitionDataRepo(),
                new ListenerDefinitionDataRepo(),
                new EventIistenerDefinitionDataRepo(),
                new ListenerTypeDataRepo(),
                new ListenerPluginManager(logger),
                new AppOptionDataRepo());

            var instance = manager.Add(new EventInstance()
            {
                Active = true,
                BusinessId = Guid.NewGuid(),
                CreateBy = "test.user",
                CreateDate = DateTime.UtcNow,
                EventData = "{ 'test' : 'value2' }",
                EventDefinitionId = 1,
                ModifyBy = "test.user",
                ModifyDate = DateTime.UtcNow,
                Notes = "test notes",
                ReferenceName = "test-reference",
                ReferenceValue = "test-ref-value",
                Status = Models.Instance.Enums.EventInstanceStatus.InQueue
            });

            Console.WriteLine(instance.ToString());
        }
        public static void TestProcess1()
        {
            string connectionString = "Data Source=.;Initial Catalog=SimpleHooks;Integrated Security=SSPI;";

            var logger = new Log.Console.Logger() { MinLogType = Log.Interface.LogModel.LogTypes.Debug };

            Business.InstanceManager manager = new(
                logger,
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = connectionString },
                new Repo.SQL.EventInstanceDataRepo(),
                new Repo.SQL.ListenerInstanceDataRepo(),
                new HttpClient.Simple.SimpleClient(),
                new Repo.SQL.EventDefinitionDataRepo(),
                new Repo.SQL.ListenerDefinitionDataRepo(),
                new Repo.SQL.EventIistenerDefinitionDataRepo(),
                new Repo.SQL.ListenerTypeDataRepo(),
                new Business.ListenerPluginManager(logger),
                new Repo.SQL.AppOptionDataRepo());

            var instances = manager.GetEventInstancesToProcess(DateTime.UtcNow);
            manager.Process(instances);
        }

        public static void TestProcess2()
        {
            string connectionString = "Data Source=.;Initial Catalog=SimpleHooks;Integrated Security=SSPI;";

            var logger = new Log.SQL.Logger()
            {
                MinLogType = LogModel.LogTypes.Information,
                ConnectionString = connectionString,
                FunctionName = "SimpleHooks_Log_Add"
            };

            Business.InstanceManager manager = new(
                logger,
                new SqlConnectionRepo() { ConnectionString = connectionString },
                new EventInstanceDataRepo(),
                new ListenerInstanceDataRepo(),
                new SimpleClient(),
                new EventDefinitionDataRepo(),
                new ListenerDefinitionDataRepo(),
                new EventIistenerDefinitionDataRepo(),
                new ListenerTypeDataRepo(),
                new ListenerPluginManager(logger),
                new AppOptionDataRepo());

            var instances = manager.GetEventInstancesToProcess(DateTime.UtcNow);
            manager.Process(instances);
        }
    }
}
