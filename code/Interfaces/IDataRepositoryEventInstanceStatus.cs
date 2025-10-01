using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTools.SimpleHooks.Interfaces
{
    public interface IDataRepositoryEventInstanceStatus
    {
        Models.Instance.EventInstanceStatusBrief ReadByBusinessId(Guid businessId, object connection);
    }
}
