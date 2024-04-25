using lib.field;

namespace lib.model;

/**
 * Model class that will be inherited from models
 */
public class Model
{
    [FieldDefinition(Name = "Id", Description = "Id of the record")]
    public required int Id;
    // TODO Add creation date & update date

    public required Environment Env;

    /**
     * Clear data not saved to environment, and restore default data.
     */
    public void Reset() => Env.ResetModelToCacheState(this);

    /**
     * Save fields to the cache.
     */
    public void Save() => Env.Save(this);

    /**
     * Update model based on given dictionary.
     * This will also save the model, so any modification made before calling this method will also be saved before
     * saving given data
     */
    public void Update(IReadOnlyDictionary<string, object?> data) => Env.Update(this, data);

    public T Transform<T>() where T : Model => Env.Get<T>(Id);
}
