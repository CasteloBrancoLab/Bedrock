using System.Diagnostics.CodeAnalysis;
using ShopDemo.Auth.Domain.Entities.ConsentTerms.Enums;

namespace ShopDemo.Auth.Domain.Entities.ConsentTerms.Inputs;

[ExcludeFromCodeCoverage(Justification = "Readonly record struct — Coverlet nao instrumenta construtor posicional gerado pelo compilador")]
public readonly record struct RegisterNewConsentTermInput(
    ConsentTermType Type,
    string TermVersion,
    string Content,
    DateTimeOffset PublishedAt
);
