using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    internal class InstanceConstants
    {
        #region Errors
        public const string ERROR_LISTENER_STATUS_NOT_SET = "Listener status not set";
        #endregion

        #region users
        public const string USER_SYSTEM_PROCESSOR = "system.processor";
        public const string USER_SYSTEM_EVENT_MANAGER = "system.event-manager";
        #endregion

        #region http codes
        public const int HTTP_CODE_SUCCEEDED = 200;
        #endregion
    }
}
