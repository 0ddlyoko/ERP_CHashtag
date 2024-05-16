using lib.field.attributes;

namespace lib.model;

/*
 * Model class that will be inherited from models
 */
public class Model
{
    [FieldDefinition(Name = "Id", Description = "Id of the record")]
    public int Id => Get<int>("Id");

    [FieldDefinition(Name = "CreationDate", Description = "Date of when the record has been created")]
    public DateTime CreationDate => Get<DateTime>("CreationDate");

    [FieldDefinition(Name = "UpdateDate", Description = "Date of when the record has been updated for the last time")]
    public DateTime UpdateDate => Get<DateTime>("UpdateDate");

    public required Environment Env;

    /**
     * Cache of this model.
     * Private usage, please never access to this cache nor modify it otherwise unknown behavior will occur
     */
    public required CachedModel CachedModel;

    /**
     * Save this model to the database.
     * This is mostly not needed as, in most cases, the save is automatically performed by the ORM
     */
    public void Save() => Env.Save(this);

    /**
     * Update model based on given dictionary.
     * This will also save the model, so any modification made before calling this method will also be saved before
     * saving given data
     */
    public void Update(IReadOnlyDictionary<string, object?> data) => Env.Update(CachedModel.Model.Name, Id, data);
    
    public T Transform<T>() where T : Model => Env.Get<T>(Id);

    /**
     * Retrieves the value of given field
     */
    protected T? Get<T>(string fieldName)
    { 
        if (fieldName == "Id")
            return (T) (object) CachedModel.Id;
        object? realValue = CachedModel.Fields[fieldName].GetRealValue();
        // TODO Check if null is good here
        return (T?) realValue;
    }

    /**
     * Change the value of the field
     */
    protected void Set(string fieldName, object? value)
    {
        Update(new Dictionary<string, object?>
        {
            { fieldName, value },
        });
    }
}