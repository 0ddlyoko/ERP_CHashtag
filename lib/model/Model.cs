using System.Collections;
using lib.field;
using lib.field.attributes;

namespace lib.model;

/*
 * Model class that will be inherited from models
 */
public class Model: IEnumerable, IEnumerator, IEquatable<Model>
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
    public void Update(IReadOnlyDictionary<string, object?> data) => Env.UpdateFields(ModelName, Ids, data);
    
    public T Transform<T>() where T : Model => Env.Get<T>(Ids);

    /**
     * Retrieves the value of given field
     * If recompute is False, we do not recompute the field if it needs to be recomputed, except if ToRetrieve is true.
     */
    public T? Get<T>(string fieldName, bool recompute = true)
    {
        if (fieldName == "Id")
        {
            if (Ids.Count != 1)
                throw new InvalidOperationException($"Cannot unpack: there is more than one record (got {Ids})"); 
            return (T)(object) Ids[0];
        }

        if (Ids.Count != 1)
        {
            FinalField field = Env.PluginManager.GetFinalModel(ModelName).Fields[fieldName];
            if (field.FieldType is not FieldType.ManyToOne and not FieldType.OneToMany and not FieldType.ManyToMany)
                throw new InvalidOperationException($"Cannot unpack: there is more than one record ({Ids})");
        }

        List<object?> results = Env.RetrieveField(Ids, ModelName, fieldName, recompute: recompute);
        // If result is a model, we know that the target field is either a M2O, O2M or an M2M
        // Transform the generic Model into an instance of needed model
        object? result;
        if (typeof(Model).IsAssignableFrom(typeof(T)))
        {
            List<int> ids = [];
            foreach (var rslt in results)
            {
                ids.AddRange((rslt as Model).Ids);
            }

            result = typeof(T) == typeof(Model) ? Env.Get(ids, ModelName) : Env.Get(ids, typeof(T));
        }
        else
        {
            result = results[0];
        }
        
        return (T?) result;
    }

    /**
     * Change the value of the field
     */
    public void Set(string fieldName, object? value)
    {
        Update(new Dictionary<string, object?>
        {
            { fieldName, value },
        });
    }

    public bool Equals(Model? other)
    {
        if (other == null)
            return false;
        return ModelName == other.ModelName && Ids.Count == other.Ids.Count && Ids.All(other.Ids.Contains);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Model)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Ids, ModelName);
    }

    public override string ToString()
    {
        return $"{ModelName}[{string.Join(", ", Ids)}]";
    }

    public IEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        _position++;
        return _position <= Ids.Count;
    }

    public void Reset()
    {
        _position = 0;
    }

    private int _position;
    public object Current => Env.Get([Ids[_position - 1]], GetType());

}