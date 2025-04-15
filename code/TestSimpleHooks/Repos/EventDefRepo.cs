using System;
using System.Collections.Generic;

namespace TestSimpleHooks.Repos
{
    internal class EventDefRepo : GenericRepo<Models.Definition.EventDefinition>
    {
        public EventDefRepo()
        {
            this.Entities.AddRange(new List<Models.Definition.EventDefinition>
            {
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Event-1",
                    Notes = "testing"
                },
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Event-2",
                    Notes = "testing"
                }
            });
        }
    }
}
