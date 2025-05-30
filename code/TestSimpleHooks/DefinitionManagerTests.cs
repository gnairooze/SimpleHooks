namespace TestSimpleHooks;
using Business;
using Interfaces;
using Log.Interface;
using Models.Definition;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;

public class DefinitionManagerTests
{
    private readonly Mock<ILog> _mockLogger;
    private readonly Mock<IConnectionRepository> _mockConnectionRepo;
    private readonly Mock<IDataRepository<AppOption>> _mockAppOptionRepo;
    private readonly Mock<IDataRepository<EventDefinition>> _mockEventDefRepo;
    private readonly Mock<IDataRepository<ListenerDefinition>> _mockListenerDefRepo;
    private readonly Mock<IDataRepository<EventDefinitionListenerDefinition>> _mockEventDefListenerDefRepo;
    private readonly DefinitionManager _definitionManager;

    public DefinitionManagerTests()
    {
        _mockLogger = new Mock<ILog>();
        _mockConnectionRepo = new Mock<IConnectionRepository>();
        _mockAppOptionRepo = new Mock<IDataRepository<AppOption>>();
        _mockEventDefRepo = new Mock<IDataRepository<EventDefinition>>();
        _mockListenerDefRepo = new Mock<IDataRepository<ListenerDefinition>>();
        _mockEventDefListenerDefRepo = new Mock<IDataRepository<EventDefinitionListenerDefinition>>();

        _definitionManager = new DefinitionManager(
            _mockLogger.Object,
            _mockEventDefRepo.Object,
            _mockListenerDefRepo.Object,
            _mockEventDefListenerDefRepo.Object,
            _mockAppOptionRepo.Object,
            _mockConnectionRepo.Object);
    }

    [Fact]
    public void LoadDefinitions_Success()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(mockConnection.Object);
        _mockAppOptionRepo.Setup(repo => repo.Read(null, mockConnection.Object)).Returns([]);
        _mockEventDefRepo.Setup(repo => repo.Read(null, mockConnection.Object)).Returns([]);
        _mockListenerDefRepo.Setup(repo => repo.Read(null, mockConnection.Object)).Returns([]);
        _mockEventDefListenerDefRepo.Setup(repo => repo.Read(null, mockConnection.Object)).Returns([]);

        // Act
        var result = _definitionManager.LoadDefinitions();

        // Assert
        Assert.True(result);
        _mockLogger.Verify(logger => logger.Add(It.IsAny<LogModel>()), Times.Exactly(2));
        _mockConnectionRepo.Verify(repo => repo.OpenConnection(mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(mockConnection.Object), Times.Once);
    }

    [Fact]
    public void LoadDefinitions_Exception()
    {
        // Arrange
        var mockConnection = new Mock<IDbConnection>();
        _mockConnectionRepo.Setup(repo => repo.CreateConnection()).Returns(mockConnection.Object);
        _mockConnectionRepo.Setup(repo => repo.OpenConnection(mockConnection.Object)).Throws(new Exception("Test exception"));

        // Act
        var result = _definitionManager.LoadDefinitions();

        // Assert
        Assert.False(result);
        _mockConnectionRepo.Verify(repo => repo.OpenConnection(mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.CloseConnection(mockConnection.Object), Times.Once);
        _mockConnectionRepo.Verify(repo => repo.DisposeConnection(mockConnection.Object), Times.Once);
    }
}