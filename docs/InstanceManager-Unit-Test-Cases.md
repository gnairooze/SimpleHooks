# InstanceManager Unit Test Cases

## Overview
This document provides comprehensive unit test cases for all public methods in the `InstanceManager` class from the SimpleHooks project. The `InstanceManager` is responsible for managing event instances and their associated listener instances, handling the complete lifecycle from creation to processing.

## Class Dependencies
The `InstanceManager` class depends on the following interfaces and services:
- `ILog` - Logging service
- `IConnectionRepository` - Database connection management
- `IDataRepository<EventInstance>` - Event instance data operations
- `IDataRepository<ListenerInstance>` - Listener instance data operations
- `IHttpClient` - HTTP client for webhook calls
- `DefinitionManager` - Manages event and listener definitions

## Public Methods

### 1. Constructor

#### Method Signature
```csharp
public InstanceManager(
    ILog logger, 
    IConnectionRepository connectionRepo, 
    IDataRepository<Models.Instance.EventInstance> eventInstanceRepo, 
    IDataRepository<Models.Instance.ListenerInstance> listenerInstanceRepo, 
    IHttpClient httpClient, 
    IDataRepository<Models.Definition.EventDefinition> eventDefRepo, 
    IDataRepository<Models.Definition.ListenerDefinition> listenerDefRepo, 
    IDataRepository<Models.Definition.EventDefinitionListenerDefinition> eventDefListenerDefRepo, 
    IDataRepository<Models.Definition.AppOption> appOptionRepo
)
```

#### Test Cases

##### Test Case 1.1: Constructor_ValidParameters_Success
**Description**: Verify constructor initializes successfully with all valid parameters
**Setup**:
- Mock all required dependencies
- Setup DefinitionManager.LoadDefinitions() to return true
**Expected Result**: InstanceManager instance created successfully

##### Test Case 1.2: Constructor_NullLogger_ThrowsArgumentNullException
**Description**: Verify constructor throws ArgumentNullException when logger is null
**Setup**: Pass null for logger parameter, valid mocks for others
**Expected Result**: ArgumentNullException with parameter name "logger"

##### Test Case 1.3: Constructor_NullConnectionRepo_ThrowsArgumentNullException
**Description**: Verify constructor throws ArgumentNullException when connectionRepo is null
**Setup**: Pass null for connectionRepo parameter, valid mocks for others
**Expected Result**: ArgumentNullException with parameter name "connectionRepo"

##### Test Case 1.4: Constructor_NullEventInstanceRepo_ThrowsArgumentNullException
**Description**: Verify constructor throws ArgumentNullException when eventInstanceRepo is null
**Setup**: Pass null for eventInstanceRepo parameter, valid mocks for others
**Expected Result**: ArgumentNullException with parameter name "eventInstanceRepo"

##### Test Case 1.5: Constructor_NullListenerInstanceRepo_ThrowsArgumentNullException
**Description**: Verify constructor throws ArgumentNullException when listenerInstanceRepo is null
**Setup**: Pass null for listenerInstanceRepo parameter, valid mocks for others
**Expected Result**: ArgumentNullException with parameter name "listenerInstanceRepo"

##### Test Case 1.6: Constructor_NullHttpClient_ThrowsArgumentNullException
**Description**: Verify constructor throws ArgumentNullException when httpClient is null
**Setup**: Pass null for httpClient parameter, valid mocks for others
**Expected Result**: ArgumentNullException with parameter name "httpClient"

##### Test Case 1.7: Constructor_DefinitionsFailToLoad_ThrowsInvalidOperationException
**Description**: Verify constructor throws InvalidOperationException when definitions fail to load
**Setup**: 
- Mock all dependencies
- Setup DefinitionManager.LoadDefinitions() to return false
**Expected Result**: InvalidOperationException with message "definitions failed to load"

---

### 2. Add Method

#### Method Signature
```csharp
public Models.Instance.EventInstance Add(Models.Instance.EventInstance eventInstance)
```

#### Test Cases

##### Test Case 2.1: Add_ValidEventInstance_Success
**Description**: Verify successful addition of event instance with listeners
**Setup**:
- Create valid EventInstance with EventDefinitionId
- Mock connection operations to succeed
- Mock event and listener repository Create operations
- Setup EventDefinitionListenerDefinition relations
**Expected Result**: 
- Returns EventInstance with assigned Id and GroupId
- Event instance created in repository
- Listener instances created and linked
- Transaction committed
- Logging calls made

##### Test Case 2.2: Add_EventInstanceWithNoListeners_Success
**Description**: Verify successful addition of event instance when no listeners are configured
**Setup**:
- Create valid EventInstance
- Mock connection operations to succeed
- Setup empty EventDefinitionListenerDefinition relations
**Expected Result**: 
- Returns EventInstance with assigned Id and GroupId
- Event instance created in repository
- No listener instances created
- Transaction committed

