namespace Pfm.Domain.Exceptions;

/// <summary>
/// A business rule that needs stored state was violated, for example a transaction whose type does
/// not match its category. Translated to HTTP 400 by the API, reported against
/// <see cref="PropertyName"/> so the UI can show it on the offending form control.
/// </summary>
public sealed class ValidationException(string propertyName, string message) : Exception(message)
{
    public string PropertyName { get; } = propertyName;
}
