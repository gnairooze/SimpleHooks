namespace SimpleTools.SimpleHooks.Repo.SQL
{
    internal static class Constants
    {
        public const string SpEventInstanceAdd = "EventInstance_Add";
        public const string SpEventInstanceRemove = "EventInstance_Remove";
        public const string SpEventInstanceEdit = "EventInstance_Edit";
        public const string SpEventInstanceGetNotProcessed = "EventInstance_GetNotProcessed";

        public const string SpListenerInstanceAdd = "ListenerInstance_Add";
        public const string SpListenerInstanceRemove = "ListenerInstance_Remove";
        public const string SpListenerInstanceEdit = "ListenerInstance_Edit";
        public const string SpListenerInstanceGetByEventInstanceId = "ListenerInstance_GetByEventInstance_Id";

        public const string SpEventAppOptionGetAll = "AppOption_GetAll";
        public const string SpEventDefinitionGetAll = "EventDefinition_GetAll";
        public const string SpListenerDefinitionGetAll = "ListenerDefinition_GetAll";
        public const string SpEventListenerDefinitionGetAll = "EventDefinition_ListenerDefinition_GetAll";
        public const string SpEventInstanceGetByBusinessId = "EventInstance_GetByBusinessId";
        public const string SpEventInstanceGetStatusByBusinessId = "EventInstance_GetStatusByBusinessId";

        public const string ParamId = "@Id";
        public const string ParamActive = "@Active";
        public const string ParamBusinessId = "@BusinessId";
        public const string ParamCreateBy = "@CreateBy";
        public const string ParamCreateDate = "@CreateDate";
        public const string ParamModifyBy = "@ModifyBy";
        public const string ParamModifyDate = "@ModifyDate";
        public const string ParamNotes = "@Notes";
        public const string ParamGroupId = "@GroupId";
        public const string ParamTimestamp = "@TimeStamp";

        public const string ParamDate = "@Date";

        public const string ParamEventDefinitionId = "@EventDefinition_Id";
        public const string ParamEventData = "@EventData";
        public const string ParamReferenceName = "@ReferenceName";
        public const string ParamReferenceValue = "@ReferenceValue";
        public const string ParamEventInstanceStatusId = "@EventInstanceStatus_Id";

        public const string ParamEventInstanceId = "@EventInstance_Id";
        public const string ParamListenerDefinitionId = "@ListenerDefinition_Id";
        public const string ParamListenerInstanceStatusId = "@ListenerInstanceStatus_Id";
        public const string ParamRemainingTrialCount = "@RemainingTrialCount";
        public const string ParamNextRun = "@NextRun";

        public const string FieldId = "Id";
        public const string FieldActive = "Active";
        public const string FieldBusinessId = "BusinessId";
        public const string FieldCreateBy = "CreateBy";
        public const string FieldCreateDate = "CreateDate";
        public const string FieldModifyBy = "ModifyBy";
        public const string FieldModifyDate = "ModifyDate";
        public const string FieldNotes = "Notes";
        public const string FieldTimestamp = "TimeStamp";
        public const string FieldGroupId = "GroupId";

        public const string FieldEventDefinitionId = "EventDefinition_Id";
        public const string FieldEventData = "EventData";
        public const string FieldReferenceName = "ReferenceName";
        public const string FieldReferenceValue = "ReferenceValue";
        public const string FieldEventInstanceStatusId = "EventInstanceStatus_Id";

        public const string FieldEventInstanceId = "EventInstance_Id";
        public const string FieldListenerDefinitionId = "ListenerDefinition_Id";
        public const string FieldListenerInstanceStatusId = "ListenerInstanceStatus_Id";
        public const string FieldRemainingTrialCount = "RemainingTrialCount";
        public const string FieldNextRun = "NextRun";

        public const string FieldName = "Name";
        public const string FieldUrl = "URL";
        public const string FieldHeaders = "Headers";
        public const string FieldTimeout = "Timeout";
        public const string FieldTrialCount = "TrialCount";
        public const string FieldRetrialDelay = "RetrialDelay";

        public const string FieldCategory = "Category";
        public const string FieldValue = "Value";

        
    }
}
