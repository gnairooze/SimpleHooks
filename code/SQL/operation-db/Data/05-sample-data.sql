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
	'sample api', 
	1, 
	'http://test.test.com/api',
	'content-type: application/json',
	60,
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
	'sample json to test event'
);
