using Pfm.Domain;

namespace Pfm.Application.Categories;

/// <summary>A category as returned by the API.</summary>
public sealed record CategoryResponse(string Id, string Name, TransactionType Type, string Icon, string Color);
