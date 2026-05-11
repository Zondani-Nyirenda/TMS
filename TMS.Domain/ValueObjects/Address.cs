namespace TMS.Domain.ValueObjects;

/// <summary>
/// Immutable value object representing a physical address.
/// Two Address instances are equal if all their fields are equal.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    public string Street { get; }
    public string City { get; }
    public string Province { get; }
    public string PostalCode { get; }
    public string Country { get; }

    // Required by EF Core
    private Address() { Street = City = Province = PostalCode = Country = string.Empty; }

    public Address(string street, string city, string province, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street is required.", nameof(street));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City is required.", nameof(city));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country is required.", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        Province = province?.Trim() ?? string.Empty;
        PostalCode = postalCode?.Trim() ?? string.Empty;
        Country = country.Trim();
    }

    public override string ToString() =>
        $"{Street}, {City}, {Province} {PostalCode}, {Country}".Trim(' ', ',');

    public bool Equals(Address? other) =>
        other is not null &&
        Street == other.Street &&
        City == other.City &&
        Province == other.Province &&
        PostalCode == other.PostalCode &&
        Country == other.Country;

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() =>
        HashCode.Combine(Street, City, Province, PostalCode, Country);
}
