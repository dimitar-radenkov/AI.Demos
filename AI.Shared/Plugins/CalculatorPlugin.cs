using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace AI.Shared.Plugins;

public sealed class CalculatorPlugin
{
    [KernelFunction("add_numbers")]
    [Description("Adds two numbers together.")]
    public static decimal Add(decimal a, decimal b) => a + b;

    [KernelFunction("subtract_numbers")]
    [Description("Subtracts the second number from the first.")]
    public static decimal Subtract(decimal a, decimal b) => a - b;

    [KernelFunction("multiply_numbers")]
    [Description("Multiplies two numbers together.")]
    public static decimal Multiply(decimal a, decimal b) => a * b;

    [KernelFunction("divide_numbers")]
    [Description("Divides the first number by the second.")]
    public static decimal Divide(decimal a, decimal b)
    {
        if (b == 0)
        {
            throw new DivideByZeroException("The divisor cannot be zero.");
        }

        return a / b;
    }
}
