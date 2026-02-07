using Moq;
using NUnit.Framework;

namespace Tests.SignaledPattern;

[TestFixture]
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
        Assert.That(completed, Is.SameAs(signal.Task), $"{nameof(IOtherClass)}.{nameof(IOtherClass.DoSomething)} was never called");
        Assert.That(sut.Result, Is.EqualTo(5));
        mockOtherClass.Verify(static otherClass => otherClass.DoSomething(5), Times.Once);
    }
}
