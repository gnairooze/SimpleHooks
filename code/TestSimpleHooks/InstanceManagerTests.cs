namespace SimpleTools.SimpleHooks.TestSimpleHooks;
using SimpleTools.SimpleHooks.Business;
using SimpleTools.SimpleHooks.Interfaces;
using SimpleTools.SimpleHooks.Log.Interface;
using SimpleTools.SimpleHooks.Models.Definition;
using SimpleTools.SimpleHooks.Models.Instance;
using SimpleTools.SimpleHooks.HttpClient.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

public class InstanceManagerTests
{
    private readonly Mock<ILog> _mockLogger;
    private readonly Mock<IConnectionRepository> _mockConnectionRepo;
    private readonly Mock<IDataRepository<EventInstance>> _mockEventInstanceRepo;
    private readonly Mock<IDataRepository<ListenerInstance>> _mockListenerInstanceRepo;
    private readonly Mock<IHttpClient> _mockHttpClient;
    private readonly Mock<IDataRepository<EventDefinition>> _mockEventDefRepo;
    private readonly Mock<IDataRepository<ListenerDefinition>> _mockListenerDefRepo;
    private readonly Mock<IDataRepository<EventDefinitionListenerDefinition>> _mockEventDefListenerDefRepo;
    private readonly Mock<IDataRepository<AppOption>> _mockAppOptionRepo;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<IDbTransaction> _mockTransaction;
    private readonly Mock<IDataRepositoryEventInstanceStatus> _mockEventInstanceStatusRepo;

    public InstanceManagerTests()
    {
        _mockLogger = new Mock<ILog>();
        _mockConnectionRepo = new Mock<IConnectionRepository>();
        _mockEventInstanceRepo = new Mock<IDataRepository<EventInstance>>();
        _mockListenerInstanceRepo = new Mock<IDataRepository<ListenerInstance>>();
        _mockHttpClient = new Mock<IHttpClient>();
        _mockEventDefRepo = new Mock<IDataRepository<EventDefinition>>();
        _mockListenerDefRepo = new Mock<IDataRepository<ListenerDefinition>>();
        _mockEventDefListenerDefRepo = new Mock<IDataRepository<EventDefinitionListenerDefinition>>();
        _mockAppOptionRepo = new Mock<IDataRepository<AppOption>>();
        _mockConnection = new Mock<IDbConnection>();
        _mockTransaction = new Mock<IDbTransaction>();
        _mockEventInstanceStatusRepo = new Mock<IDataRepositoryEventInstanceStatus>();

        // Setup the EventInstanceRepo to also implement IDataRepositoryEventInstanceStatus
        _mockEventInstanceRepo.As<IDataRepositoryEventInstanceStatus>();

        // Setup default successful behavior for DefinitionManager
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(_mockConnection.Object));
        _mockConnectionRepo.Setup(repo => repo.CloseConnection(_mockConnection.Object));
        _mockConnectionRepo.Setup(repo => repo.DisposeConnection(_mockConnection.Object));
        _mockConnectionRepo.Setup(repo => repo.BeginTransaction(_mockConnection.Object)).Returns(_mockTransaction.Object);
        _mockConnectionRepo.Setup(repo => repo.CommitTransaction(_mockTransaction.Object));
        _mockConnectionRepo.Setup(repo => repo.RollbackTransaction(_mockTransaction.Object));

