use SimpleHooks
go

insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (1, 'Anonymous', 'listener-plugins/anonymous/SimpleHooks.ListenerPlugins.Anonymous.dll', 'system.admin', 'system.admin', '')
insert ListenerType(Id, [Name], [Path], CreateBy, ModifyBy, Notes) values (2, 'TypeA', 'listener-plugins/typea/SimpleHooks.ListenerPlugins.TypeA.dll', 'system.admin', 'system.admin', 'include Bearer token in header')
