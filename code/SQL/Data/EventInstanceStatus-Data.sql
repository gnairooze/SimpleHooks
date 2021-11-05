use SimpleHooks
go

insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (1, 'InQueue', 'system.admin', 'system.admin')
insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (2, 'Processing', 'system.admin', 'system.admin')
insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (4, 'Hold', 'system.admin', 'system.admin')
insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (8, 'Succeeded', 'system.admin', 'system.admin')
insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (16, 'Failed', 'system.admin', 'system.admin')
insert EventInstanceStatus(Id, [Name], CreateBy, ModifyBy) values (32, 'Aborted', 'system.admin', 'system.admin')


