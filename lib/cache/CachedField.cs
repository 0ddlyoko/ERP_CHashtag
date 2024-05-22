using lib.field;
using lib.model;

namespace lib.cache;

/**
 * Field cache per environment
 */
public class CachedField
{
    public Environment Env => CachedModel.Env;
    public required CachedModel CachedModel;
    public required FinalField Field;
    public object? Value;
    // If ToRetrieve, this value needs to be retrieved from the database
    public bool ToRetrieve = true;

    /**
     * Modify this field
     */
    public bool UpdateField(object? newValue)
    {
        // Special case: Date
        if (Field.FieldType == FieldType.Date && newValue != null)
        {
            newValue = ((DateTime)newValue).Date;
        }
        // Special case: ManyToOne
        else if (Field.FieldType is FieldType.ManyToOne)
        {
            // A Model should be given here (or null)
            if (newValue is Model model)
                newValue = model.Id;
            else if (newValue is not int and not null)
                throw new InvalidOperationException($"Given value for field {Field.FieldName} is not valid, you should enter null, an integer or a model of type {Field.TargetType}");
        } 
        // Special case: OneToMany or ManyToMany
        else if (Field.FieldType is FieldType.OneToMany or FieldType.ManyToMany)
        {
            // A Model should be given here (or null)
            if (newValue == null)
                newValue = new List<int>();
            else if (newValue is Model model)
                newValue = model.Ids;
            else if (newValue is int i)
                newValue = new List<int> { i };
            else if (newValue is not List<int>)
                throw new InvalidOperationException($"Given value for field {Field.FieldName} is not valid, you should enter null, a list of integer or a model of type {Field.TargetType}");
        }
        Value = newValue;
        ToRetrieve = false;
        return true;
    }
}