##### Test Case 2.3: Add_DatabaseException_ReturnsNull
**Description**: Verify proper handling when database operation fails
**Setup**:
- Create valid EventInstance
- Mock connection operations to succeed initially
- Mock eventInstanceRepo.Create() to throw exception
**Expected Result**: 
- Returns null
- Transaction rolled back
- Connection closed and disposed
- Exception logged

##### Test Case 2.4: Add_ConnectionException_ReturnsNull
**Description**: Verify proper handling when connection fails
**Setup**:
- Create valid EventInstance
- Mock OpenConnection to throw exception
**Expected Result**: 
- Returns null
- Transaction rolled back
- Connection closed and disposed
- Exception logged

##### Test Case 2.5: Add_GroupIdAssignment_CyclesThroughMaxGroups
**Description**: Verify GroupId assignment cycles correctly through max groups
**Setup**:
- Configure maxGroups to 3 via AppOptions
- Call Add method multiple times
**Expected Result**: GroupId values cycle: 1, 2, 3, 1, 2, 3...

---

### 3. GetEventInstancesToProcess Method

#### Method Signature
```csharp
public List<Models.Instance.EventInstance> GetEventInstancesToProcess(DateTime runDate, int groupId = 1)
```

#### Test Cases

##### Test Case 3.1: GetEventInstancesToProcess_ValidParameters_ReturnsEvents
**Description**: Verify method returns event instances ready for processing
**Setup**:
- Mock connection operations to succeed
- Setup eventInstanceRepo.Read() to return list of EventInstances
- Setup DefinitionManager with EventDefinitions and ListenerDefinitions
**Expected Result**: 
- Returns list of EventInstance objects
- Each EventInstance has enriched Definition property
- Each ListenerInstance has enriched Definition property
- Logging calls made with event count

##### Test Case 3.2: GetEventInstancesToProcess_NoEventsFound_ReturnsEmptyList
**Description**: Verify method handles case when no events are found
**Setup**:
- Mock connection operations to succeed
- Setup eventInstanceRepo.Read() to return empty list
**Expected Result**: 
- Returns empty list
- Logging shows 0 events to be processed
- Connection properly closed and disposed

##### Test Case 3.3: GetEventInstancesToProcess_DatabaseException_ReturnsNull
**Description**: Verify proper exception handling during database read
**Setup**:
- Mock connection operations to succeed initially
- Mock eventInstanceRepo.Read() to throw exception
**Expected Result**: 
- Returns null
- Exception logged
- Connection properly closed and disposed

##### Test Case 3.4: GetEventInstancesToProcess_ConnectionException_ReturnsNull
**Description**: Verify proper exception handling during connection
**Setup**:
- Mock CreateConnection to succeed
- Mock OpenConnection to throw exception
**Expected Result**: 
- Returns null
- Exception logged
- Connection properly closed and disposed

##### Test Case 3.5: GetEventInstancesToProcess_CustomGroupId_UsesCorrectGroupId
**Description**: Verify method uses provided groupId parameter
**Setup**:
- Call method with groupId = 5
- Verify read operation includes correct groupId
**Expected Result**: Read operation contains groupId parameter with value 5

---

### 4. Process Method (List overload)

#### Method Signature
```csharp
public void Process(List<Models.Instance.EventInstance> eventInstances)
```

#### Test Cases

##### Test Case 4.1: Process_ValidEventInstancesList_ProcessesAllEvents
**Description**: Verify method processes all event instances in the list
**Setup**:
- Create list of EventInstance objects with ListenerInstances
- Mock all repository and HTTP operations to succeed
**Expected Result**: 
- All event instances processed
- Status updated to Processing then final status
- HTTP calls made for all listeners
- Logging calls made for start and end

##### Test Case 4.2: Process_EmptyList_CompletesSuccessfully
**Description**: Verify method handles empty list gracefully
**Setup**: Pass empty list of EventInstance objects
**Expected Result**: 
- Method completes without errors
- Start and end logging calls made
- No processing operations performed

##### Test Case 4.3: Process_ExceptionInSingleEvent_ContinuesProcessingOthers
**Description**: Verify method continues processing other events when one fails
**Setup**:
- Create list with multiple EventInstance objects
- Mock one event to throw exception during processing
**Expected Result**: 
- Exception logged for failing event
- Other events continue to be processed
- Method completes successfully

##### Test Case 4.4: Process_NullList_HandlesGracefully
**Description**: Verify method handles null input gracefully
**Setup**: Pass null for eventInstances parameter
**Expected Result**: Method completes without throwing exception

---

### 5. ReadEventInstanceStatusByBusinessId Method

#### Method Signature
```csharp
public EventInstanceStatusBrief ReadEventInstanceStatusByBusinessId(Guid businessId)
```

#### Test Cases

##### Test Case 5.1: ReadEventInstanceStatusByBusinessId_ValidBusinessId_ReturnsStatus
**Description**: Verify method returns status for valid business ID
**Setup**:
- Mock connection operations to succeed
- Setup eventInstanceRepo cast to IDataRepositoryEventInstanceStatus
- Mock ReadByBusinessId to return EventInstanceStatusBrief
**Expected Result**: 
- Returns EventInstanceStatusBrief with correct data
- Connection opened, closed, and disposed
- Logging calls made

