using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using ShopDemo.Core.Entities.Persons.Enums;
using ShopDemo.Core.Entities.Persons.ValueObjects;

namespace ShopDemo.Core.Entities.Persons;

public interface IPerson
    : IEntity
{
    public PersonType PersonType { get; }
    /*
    Uma person (pessoa) não possui nome diretamente, nome e sobre nome pertencem a 
    pessoas físicas (individual persons) e razão social (legal name) e nome fantasia
    (trade name) pertencem a pessoas jurídicas (legal persons).

    O que person tem, na verdade, é o nome de exibição (display name), que é uma
    propriedade derivada dos nomes específicos de cada tipo de pessoa.
    */
    public string DisplayName { get; }
    public EmailAddress EmailAddress { get; }
    /*
    Uma person (pessoa) possui uma coleção de identificadores (person identifiers)
    que são representados pela interface IPersonIdentifier. Esses identificadores podem
    variar dependendo do tipo de pessoa (física ou jurídica) e do país de origem.

    A coleção de identificadores é uma array de IPersonIdentifier, permitindo que
    uma pessoa tenha múltiplos identificadores associados a ela, como CPF, CNPJ, etc.
    */
    public IPersonIdentifier[] PersonIdentifierCollection { get; }
}
