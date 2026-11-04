namespace QTKD.Api.Services;

public class ContractValidationException : Exception
{
    public ContractValidationException(IReadOnlyList<string> errors)
        : base("The contract does not meet CTR-02.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
