using lib.model;

namespace lib.field;

/**
 * Field cache per environment
 */
public class CachedField
{
    public required Environment Env;
    public required CachedModel CachedModel;
    public required FinalField Field;
    // If dirty, this need to be updated to the database
    public bool Dirty = false;
    // If ToRecompute, we need to recompute this value
    public bool ToRecompute = false;
    public object? Value;

    /**
     * Modify this field
     */
    public bool ModifyField(object? newValue)
    {
        // Value can be the same but this field needs to be recomputed.
        // If it's the case, remove the "ToRecompute" flag, as we don't need anymore to recompute it
        if (newValue == Value && !ToRecompute)
            return false;
        // Special case: Date
        if (Field.FieldType == FieldType.Date && newValue != null)
        {
            newValue = ((DateTime)newValue).Date;
        }
        Value = newValue;
        Dirty = true;
        ToRecompute = false;
        CachedModel.Dirty = true;
        PropagateToRecompute();
        return true;
    }

    /**
     * Propagate fields that depends on this one as a field that we need to recompute
     */
    private void PropagateToRecompute()
    {
        if (ToRecompute)
            return;
        // Do not recompute on those fields
        if (Field.Name is "Id" or "CreationDate" or "UpdateDate")
            return;
        foreach (string fieldToRecompute in Field.InverseCompute)
        {
            CachedModel? targetModel = CachedModel.GetCachedModelOfTargetField(fieldToRecompute);
            if (targetModel == null)
                continue;
            // Later, fieldToRecompute can be composed of multiple fields separated by a dot.
            // This is not the case now, but should be fixed soon.
            CachedField targetField = targetModel.Fields[fieldToRecompute];
            if (targetField.ToRecompute)
                continue;
            targetField.ToRecompute = true;
            targetField.PropagateToRecompute();
        }
    }

    /**
     * Recompute this field if we need to recompute it
     */
    public void RecomputeField()
    {
        if (!ToRecompute)
            return;
        // Before recomputing this field, recompute all fields that this one depends
        if (Field.DefaultComputedMethod == null)
            return;
        RecomputeDependentsFields();
        
        // Now, compute this field
        if (Field.LastOccurenceOfComputedMethod == null)
            throw new InvalidOperationException($"Cannot find last occurence of computed method for field {Field.Name} in model {CachedModel.Model.Name}");
        object? objInstance = Activator.CreateInstance(Field.LastOccurenceOfComputedMethod.PluginModel.Type);
        if (objInstance is not Model instance)
            throw new InvalidOperationException($"Created instance of type {Field.LastOccurence.Type} is not a Model! This should not occur");
        instance.Env = Env;
        instance.CachedModel = CachedModel;
        Field.DefaultComputedMethod?.MethodInfo?.Invoke(instance, null);
        
        ToRecompute = false;
    }

    /**
     * Recompute fields that this computed field needs.
     * If this computed fields doesn't need to be recomputed, do not compute dependent fields
     */
    private void RecomputeDependentsFields()
    {
        if (!ToRecompute)
            return;
        string[]? fields = Field.DefaultComputedMethod?.ComputedAttribute?.Fields;
        if (fields == null || fields.Length == 0) 
            return;
        foreach (string fieldToMaybeRecompute in fields)
        {
            CachedModel? targetModel = CachedModel.GetCachedModelOfTargetField(fieldToMaybeRecompute);
            // Later, fieldToRecompute can be composed of multiple fields separated by a dot.
            // This is not the case now, but should be fixed soon.
            targetModel?.Fields[fieldToMaybeRecompute].RecomputeField();
        }
    }

    /**
     * Retrieves real value, aka the latest up-to-date value.
     * If field needs to be recomputed, recompute it and return latest value
     */
    public object? GetRealValue()
    {
        RecomputeField();
        return Value;
    }
}
