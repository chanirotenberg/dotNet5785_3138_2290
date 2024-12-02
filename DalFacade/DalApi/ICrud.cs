using DO;

namespace DalApi;

public interface ICrud<T> where T : class
{
    /// <summary>
    /// Creates a new entity in the DAL.
    /// </summary>
    void Create(T item);

    /// <summary>
    /// Reads an entity by its ID.
    /// </summary>
    T? Read(int id);

    /// <summary>
    /// Reads all entities, optionally filtered.
    /// </summary>
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(T item);

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    void Delete(int id);

    /// <summary>
    /// Deletes all entities.
    /// </summary>
    void DeleteAll();

    /// <summary>
    /// Reads an entity based on a filter function.
    /// </summary>
    T? Read(Func<T, bool> filter);
}
