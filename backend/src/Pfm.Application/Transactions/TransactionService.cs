using Pfm.Application.Abstractions;
using Pfm.Application.Categories;
using Pfm.Domain;
using Pfm.Domain.Entities;
using Pfm.Domain.Exceptions;

namespace Pfm.Application.Transactions;

/// <summary>
/// Use cases for transactions. Enforces that a transaction always matches the type of the category
/// it is booked on (business rule 1).
/// </summary>
public sealed class TransactionService(ITransactionRepository transactions, ICategoryRepository categories)
{
    public async Task<IReadOnlyList<TransactionResponse>> GetAsync(
        TransactionFilter filter,
        CancellationToken cancellationToken)
    {
        var stored = await transactions.GetAsync(filter, cancellationToken);
        if (stored.Count == 0)
        {
            return [];
        }

        var byId = (await categories.GetAllAsync(cancellationToken)).ToDictionary(category => category.Id);
        return [.. stored.Select(transaction => ToResponse(transaction, byId[transaction.CategoryId]))];
    }

    public async Task<TransactionResponse> GetAsync(string id, CancellationToken cancellationToken)
    {
        var transaction = await transactions.GetByIdAsync(id, cancellationToken) ?? throw NotFound(id);
        var byId = (await categories.GetAllAsync(cancellationToken)).ToDictionary(category => category.Id);
        return ToResponse(transaction, byId[transaction.CategoryId]);
    }

    public async Task<TransactionResponse> CreateAsync(TransactionRequest request, CancellationToken cancellationToken)
    {
        var category = await ResolveCategoryAsync(request, cancellationToken);

        var transaction = new Transaction
        {
            Type = request.Type,
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = category.Id,
            Description = NormaliseDescription(request.Description)
        };

        await transactions.InsertAsync(transaction, cancellationToken);
        return ToResponse(transaction, category);
    }

    public async Task<TransactionResponse> UpdateAsync(
        string id,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await transactions.GetByIdAsync(id, cancellationToken) ?? throw NotFound(id);
        var category = await ResolveCategoryAsync(request, cancellationToken);

        transaction.Type = request.Type;
        transaction.Amount = request.Amount;
        transaction.Date = request.Date;
        transaction.CategoryId = category.Id;
        transaction.Description = NormaliseDescription(request.Description);

        if (!await transactions.UpdateAsync(transaction, cancellationToken))
        {
            throw NotFound(id);
        }

        return ToResponse(transaction, category);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken)
    {
        if (!await transactions.DeleteAsync(id, cancellationToken))
        {
            throw NotFound(id);
        }
    }

    private async Task<Category> ResolveCategoryAsync(TransactionRequest request, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new ValidationException("categoryId", "Die ausgewählte Kategorie existiert nicht.");

        if (category.Type != request.Type)
        {
            throw new ValidationException(
                "categoryId",
                $"\"{category.Name}\" ist {Describe(category.Type)} und passt nicht zum gewählten Typ.");
        }

        return category;
    }

    private static string Describe(TransactionType type) =>
        type == TransactionType.Income ? "eine Einnahmenkategorie" : "eine Ausgabenkategorie";

    private static string? NormaliseDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static TransactionResponse ToResponse(Transaction transaction, Category category) =>
        new(transaction.Id,
            transaction.Type,
            transaction.Amount,
            transaction.Date,
            CategoryResponse.From(category),
            transaction.Description);

    private static NotFoundException NotFound(string id) =>
        new($"Die Transaktion \"{id}\" wurde nicht gefunden.");
}
