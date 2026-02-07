namespace Tests.SignaledPattern;

public sealed class ClassWithConcurrency
{
    private readonly IOtherClass _otherClass;

    public ClassWithConcurrency(IOtherClass otherClass)
    {
        _otherClass = otherClass;
    }

    public int Result { get; private set; }

    public void FireAndForgetCall(int a, int b)
    {
        Task.Run(() =>
        {
            Result = a + b;
            _otherClass.DoSomething(Result);
        });
    }
}
