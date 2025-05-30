using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface IDataRepositoryEventInstanceStatus
    {
        Models.Instance.EventInstanceStatusBrief ReadByBusinessId(Guid businessId, object connection);
    }
}
