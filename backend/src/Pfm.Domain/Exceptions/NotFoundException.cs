namespace Pfm.Domain.Exceptions;

/// <summary>
/// The requested resource does not exist. Translated to HTTP 404 by the API.
/// </summary>
public sealed class NotFoundException(string message) : Exception(message);
