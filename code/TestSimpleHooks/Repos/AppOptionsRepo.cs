using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSimpleHooks.Repos
{
    internal class AppOptionsRepo : GenericRepo<Models.Definition.AppOption>
    {
        public AppOptionsRepo()
        {
            this.Entities.AddRange(new List<Models.Definition.AppOption> {
                new (){ 
                    Active = true,
                    Category = "test",
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Key-1",
                    Notes = "testing",
                    Value = "Test-Value-1"
                },
                new (){
                    Active = true,
                    Category = "test",
                    CreateBy = "test-system-user",
                    CreateDate = DateTime.UtcNow,
                    Id = 1,
                    ModifyBy = "test-system-user",
                    ModifyDate = DateTime.UtcNow,
                    Name = "Test-Key-2",
                    Notes = "testing",
                    Value = "Test-Value-2"
                }
            });
        }
    }
}
