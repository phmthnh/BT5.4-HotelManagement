namespace HotelManagement.DTO;
public sealed record FieldDTO(string Column, string Caption, string Kind, int Length = 100, bool Required = true);
public sealed record CatalogDTO(string Table, string Title, string Key, bool Identity, FieldDTO[] Fields);
