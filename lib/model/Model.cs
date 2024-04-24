using lib.field;

namespace lib.model;

/**
 * Model class that will be inherited from models
 */
public class Model
{
    [FieldDefinition(Name = "Id", Description = "Id of the record")]
    public int Id;
    // TODO Add creation date & update date

    public Environment Env;

    /**
     * Clear data not saved to environment, and restore default data.
     */
    public void Reset() => Env.ResetModelToCacheState(this);

    public void Save() => Env.Save(this);
}
