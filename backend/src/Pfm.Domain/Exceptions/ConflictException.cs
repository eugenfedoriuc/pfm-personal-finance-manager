namespace Pfm.Domain.Exceptions;

/// <summary>
/// The request is well formed but conflicts with the current state of the data, for example a
/// duplicate category name. Translated to HTTP 409 by the API.
/// </summary>
public sealed class ConflictException(string message) : Exception(message);
