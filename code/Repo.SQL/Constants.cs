using System;
using System.Collections.Generic;
using System.Text;

namespace Repo.SQL
{
    internal class Constants
    {
        public const string SP_EVENT_INSTANCE_ADD = "EventInstance_Add";
        public const string SP_EVENT_INSTANCE_REMOVE = "EventInstance_Remove";
        public const string SP_EVENT_INSTANCE_EDIT = "EventInstance_Edit";
        public const string SP_EVENT_INSTANCE_GET_NOT_PROCESSED = "EventInstance_GetNotProcessed";

        public const string SP_LISTENER_INSTANCE_ADD = "ListenerInstance_Add";
        public const string SP_LISTENER_INSTANCE_REMOVE = "ListenerInstance_Remove";
        public const string SP_LISTENER_INSTANCE_EDIT = "ListenerInstance_Edit";
        public const string SP_LISTENER_INSTANCE_GET_BY_EVENT_INSTANCE_ID = "ListenerInstance_GetByEventInstance_Id";

        public const string SP_EVENT_APP_OPTION_GET_ALL = "AppOption_GetAll";
        public const string SP_EVENT_DEFINITION_GET_ALL = "EventDefinition_GetAll";
        public const string SP_LISTENER_DEFINITION_GET_ALL = "ListenerDefinition_GetAll";
        public const string SP_EVENT_LISTENER_DEFINITION_GET_ALL = "EventDefinition_ListenerDefinition_GetAll";
        
        public const string PARAM_ID = "@Id";
        public const string PARAM_ACTIVE = "@Active";
        public const string PARAM_BUSINESS_ID = "@BusinessId";
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

        public const string PARAM_EVENT_INSTANCE_ID = "@EventInstance_Id";
        public const string PARAM_LISTENER_DEFINITION_ID = "@ListenerDefinition_Id";
        public const string PARAM_LISTENER_INSTANCE_STATUS_ID = "@ListenerInstanceStatus_Id";
        public const string PARAM_REMAINING_TRIAL_COUNT = "@RemainingTrialCount";
        public const string PARAM_NEXT_RUN = "@NextRun";

        public const string FIELD_ID = "Id";
        public const string FIELD_ACTIVE = "Active";
        public const string FIELD_BUSINESS_ID = "BusinessId";
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

        public const string FIELD_NAME = "Name";
        public const string FIELD_URL = "URL";
        public const string FIELD_HEADERS = "Headers";
        public const string FIELD_TIMEOUT = "Timeout";
        public const string FIELD_TRIAL_COUNT = "TrialCount";
        public const string FIELD_RETRIAL_DELAY = "RetrialDelay";

        public const string FIELD_CATEGORY = "Category";
        public const string FIELD_VALUE = "Value";
    }
}
