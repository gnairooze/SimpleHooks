using System;
using System.Collections.Generic;

namespace TestSimpleHooks.Repos
{
    internal class EventDefListenerDefRepo: GenericRepo<Models.Definition.EventDefinitionListenerDefinition>
    {
        public EventDefListenerDefRepo()
        {
            this.Entities.AddRange(new List<Models.Definition.EventDefinitionListenerDefinition>
            {
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Notes = "testing",
                    EventDefinitionId = 1,
                    ListenerDefinitionId = 1
                },
                new (){
                    Active = true,
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Notes = "testing",
                    EventDefinitionId = 2,
                    ListenerDefinitionId = 2
                }
            });
        }
    }
}
