using System;
using static Models.Instance.Enums;

namespace SimpleHooks.Web.Models
{
    public class EventStatusBriefViewModel
    {
        public long Id { get; set; }
        public Guid BusinessId { get; set; }
        public EventInstanceStatus Status { get; set; }
    }
}
