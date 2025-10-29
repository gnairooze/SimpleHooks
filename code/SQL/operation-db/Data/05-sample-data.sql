USE SimpleHooks;

set identity_insert EventDefinition on;

insert EventDefinition
(
	Id,
	Name,
	Active,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	1,
	'Test Event',
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'test event to test the system'
);

set identity_insert EventDefinition off;

set identity_insert ListenerDefinition on;

insert ListenerDefinition
(
	Id,
	Name,
	Type_Id,
	Type_Options,
	Active,
	URL,
	Headers,
	Timeout,
	TrialCount,
	RetrialDelay,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	1,
	'sample listener',
	1,
	'',
	1,
	'http://host.docker.internal:5011/api/sample',
	'content-type: application/json',
	600,
	3,
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'save event data'
);

insert ListenerDefinition
(
	Id,
	Name,
	Type_Id,
	Type_Options,
	Active,
	URL,
	Headers,
	Timeout,
	TrialCount,
	RetrialDelay,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	2,
	'sample auth listener',
	2,
	'SIMPLE_HOOKS_LISTENER_PLUGIN_SAMPLE_OPTIONS',
	1,
	'http://host.docker.internal:5011/api/SampleAuth',
	'content-type: application/json',
	600,
	3,
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'save event data'
);

set identity_insert ListenerDefinition off;

insert EventDefinition_ListenerDefinition
(
	EventDefinition_Id,
	ListenerDefinition_Id,
	Active,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	1,
	1,
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'sample listener api to test event'
);

insert EventDefinition_ListenerDefinition
(
	EventDefinition_Id,
	ListenerDefinition_Id,
	Active,
	CreateBy,
	CreateDate,
	ModifyBy,
	ModifyDate,
	Notes
)
values
(
	1,
	2,
	1,
	'admin',
	'2024-02-20',
	'admin',
	'2024-02-20',
	'sample listener api to test event'
);
