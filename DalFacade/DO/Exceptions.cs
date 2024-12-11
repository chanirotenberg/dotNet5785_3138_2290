namespace DO;
/// <summary>
/// Exception thrown when an object does not exist in the data layer.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalDoesNotExistException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalDoesNotExistException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when an object already exists in the data layer.
/// </summary>
public class DalAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalAlreadyExistsException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalAlreadyExistsException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when an object cannot be deleted from the data layer.
/// </summary>
public class DalDeletionImpossible : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalDeletionImpossible"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalDeletionImpossible(string? message) : base(message) { }
}

/// <summary>
/// 
/// </summary>////////////////////////////////////////////////////////////////
public class DalXMLFileLoadCreateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DalXMLFileLoadCreateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}
