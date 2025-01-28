
namespace BO;

/// <summary>
/// Exception thrown when an entity does not exist.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an entity already exists.
/// </summary>
[Serializable]
public class BlDuplicateEntityException : Exception
{
    public BlDuplicateEntityException(string? message) : base(message) { }
    public BlDuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when deletion of an entity is not allowed.
/// </summary>
[Serializable]
public class BlDeletionImpossibleException : Exception
{
    public BlDeletionImpossibleException(string? message) : base(message) { }
    public BlDeletionImpossibleException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when authorization fails.
/// </summary>
[Serializable]
public class BlAuthorizationException : Exception
{
    public BlAuthorizationException(string? message) : base(message) { }
    public BlAuthorizationException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a property is null.
/// </summary>
[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a provided value is invalid.
/// </summary>
[Serializable]
public class BlInvalidValueException : Exception
{
    public BlInvalidValueException(string? message) : base(message) { }
    public BlInvalidValueException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when logical operations fail.
/// </summary>
[Serializable]
public class BlLogicException : Exception
{
    public BlLogicException(string? message) : base(message) { }
    public BlLogicException(string message, Exception innerException) : base(message, innerException) { }
}

[Serializable]
public class BlValidationException : Exception
{
    public BlValidationException(string? message) : base(message) { }
    public BlValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for generic BL-level errors.
/// </summary>
[Serializable]
public class BlException : Exception
{
    public BlException(string? message) : base(message) { }
    public BlException(string message, Exception innerException) : base(message, innerException) { }
}