##### Test Case 5.2: ReadEventInstanceStatusByBusinessId_BusinessIdNotFound_ReturnsEmptyBrief
**Description**: Verify method handles case when business ID is not found
**Setup**:
- Mock connection operations to succeed
- Mock ReadByBusinessId to return empty EventInstanceStatusBrief
**Expected Result**: 
- Returns empty EventInstanceStatusBrief
- Connection properly managed
- Logging calls made

##### Test Case 5.3: ReadEventInstanceStatusByBusinessId_DatabaseException_ReturnsEmptyBrief
**Description**: Verify proper exception handling during database read
**Setup**:
- Mock connection operations to succeed initially
- Mock ReadByBusinessId to throw exception
**Expected Result**: 
- Returns empty EventInstanceStatusBrief
- Exception logged
- Connection properly closed and disposed

##### Test Case 5.4: ReadEventInstanceStatusByBusinessId_ConnectionException_ReturnsEmptyBrief
**Description**: Verify proper exception handling during connection
**Setup**:
- Mock CreateConnection to succeed
- Mock OpenConnection to throw exception
**Expected Result**: 
- Returns empty EventInstanceStatusBrief
- Exception logged
- Connection properly closed and disposed

##### Test Case 5.5: ReadEventInstanceStatusByBusinessId_EmptyGuid_HandlesGracefully
**Description**: Verify method handles empty GUID parameter
**Setup**: Pass Guid.Empty as businessId parameter
**Expected Result**: Method executes without throwing exception

---

### 6. DefinitionMgr Property

#### Property Signature
```csharp
public DefinitionManager DefinitionMgr => _definitionManager;
```

#### Test Cases

##### Test Case 6.1: DefinitionMgr_Get_ReturnsDefinitionManager
**Description**: Verify property returns the internal DefinitionManager instance
**Setup**: Create InstanceManager with valid dependencies
**Expected Result**: Property returns non-null DefinitionManager instance

##### Test Case 6.2: DefinitionMgr_MultipleAccess_ReturnsSameInstance
**Description**: Verify property consistently returns the same instance
**Setup**: Create InstanceManager and access property multiple times
**Expected Result**: All accesses return the same DefinitionManager instance

---

## Test Setup Helpers

### Common Mock Setup
```csharp
private Mock<ILog> CreateMockLogger()
private Mock<IConnectionRepository> CreateMockConnectionRepository()
private Mock<IDataRepository<EventInstance>> CreateMockEventInstanceRepository()
private Mock<IDataRepository<ListenerInstance>> CreateMockListenerInstanceRepository()
private Mock<IHttpClient> CreateMockHttpClient()
private Mock<IDataRepository<EventDefinition>> CreateMockEventDefinitionRepository()
private Mock<IDataRepository<ListenerDefinition>> CreateMockListenerDefinitionRepository()
private Mock<IDataRepository<EventDefinitionListenerDefinition>> CreateMockEventDefListenerDefRepository()
private Mock<IDataRepository<AppOption>> CreateMockAppOptionRepository()
```

### Test Data Builders
```csharp
private EventInstance CreateValidEventInstance()
private ListenerInstance CreateValidListenerInstance()
private EventDefinition CreateValidEventDefinition()
private ListenerDefinition CreateValidListenerDefinition()
private EventDefinitionListenerDefinition CreateValidEventDefListenerDef()
private AppOption CreateMaxGroupsAppOption(int maxGroups)
```

## Integration Considerations

### Database Transaction Testing
- Verify transaction commit on success
- Verify transaction rollback on failure
- Test transaction scope across multiple operations

### HTTP Client Integration
- Mock various HTTP response codes
- Test timeout scenarios
- Verify request headers and payload

### Logging Integration
- Verify correlation IDs are consistent
- Test different log levels (Debug, Information, Error)
- Verify log content includes relevant context

### Concurrency Testing
- Test GroupId assignment under concurrent access
- Verify thread safety of static _groupId field
- Test multiple simultaneous processing operations

## Performance Considerations

### Large Dataset Testing
- Test with large lists of EventInstances
- Verify memory usage with many ListenerInstances
- Test database connection pooling behavior

### Error Recovery Testing
- Test recovery from temporary database failures
- Verify behavior during HTTP client timeouts
- Test handling of malformed JSON in EventData

## Notes

1. All tests should use proper mocking frameworks (e.g., Moq) to isolate units under test
2. Tests should verify both positive and negative scenarios
3. Exception handling should be thoroughly tested
4. Resource disposal (connections, transactions) should be verified
5. Logging behavior should be validated in all test cases
6. Consider using test data builders for complex object creation
7. Integration tests may be needed for end-to-end scenarios
8. Performance tests should be considered for high-volume scenarios
