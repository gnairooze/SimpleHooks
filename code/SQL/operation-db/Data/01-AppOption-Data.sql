use SimpleHooks
go

insert AppOption (Category, [Name], [Value], CreateBy, ModifyBy) values ('GetNotProcessed', 'TopCount', '100', 'system.admin', 'system.admin')
insert AppOption (Category, [Name], [Value], CreateBy, ModifyBy) values ('GetNotProcessed', 'MaxGroups', '1', 'system.admin', 'system.admin')
