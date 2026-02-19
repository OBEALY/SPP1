namespace TestedProject;

public sealed class MathService
{
    public int Add(int a, int b) => a + b;

    public int Subtract(int a, int b) => a - b;

    public int Divide(int a, int b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }

        return a / b;
    }

    public async Task<int> MultiplyAsync(int a, int b)
    {
        await Task.Delay(30);
        return a * b;
    }
}
