using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Instance
{
    public class Enums
    {
        public enum EventInstanceStatus
        {
            NotSet = 0,
            InQueue = 1,
            Processing = 2,
            Hold = 4,
            Succeeded = 8, //end state
            Failed = 16, //end state
            Aborted = 32 //end state
        }

        public enum ListenerInstanceStatus
        {
            NotSet = 0,
            InQueue = 1,
            Processing = 2,
            Hold = 4,
            Succeeded = 8, //end state
            Failed = 16, //end state
            Aborted = 32, //end state
            WaitingForRetrial = 64
        }

        public enum EventInstanceReadOperations
        {
            NotSet = 0,
            ReadNotProcessed = 1
        }

        public enum ListenerInstanceReadOperations
        {
            NotSet = 0,
            ReadByEventInstanceId = 1
        }

        public static bool IsEndState(EventInstanceStatus status)
        {
            switch (status)
            {
                case EventInstanceStatus.Succeeded:
                case EventInstanceStatus.Failed:
                case EventInstanceStatus.Aborted:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEndState(ListenerInstanceStatus status)
        {
            switch (status)
            {
                case ListenerInstanceStatus.Succeeded:
                case ListenerInstanceStatus.Failed:
                case ListenerInstanceStatus.Aborted:
                    return true;
                default:
                    return false;
            }
        }
    }
}
