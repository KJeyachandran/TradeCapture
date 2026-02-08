using Moq;
using TradeCapture.Foundation.Events;
using TradeCapture.Foundation.Orders;
using Xunit;

namespace TradeCapture.Tests.UnitTests;

public class OrderTests
{
    private const int DEFAULT_QUANTITY = 100;

    [Fact]
    public void WithNullOrderService_ConstructorThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Order(null!, 100m));
    }

    [Theory]
    [InlineData(99.99, true)]  // Below threshold
    [InlineData(100.00, false)] // Equal to threshold
    [InlineData(100.01, false)] // Above threshold
    public void RespondToTick_BuyWithCorrectPriceThreshold(decimal price, bool shouldBuy)
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);
        bool placedEventRaised = false;
        order.Placed += (args) => placedEventRaised = true;

        // Act
        order.RespondToTick("ABCD", price);

        // Assert
        if (shouldBuy)
        {
            mockOrderService.Verify(s => s.Buy("ABCD", It.IsAny<int>(), price), Times.Once);
            Assert.True(placedEventRaised);
        }
        else
        {
            mockOrderService.Verify(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
            Assert.False(placedEventRaised);
        }
    }

    [Fact]
    public void RespondToTick_BuyWithException_RaisesErrored()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var expectedException = new InvalidOperationException("Buy failed");
        mockOrderService.Setup(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                        .Throws(expectedException);

        var order = new Order(mockOrderService.Object, 100m);
        bool erroredEventRaised = false;
        ErroredEventArgs capturedArgs = null!;
        order.Errored += (args) =>
        {
            erroredEventRaised = true;
            capturedArgs = args;
        };

        // Act
        order.RespondToTick("ABCD", 99m);

        // Assert
        Assert.True(erroredEventRaised);
        Assert.NotNull(capturedArgs);
        Assert.Equal("ABCD", capturedArgs.Code);
        Assert.Equal(99m, capturedArgs.Price);
        Assert.Same(expectedException, capturedArgs.Exception);
    }

    [Fact]
    public void RespondToTick_NoFurtherBuys_AfterSuccessfulBuy()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);
        int buyCount = 0;
        mockOrderService.Setup(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                        .Callback(() => buyCount++);

        // Act
        order.RespondToTick("ABCD", 99m);   // first call(should buy)
        order.RespondToTick("ABCD", 95m);   // second call (should not buy)
        order.RespondToTick("ABCD", 90m);   // third call (should not buy)

        // Assert
        Assert.Equal(1, buyCount);
        mockOrderService.Verify(s => s.Buy("ABCD", It.IsAny<int>(), 99m), Times.Once);
    }

    [Fact]
    public void RespondToTick_NoFurtherBuys_AfterErrorEvent()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        mockOrderService.SetupSequence(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                        .Throws<InvalidOperationException>()
                        .Throws<InvalidOperationException>(); // Should never be called

        var order = new Order(mockOrderService.Object, 100m);

        // Act
        order.RespondToTick("ABCD", 99m);   // first call (should error)
        
        order.RespondToTick("ABCD", 95m);   // second call (should not attempt buy due to error)

        // Assert
        mockOrderService.Verify(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Once);
    }

    [Fact]
    public void RespondToTick_NullOrEmptyCode_ReturnsWithoutBuy()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);

        // Act
        order.RespondToTick(null!, 99m);
        order.RespondToTick("", 99m);
        order.RespondToTick("   ", 99m);

        // Assert
        mockOrderService.Verify(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task RespondToTick_ThreadSafe_MultipleThreadsOnlyBuyOnce()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);
        int buyCount = 0;
        mockOrderService.Setup(s => s.Buy(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<decimal>()))
                        .Callback(() => Interlocked.Increment(ref buyCount));

        // Act
        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            tasks[i] = Task.Run(() => order.RespondToTick("ABCD", 99m));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(1, buyCount);
    }

    [Fact]
    public void RespondToTick_EventHandlers_CanBeNull()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);
        // No event handlers attached

        // Act & Assert (should not throw)
        var exception = Record.Exception(() => order.RespondToTick("ABCD", 99m));
        Assert.Null(exception);
    }


    [Fact]
    public void RespondToTick_BuyIsPlaced_WhenPriceBelowThreshold()
    {
        var mockOrderService = new Mock<IOrderService>();
        var order = new Order(mockOrderService.Object, 100m);

        PlacedEventArgs? placed = null;
        order.Placed += e => placed = e;

        order.RespondToTick("XYZ", 90m);

        mockOrderService.Verify(s => s.Buy("XYZ", DEFAULT_QUANTITY, 90m), Times.Once);
        Assert.NotNull(placed);
    }

    [Fact]
    public void RespondToTick_OnlyOneBuyIsPerformed_WhenCalledFromMultipleThreads()
    {
        var service = new Mock<IOrderService>();
        var order = new Order(service.Object, 100m);

        Parallel.For(0, 100, _ =>
        {
            order.RespondToTick("XYZ", 90m);
        });

        service.Verify(s => s.Buy("XYZ", DEFAULT_QUANTITY, 90m), Times.Once);
    }

    [Fact]
    public void RespondToTick_ErrorRaised_WhenBuyFails()
    {
        var service = new Mock<IOrderService>();
        service.Setup(s => s.Buy(It.IsAny<string>(), DEFAULT_QUANTITY, It.IsAny<decimal>()))
               .Throws(new InvalidOperationException("Boom"));

        var order = new Order(service.Object, 100m);

        ErroredEventArgs? errored = null;
        order.Errored += e => errored = e;

        order.RespondToTick("ABC", 90m);

        Assert.NotNull(errored);
        Assert.IsType<InvalidOperationException>(errored!.Exception);
    }

    [Fact]
    public void RespondToTick_NoFurtherBuys_AfterError()
    {
        var service = new Mock<IOrderService>();
        service.Setup(s => s.Buy(It.IsAny<string>(), DEFAULT_QUANTITY, It.IsAny<decimal>()))
               .Throws(new Exception());

        var order = new Order(service.Object, 100m);

        order.RespondToTick("ABC", 90m);
        order.RespondToTick("ABC", 80m);

        service.Verify(s => s.Buy(It.IsAny<string>(), DEFAULT_QUANTITY, It.IsAny<decimal>()), Times.Once);
    }
}