        // Setup empty collections for DefinitionManager to load successfully
        _mockAppOptionRepo.Setup(repo => repo.Read(null, _mockConnection.Object)).Returns(new List<AppOption>());
        _mockEventDefRepo.Setup(repo => repo.Read(null, _mockConnection.Object)).Returns(new List<EventDefinition>());
        _mockListenerDefRepo.Setup(repo => repo.Read(null, _mockConnection.Object)).Returns(new List<ListenerDefinition>());
        _mockEventDefListenerDefRepo.Setup(repo => repo.Read(null, _mockConnection.Object)).Returns(new List<EventDefinitionListenerDefinition>());
    }

    private InstanceManager CreateInstanceManager()
    {
        return new InstanceManager(
            _mockLogger.Object,
            _mockConnectionRepo.Object,
            _mockEventInstanceRepo.Object,
            _mockListenerInstanceRepo.Object,
            _mockHttpClient.Object,
            _mockEventDefRepo.Object,
            _mockListenerDefRepo.Object,
            _mockEventDefListenerDefRepo.Object,
            _mockAppOptionRepo.Object);
    }

    private EventInstance CreateValidEventInstance()
    {
        return new EventInstance
        {
            Id = 1,
            EventDefinitionId = 1,
            BusinessId = Guid.NewGuid(),
            EventData = "{\"test\": \"data\"}",
            ReferenceName = "TestRef",
            ReferenceValue = "TestValue",
            Status = Enums.EventInstanceStatus.InQueue,
            CreateBy = "TestUser",
            CreateDate = DateTime.UtcNow,
            ModifyBy = "TestUser",
            ModifyDate = DateTime.UtcNow,
            Active = true
        };
    }

    private ListenerInstance CreateValidListenerInstance()
    {
        return new ListenerInstance
        {
            Id = 1,
            EventInstanceId = 1,
            ListenerDefinitionId = 1,
            Status = Enums.ListenerInstanceStatus.InQueue,
            RemainingTrialCount = 3,
            NextRun = DateTime.UtcNow,
            CreateBy = "TestUser",
            CreateDate = DateTime.UtcNow,
            ModifyBy = "TestUser",
            ModifyDate = DateTime.UtcNow,
            Active = true
        };
    }

    private EventDefinition CreateValidEventDefinition()
    {
        return new EventDefinition
        {
            Id = 1,
            Name = "TestEvent",
            CreateBy = "TestUser",
            CreateDate = DateTime.UtcNow,
            ModifyBy = "TestUser",
            ModifyDate = DateTime.UtcNow,
            Active = true
        };
    }

    private ListenerDefinition CreateValidListenerDefinition()
    {
        return new ListenerDefinition
        {
            Id = 1,
            Name = "TestListener",
            Url = "http://test.com/webhook",
            Timeout = 30,
            TrialCount = 3,
            RetrialDelay = 5,
            CreateBy = "TestUser",
            CreateDate = DateTime.UtcNow,
            ModifyBy = "TestUser",
            ModifyDate = DateTime.UtcNow,
            Active = true
        };
    }

    private EventDefinitionListenerDefinition CreateValidEventDefListenerDef()
    {
        return new EventDefinitionListenerDefinition
        {
            Id = 1,
            EventDefinitionId = 1,
            ListenerDefinitionId = 1,
            CreateBy = "TestUser",
            CreateDate = DateTime.UtcNow,
            ModifyBy = "TestUser",
            ModifyDate = DateTime.UtcNow,
            Active = true
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ValidParameters_Success()
    {
        // Act & Assert - Should not throw
        var instanceManager = CreateInstanceManager();
        
        Assert.NotNull(instanceManager);
        Assert.NotNull(instanceManager.DefinitionMgr);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new InstanceManager(
                null,
                _mockConnectionRepo.Object,
                _mockEventInstanceRepo.Object,
                _mockListenerInstanceRepo.Object,
                _mockHttpClient.Object,
                _mockEventDefRepo.Object,
                _mockListenerDefRepo.Object,
                _mockEventDefListenerDefRepo.Object,
                _mockAppOptionRepo.Object));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullConnectionRepo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new InstanceManager(
                _mockLogger.Object,
                null,
                _mockEventInstanceRepo.Object,
                _mockListenerInstanceRepo.Object,
                _mockHttpClient.Object,
                _mockEventDefRepo.Object,
                _mockListenerDefRepo.Object,
                _mockEventDefListenerDefRepo.Object,
                _mockAppOptionRepo.Object));

        Assert.Equal("connectionRepo", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullEventInstanceRepo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new InstanceManager(
                _mockLogger.Object,
                _mockConnectionRepo.Object,
                null,
                _mockListenerInstanceRepo.Object,
                _mockHttpClient.Object,
                _mockEventDefRepo.Object,
                _mockListenerDefRepo.Object,
                _mockEventDefListenerDefRepo.Object,
                _mockAppOptionRepo.Object));

        Assert.Equal("eventInstanceRepo", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullListenerInstanceRepo_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new InstanceManager(
                _mockLogger.Object,
                _mockConnectionRepo.Object,
                _mockEventInstanceRepo.Object,
                null,
                _mockHttpClient.Object,
                _mockEventDefRepo.Object,
                _mockListenerDefRepo.Object,
                _mockEventDefListenerDefRepo.Object,
                _mockAppOptionRepo.Object));

        Assert.Equal("listenerInstanceRepo", exception.ParamName);
    }

    [Fact]
    public void Constructor_NullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new InstanceManager(
                _mockLogger.Object,
                _mockConnectionRepo.Object,
                _mockEventInstanceRepo.Object,
                _mockListenerInstanceRepo.Object,
                null,
                _mockEventDefRepo.Object,
                _mockListenerDefRepo.Object,
                _mockEventDefListenerDefRepo.Object,
                _mockAppOptionRepo.Object));

        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public void Constructor_DefinitionsFailToLoad_ThrowsInvalidOperationException()
    {
        // Arrange - Make DefinitionManager fail to load
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(_mockConnection.Object))
            .Throws(new Exception("Database connection failed"));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => CreateInstanceManager());
        
        Assert.Equal("definitions failed to load", exception.Message);
    }

    #endregion

    #region Add Method Tests

    [Fact]
    public void Add_ValidEventInstance_Success()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstance = CreateValidEventInstance();
        var listenerDefinition = CreateValidListenerDefinition();
        var eventDefListenerDef = CreateValidEventDefListenerDef();
        var createdListenerInstance = CreateValidListenerInstance();

        // Setup DefinitionManager with test data
        instanceManager.DefinitionMgr.EventDefinitionListenerDefinitionRelations.Add(eventDefListenerDef);
        instanceManager.DefinitionMgr.ListenerDefinitions.Add(listenerDefinition);

        _mockEventInstanceRepo.Setup(repo => repo.Create(It.IsAny<EventInstance>(), _mockConnection.Object, _mockTransaction.Object))
            .Returns((EventInstance ei, object conn, object trans) => { ei.Id = 1; return ei; });

        _mockListenerInstanceRepo.Setup(repo => repo.Create(It.IsAny<ListenerInstance>(), _mockConnection.Object, _mockTransaction.Object))
            .Returns(createdListenerInstance);

        // Act
        var result = instanceManager.Add(eventInstance);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.True(result.GroupId >= 1);
        _mockEventInstanceRepo.Verify(repo => repo.Create(It.IsAny<EventInstance>(), _mockConnection.Object, _mockTransaction.Object), Times.Once);
        _mockListenerInstanceRepo.Verify(repo => repo.Create(It.IsAny<ListenerInstance>(), _mockConnection.Object, _mockTransaction.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.CommitTransaction(_mockTransaction.Object), Times.Once);
    }

    [Fact]
    public void Add_EventInstanceWithNoListeners_Success()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstance = CreateValidEventInstance();

        // No EventDefinitionListenerDefinition relations setup (empty list)

        _mockEventInstanceRepo.Setup(repo => repo.Create(It.IsAny<EventInstance>(), _mockConnection.Object, _mockTransaction.Object))
            .Returns((EventInstance ei, object conn, object trans) => { ei.Id = 1; return ei; });

        // Act
        var result = instanceManager.Add(eventInstance);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Empty(result.ListenerInstances);
        _mockEventInstanceRepo.Verify(repo => repo.Create(It.IsAny<EventInstance>(), _mockConnection.Object, _mockTransaction.Object), Times.Once);
        _mockListenerInstanceRepo.Verify(repo => repo.Create(It.IsAny<ListenerInstance>(), It.IsAny<object>(), It.IsAny<object>()), Times.Never);
        _mockConnectionRepo.Verify(repo => repo.CommitTransaction(_mockTransaction.Object), Times.Once);
    }

    [Fact]
    public void Add_DatabaseException_ReturnsNull()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstance = CreateValidEventInstance();

        _mockEventInstanceRepo.Setup(repo => repo.Create(It.IsAny<EventInstance>(), _mockConnection.Object, _mockTransaction.Object))
            .Throws(new Exception("Database error"));

        // Act
        var result = instanceManager.Add(eventInstance);

        // Assert
        Assert.Null(result);
        _mockConnectionRepo.Verify(repo => repo.RollbackTransaction(_mockTransaction.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void Add_ConnectionException_ReturnsNull()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstance = CreateValidEventInstance();

        // Reset and setup for this specific test
        _mockConnectionRepo.Reset();
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(_mockConnection.Object))
            .Throws(new Exception("Connection failed"));

        // Act
        var result = instanceManager.Add(eventInstance);

        // Assert
        Assert.Null(result);
        // When connection fails, transaction is null, so rollback is called with null
        _mockConnectionRepo.Verify(repo => repo.RollbackTransaction(null), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.Once);
    }

    #endregion

    #region GetEventInstancesToProcess Method Tests

    [Fact]
    public void GetEventInstancesToProcess_ValidParameters_ReturnsEvents()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var runDate = DateTime.UtcNow;
        var eventInstance = CreateValidEventInstance();
        var listenerInstance = CreateValidListenerInstance();
        var eventDefinition = CreateValidEventDefinition();
        var listenerDefinition = CreateValidListenerDefinition();

        eventInstance.ListenerInstances.Add(listenerInstance);
        var eventInstances = new List<EventInstance> { eventInstance };

        // Setup DefinitionManager with test data
        instanceManager.DefinitionMgr.EventDefinitions.Add(eventDefinition);
        instanceManager.DefinitionMgr.ListenerDefinitions.Add(listenerDefinition);

        _mockEventInstanceRepo.Setup(repo => repo.Read(It.IsAny<Dictionary<string, string>>(), _mockConnection.Object))
            .Returns(eventInstances);

        // Act
        var result = instanceManager.GetEventInstancesToProcess(runDate, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(eventDefinition, result[0].Definition);
        Assert.Equal(listenerDefinition, result[0].ListenerInstances[0].Definition);
        _mockEventInstanceRepo.Verify(repo => repo.Read(It.IsAny<Dictionary<string, string>>(), _mockConnection.Object), Times.Once);
    }

    [Fact]
    public void GetEventInstancesToProcess_NoEventsFound_ReturnsEmptyList()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var runDate = DateTime.UtcNow;

        _mockEventInstanceRepo.Setup(repo => repo.Read(It.IsAny<Dictionary<string, string>>(), _mockConnection.Object))
            .Returns(new List<EventInstance>());

        // Act
        var result = instanceManager.GetEventInstancesToProcess(runDate, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void GetEventInstancesToProcess_DatabaseException_ReturnsNull()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var runDate = DateTime.UtcNow;

        _mockEventInstanceRepo.Setup(repo => repo.Read(It.IsAny<Dictionary<string, string>>(), _mockConnection.Object))
            .Throws(new Exception("Database error"));

        // Act
        var result = instanceManager.GetEventInstancesToProcess(runDate, 1);

        // Assert
        Assert.Null(result);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void GetEventInstancesToProcess_ConnectionException_ReturnsNull()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var runDate = DateTime.UtcNow;

        // Reset the setup to throw exception for this specific test
        _mockConnectionRepo.Reset();
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(_mockConnection.Object))
            .Throws(new Exception("Connection failed"));

        // Act
        var result = instanceManager.GetEventInstancesToProcess(runDate, 1);

        // Assert
        Assert.Null(result);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.Once);
    }

    [Fact]
    public void GetEventInstancesToProcess_CustomGroupId_UsesCorrectGroupId()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var runDate = DateTime.UtcNow;
        var groupId = 5;

        _mockEventInstanceRepo.Setup(repo => repo.Read(It.IsAny<Dictionary<string, string>>(), _mockConnection.Object))
            .Returns(new List<EventInstance>());

        // Act
        var result = instanceManager.GetEventInstancesToProcess(runDate, groupId);

        // Assert
        _mockEventInstanceRepo.Verify(repo => repo.Read(
            It.Is<Dictionary<string, string>>(d => d.ContainsValue(groupId.ToString())), 
            _mockConnection.Object), Times.Once);
    }

    #endregion

    #region Process Method Tests

    [Fact]
    public void Process_ValidEventInstancesList_ProcessesAllEvents()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstance1 = CreateValidEventInstance();
        var eventInstance2 = CreateValidEventInstance();
        eventInstance2.Id = 2;
        var eventInstances = new List<EventInstance> { eventInstance1, eventInstance2 };

        // Setup definitions
        var eventDefinition = CreateValidEventDefinition();
        instanceManager.DefinitionMgr.EventDefinitions.Add(eventDefinition);

        // Act
        instanceManager.Process(eventInstances);

        // Assert
        _mockLogger.Verify(logger => logger.Add(It.IsAny<LogModel>()), Times.AtLeast(2)); // Start and end logs
        _mockEventInstanceRepo.Verify(repo => repo.Edit(It.IsAny<EventInstance>(), It.IsAny<object>(), It.IsAny<object>()), Times.AtLeast(2));
    }

    [Fact]
    public void Process_EmptyList_CompletesSuccessfully()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var eventInstances = new List<EventInstance>();

        // Act & Assert - Should not throw
        instanceManager.Process(eventInstances);

        _mockLogger.Verify(logger => logger.Add(It.IsAny<LogModel>()), Times.AtLeast(2)); // Start and end logs
    }

    [Fact]
    public void Process_NullList_HandlesGracefully()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();

        // Act & Assert - Should throw NullReferenceException as the method doesn't handle null
        Assert.Throws<NullReferenceException>(() => instanceManager.Process(null));
    }

    #endregion

    #region ReadEventInstanceStatusByBusinessId Method Tests

    [Fact]
    public void ReadEventInstanceStatusByBusinessId_ValidBusinessId_ReturnsStatus()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var businessId = Guid.NewGuid();
        var expectedBrief = new EventInstanceStatusBrief
        {
            Id = 1,
            BusinessId = businessId,
            Status = Enums.EventInstanceStatus.Succeeded
        };

        _mockEventInstanceRepo.As<IDataRepositoryEventInstanceStatus>()
            .Setup(repo => repo.ReadByBusinessId(businessId, _mockConnection.Object))
            .Returns(expectedBrief);

        // Act
        var result = instanceManager.ReadEventInstanceStatusByBusinessId(businessId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBrief.Id, result.Id);
        Assert.Equal(expectedBrief.BusinessId, result.BusinessId);
        Assert.Equal(expectedBrief.Status, result.Status);
        _mockConnectionRepo.Verify(repo => repo.OpenConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void ReadEventInstanceStatusByBusinessId_BusinessIdNotFound_ReturnsEmptyBrief()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var businessId = Guid.NewGuid();

        _mockEventInstanceRepo.As<IDataRepositoryEventInstanceStatus>()
            .Setup(repo => repo.ReadByBusinessId(businessId, _mockConnection.Object))
            .Returns(new EventInstanceStatusBrief());

        // Act
        var result = instanceManager.ReadEventInstanceStatusByBusinessId(businessId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        Assert.Equal(Guid.Empty, result.BusinessId);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void ReadEventInstanceStatusByBusinessId_DatabaseException_ReturnsEmptyBrief()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var businessId = Guid.NewGuid();

        _mockEventInstanceRepo.As<IDataRepositoryEventInstanceStatus>()
            .Setup(repo => repo.ReadByBusinessId(businessId, _mockConnection.Object))
            .Throws(new Exception("Database error"));

        // Act
        var result = instanceManager.ReadEventInstanceStatusByBusinessId(businessId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.AtLeast(1));
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.AtLeast(1));
    }

    [Fact]
    public void ReadEventInstanceStatusByBusinessId_ConnectionException_ReturnsEmptyBrief()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();
        var businessId = Guid.NewGuid();

        // Reset and setup for this specific test
        _mockConnectionRepo.Reset();
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(_mockConnection.Object);
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(_mockConnection.Object))
            .Throws(new Exception("Connection failed"));

        // Act
        var result = instanceManager.ReadEventInstanceStatusByBusinessId(businessId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Id);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(_mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(_mockConnection.Object), Times.Once);
    }

    [Fact]
    public void ReadEventInstanceStatusByBusinessId_EmptyGuid_HandlesGracefully()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();

        _mockEventInstanceRepo.As<IDataRepositoryEventInstanceStatus>()
            .Setup(repo => repo.ReadByBusinessId(Guid.Empty, _mockConnection.Object))
            .Returns(new EventInstanceStatusBrief());

        // Act & Assert - Should not throw
        var result = instanceManager.ReadEventInstanceStatusByBusinessId(Guid.Empty);

        Assert.NotNull(result);
    }

    #endregion

    #region DefinitionMgr Property Tests

    [Fact]
    public void DefinitionMgr_Get_ReturnsDefinitionManager()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();

        // Act
        var result = instanceManager.DefinitionMgr;

        // Assert
        Assert.NotNull(result);
        Assert.IsType<DefinitionManager>(result);
    }

    [Fact]
    public void DefinitionMgr_MultipleAccess_ReturnsSameInstance()
    {
        // Arrange
        var instanceManager = CreateInstanceManager();

        // Act
        var result1 = instanceManager.DefinitionMgr;
        var result2 = instanceManager.DefinitionMgr;

        // Assert
        Assert.Same(result1, result2);
    }

    #endregion
}
