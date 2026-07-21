namespace HR.Common.Mapping;

using Mapster;

/// <summary>
/// Interface for mapping configuration using Mapster.
/// </summary>
public interface IMapFrom<T>
{
    void Mapping(TypeAdapterConfig config);
}

/// <summary>
/// Interface for mapping from multiple source types.
/// </summary>
public interface IMapTo<T>
{
    void Mapping(TypeAdapterConfig config);
}
