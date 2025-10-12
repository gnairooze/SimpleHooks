use SimpleHooks
go

insert ListenerDefinitionAuthType(Id, [Name], CreateBy, ModifyBy, Notes) values (1, 'Anonymous', 'system.admin', 'system.admin', '')
insert ListenerDefinitionAuthType(Id, [Name], CreateBy, ModifyBy, Notes) values (2, 'TypeA', 'system.admin', 'system.admin', 'include Bearer token in header')
