using System;
using static SimpleTools.SimpleHooks.Models.Instance.Enums;

namespace SimpleTools.SimpleHooks.Web.Models
{
    public class EventStatusBriefViewModel
    {
        public long Id { get; set; }
        public Guid BusinessId { get; set; }
        public EventInstanceStatus Status { get; set; }
    }
}
