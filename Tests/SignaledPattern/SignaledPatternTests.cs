using Moq;

namespace Tests.SignaledPattern;

public sealed class SignaledPatternTests
{
    [Test]
    public async Task TestUsingSignalAsync()
    {
        // Arrange
        var signal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var mockOtherClass = new Mock<IOtherClass>();
        mockOtherClass
            .Setup(static otherClass => otherClass.DoSomething(It.IsAny<int>()))
            .Callback<int>(value => signal.TrySetResult(value));

        var sut = new ClassWithConcurrency(mockOtherClass.Object);

        // Act
        sut.FireAndForgetCall(2, 3);

        var completed = await Task.WhenAny(signal.Task, Task.Delay(TimeSpan.FromSeconds(10)));

        // Assert
        var signalCompletedFirst = ReferenceEquals(completed, signal.Task);
        await Assert.That(signalCompletedFirst).IsTrue();
        await Assert.That(sut.Result).IsEqualTo(5);
        mockOtherClass.Verify(static otherClass => otherClass.DoSomething(5), Times.Once);
    }
}
