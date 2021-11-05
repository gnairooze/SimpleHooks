using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks
{
    internal class TestActualOperation
    {
        public static void TestAddEventInstance1()
        {
            string connectionString = "";

            Business.InstanceManager manager = new Business.InstanceManager(
                new Log.Console.Logger() { MinLogType = Log.Interface.LogModel.LogTypes.Debug },
                new Repo.SQL.SqlConnectionRepo() { ConnectionString = connectionString },
                new Repo.SQL.EventInstanceDataRepo(),
                new Repo.SQL.ListenerInstanceDataRepo(),
                new HttpClient.Simple.SimpleClient(),
                new Repo.SQL.EventDefinitionDataRepo(),
                new Repo.SQL.ListenerDefinitionDataRepo(),
                new Repo.SQL.EventIistenerDefinitionDataRepo(),
                null);

            var instance = manager.Add(new Models.Instance.EventInstance() { 
                Active = true,
                CreateBy = "test.user",
                CreateDate = DateTime.UtcNow,
                EventData = "{ 'test' = 'value' }",
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
    }
}
