use SimpleHooks
go

insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (1, 'Anonymous', 'listener-plugins/Anonymous/Anonymous.dll', 'system.admin', 'system.admin', '')
insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (2, 'TypeA', 'listener-plugins/TypeA/TypeAListener.dll', 'system.admin', 'system.admin', 'include Bearer token in header')
