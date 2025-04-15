using System;
using System.Collections.Generic;

namespace TestSimpleHooks.Repos
{
    internal class ListenerDefRepo : GenericRepo<Models.Definition.ListenerDefinition>
    {
        public ListenerDefRepo()
        {
            this.Entities.AddRange(new List<Models.Definition.ListenerDefinition>
            {
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Listener-1",
                    Notes = "testing",
                    TrialCount = 3,
                    RetrialDelay = 1,
                    Timeout = 2,
                    Url = "http://test1.test.test"

                },
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Listener-2",
                    Notes = "testing",
                    TrialCount = 3,
                    RetrialDelay = 1,
                    Timeout = 2,
                    Url = "http://test2.test.test"
                }
            });

            this.Entities.ForEach(e => e.Headers.Add("content-type=application/json"));
        }
    }
}
