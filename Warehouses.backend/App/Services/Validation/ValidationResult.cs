using Warehouses.backend.Services;

namespace Warehouses.backend.App.Services.Validation;

/// <summary>
/// Результат валидации площадки
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public PlatformAbsorptionResult? AbsorptionResult { get; private set; }

    private ValidationResult(bool isValid, string? errorMessage = null, PlatformAbsorptionResult? absorptionResult = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
        AbsorptionResult = absorptionResult;
    }

    public static ValidationResult Success(PlatformAbsorptionResult? absorptionResult = null)
        => new ValidationResult(true, absorptionResult: absorptionResult);

    public static ValidationResult Error(string message)
        => new ValidationResult(false, message);
}