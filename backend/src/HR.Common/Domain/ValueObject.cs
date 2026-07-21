namespace HR.Common.Domain;

/// <summary>
/// Base class for value objects.
/// Value objects are immutable objects defined by their attributes rather than their identity.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Get the atomic values that define this value object.
    /// </summary>
    protected abstract IEnumerable<object> GetAtomicValues();

    /// <summary>
    /// Override equality comparison.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;
        var thisValues = GetAtomicValues().GetEnumerator();
        var otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (ReferenceEquals(thisValues.Current, null) ^ ReferenceEquals(otherValues.Current, null))
                return false;

            if (thisValues.Current?.Equals(otherValues.Current) == false)
                return false;
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    /// <summary>
    /// Override hash code calculation.
    /// </summary>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(int), (hashcode, value) =>
                HashCode.Combine(hashcode, value));
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            return false;

        return ReferenceEquals(left, null) || left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
