using System;
using System.Collections.Generic;
using System.Text;

namespace Repo.SQL
{
    internal class Constants
    {
        public const string SP_EVENTINSTANCE_ADD = "EventInstance_Add";
        public const string SP_EVENTINSTANCE_REMOVE = "EventInstance_Remove";
        public const string SP_EVENTINSTANCE_EDIT = "EventInstance_Edit";
        public const string SP_EVENTINSTANCE_GET_NOT_PROCESSED = "EventInstance_GetNotProcessed";

        public const string PARAM_ID = "@Id";
        public const string PARAM_ACTIVE = "@Active";
        public const string PARAM_CREATE_BY = "@CreateBy";
        public const string PARAM_CREATE_DATE = "@CreateDate";
        public const string PARAM_MODIFY_BY = "@ModifyBy";
        public const string PARAM_MODIFY_DATE = "@ModifyDate";
        public const string PARAM_NOTES = "@Notes";
        public const string PARAM_TIMESTAMP = "@TimeStamp";

        public const string PARAM_DATE = "@Date";

        public const string PARAM_EVENT_DEFINITION_ID = "@EventDefinition_Id";
        public const string PARAM_EVENT_DATA = "@EventData";
        public const string PARAM_REFERENCE_NAME = "@ReferenceName";
        public const string PARAM_REFERENCE_VALUE = "@ReferenceValue";
        public const string PARAM_EVENT_INSTANCE_STATUS_ID = "@EventInstanceStatus_Id";

        public const string FIELD_ID = "Id";
        public const string FIELD_ACTIVE = "Active";
        public const string FIELD_CREATE_BY = "CreateBy";
        public const string FIELD_CREATE_DATE = "CreateDate";
        public const string FIELD_MODIFY_BY = "ModifyBy";
        public const string FIELD_MODIFY_DATE = "ModifyDate";
        public const string FIELD_NOTES = "Notes";
        public const string FIELD_TIMESTAMP = "TimeStamp";

        public const string FIELD_EVENT_DEFINITION_ID = "EventDefinition_Id";
        public const string FIELD_EVENT_DATA = "EventData";
        public const string FIELD_REFERENCE_NAME = "ReferenceName";
        public const string FIELD_REFERENCE_VALUE = "ReferenceValue";
        public const string FIELD_EVENT_INSTANCE_STATUS_ID = "EventInstanceStatus_Id";

        public const string FIELD_EVENT_INSTANCE_ID = "EventInstance_Id";
        public const string FIELD_LISTENER_DEFINITION_ID = "ListenerDefinition_Id";
        public const string FIELD_LISTENER_INSTANCE_STATUS_ID = "ListenerInstanceStatus_Id";
        public const string FIELD_REMAINING_TRIAL_COUNT = "RemainingTrialCount";
        public const string FIELD_NEXT_RUN = "NextRun";
    }
}
