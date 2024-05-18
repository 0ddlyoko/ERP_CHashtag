using lib.field;
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
    public required List<int> Ids;
    public required string ModelName;

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
    public void Update(IReadOnlyDictionary<string, object?> data) => Env.Update(ModelName, Ids, data);
    
    public T Transform<T>() where T : Model => Env.Get<T>(Ids);

    /**
     * Retrieves the value of given field
     */
    protected T? Get<T>(string fieldName)
    {
        if (fieldName == "Id")
        {
            if (Ids.Count != 1)
                throw new InvalidOperationException($"Cannot unpack: there is more than one record ({Ids.Count})"); 
            return (T)(object)Ids[0];
        }

        return (T?) Env.GetField(ModelName, Ids, fieldName);
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