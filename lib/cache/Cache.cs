using lib.field;
using lib.model;

namespace lib.cache;

public class Cache
{
    public readonly Environment Environment;
    // Dictionary containing ids with fields that need to be recomputed.
    // (model, (field, [ids]))
    public readonly Dictionary<string, Dictionary<string, HashSet<int>>> ToRecompute = new();
    // Dictionary containing ids with his corresponding CachedModel
    // (model, (id, CachedModel))
    public readonly Dictionary<string, Dictionary<int, CachedModel>> CachedModels = new();

    public Cache(Environment environment)
    {
        Environment = environment;
        foreach (var finalModel in environment.PluginManager.Models)
        {
            CachedModels[finalModel.Name] = new Dictionary<int, CachedModel>();
            ToRecompute[finalModel.Name] = new Dictionary<string, HashSet<int>>();
            foreach (var (fieldName, _) in finalModel.Fields)
            {
                ToRecompute[finalModel.Name][fieldName] = [];
            }
        }
    }

    /**
     * Retrieve given field for given records.
     * If the field need to be retrieved from the database, or need to be computed, compute it.
     * Return the cached value for each given ids as a list of data where the order is maintained
     */
    public List<object?> Get(Model records, FinalField field, bool recompute = true)
    {
        if (records.Ids.Count == 0)
            return [];
        // Retrieve or Recompute given field for given records if needed
        EnsureCacheModelIsPresent(field.FinalModel, records.Ids);
        // If we need to retrieve / recompute it, do it
        List<int> idsToRetrieve = [];
        // List<int> idsToCompute = [];
        foreach (var id in records.Ids)
        {
            var cachedField = CachedModels[field.FinalModel.Name][id].Fields[field.FieldName];
            if (cachedField.ToRetrieve)
            {
                idsToRetrieve.Add(id);
            }
            //
            // if (recompute && IsToRecompute(field, id))
            // {
            //     idsToCompute.Add(id);
            // }
        }

        if (idsToRetrieve.Count != 0)
        {
            // TODO Retrieve those fields
            throw new NotSupportedException("It's not possible to retrieve fields from the database as we don't support it right now");
        }

        Environment.ComputeField(field);
        // If there is a target, compute it
        if (field.TargetField != null)
            Environment.ComputeField(field.TargetFinalField!);
        //
        // if (idsToCompute.Count != 0)
        // {
        //     // Compute those fields
        //     ComputeFields(field, idsToCompute);
        // }

        return records.Ids.Select(id => CachedModels[field.FinalModel.Name][id].GetField(field)).ToList();
    }

    /**
     * Compute given field for given ids
     */
    public void ComputeFields(FinalField field, List<int> ids)
    {
        // We don't need to call the computed method on ids that do not need to be recomputed
        ids = ids.Where(id => IsToRecompute(field, id)).ToList();
        // Remove them from the compute cache to avoid having infinite loops
        RemoveAsRecompute(field, ids);
        
        if (field.DefaultComputedMethod == null || field.DefaultComputedMethod.MethodInfo == null)
            return;
        if (field.LastOccurenceOfComputedMethod == null)
            throw new InvalidOperationException($"Cannot find last occurence of computed method for field {field.FieldName} in model {field.FinalModel.Name}");
        object? objInstance = Activator.CreateInstance(field.LastOccurenceOfComputedMethod.PluginModel.Type);
        if (objInstance is not Model model)
            throw new InvalidOperationException($"Created instance of type {field.LastOccurence.Type} is not a Model! This should not occur");
        
        model.Env = Environment;
        model.ModelName = field.FinalModel.Name;
        model.Ids = ids;
        field.DefaultComputedMethod.MethodInfo.Invoke(model, null);
    }

    /**
     * Modify given field of given model to given value
     */
    public void Set(Model records, FinalField field, object? newValue, bool create = false)
    {
        Modified(records, field, create);
        EnsureCacheModelIsPresent(field.FinalModel, records.Ids);
        foreach (var id in records.Ids)
        {
            UpdateField(field, id, newValue);
        }
        Modified(records, field, create);
        
        // Remove ToRecompute flag again as it's added in the second Modified(..) call
        RemoveAsRecompute(field, records.Ids);
    }

    /**
     * Update corresponding field to given value for given record.
     * This method ensure links between M2O & O2M, M2M & M2M are correct.
     */
    private void UpdateField(FinalField field, int recordId, object? newValue)
    {
        if (newValue is Model m)
        {
            if (field.FieldType is FieldType.ManyToOne)
                newValue = m.Ids.Count == 0 ? null : m.Id;
            else
                newValue = m.Ids;
        }
        object? oldValue = CachedModels[field.FinalModel.Name][recordId].Fields[field.FieldName].Value;
        // Remove ToRecompute flag
        RemoveAsRecompute(field, [recordId]);
        if (!CachedModels[field.FinalModel.Name][recordId].UpdateField(field, newValue))
            return;
        
        // No need to update the target field if ManyToOne doesn't have any target
        if (field.TargetField == null || field.TargetFinalField == null || field.TargetFinalModel == null)
            return;
        var cachedModels = CachedModels[field.TargetFinalModel.Name];
        if (field.FieldType is FieldType.ManyToOne)
        {
            if (oldValue != null)
            {
                if (oldValue is not int id)
                    throw new InvalidOperationException($"Value from cache for field {field.FieldName} should be an int but is {oldValue}");
                // No need to update old value if the record is not already loaded in cache
                if (cachedModels.ContainsKey(id))
                {
                    var cachedField = cachedModels[id].Fields[field.TargetField];
                    object? oldCachedValue = cachedField.Value;
                    if (oldCachedValue is not List<int> lst)
                        throw new InvalidOperationException($"Retrieved value for field {field.FieldName} should be a list of int but is {oldCachedValue}");
                    // Update the field by removing the ID from the list
                    List<int> newList = [..lst];
                    newList.Remove(recordId);
                    cachedModels[id].UpdateField(field.TargetFinalField, newList);
                }
            }
            // Now, do the same for the new value
            if (newValue != null)
            {
                int id = newValue switch
                {
                    Model model => model.Id,
                    int i => i,
                    _ => throw new InvalidOperationException($"Value from cache for field {field.FieldName} should be an int but is {newValue}")
                };
                // No need to update old value if the record is not already loaded in cache
                if (cachedModels.ContainsKey(id))
                {
                    var cachedField = cachedModels[id].Fields[field.TargetField];
                    object? oldCachedValue = cachedField.Value;
                    if (oldCachedValue is not List<int> lst)
                        throw new InvalidOperationException($"Retrieved value for field {field.FieldName} should be a list of int but is {oldCachedValue}");
                    // Update the field by removing the ID from the list
                    List<int> newList = [..lst, recordId];
                    cachedModels[id].UpdateField(field.TargetFinalField, newList);
                }
            }
        }
        else if (field.FieldType is FieldType.OneToMany or FieldType.ManyToMany)
        {
            List<int> added = [];
            List<int> removed = [];
            // We can't have both oldValue and newValue null
            if (oldValue is List<int> { Count: 0 })
                added = [..(newValue as List<int>)!];
            else if (newValue is List<int> { Count: 0 })
                removed = [..(oldValue as List<int>)!];
            else if (oldValue is List<int> oldValueLst && newValue is List<int> newValueLst)
            {
                removed = oldValueLst.Except(newValueLst).ToList();
                added = newValueLst.Except(oldValueLst).ToList();
            }

            foreach (var id in removed)
            {
                // If it's a ManyToOne, force it as we need to save it later in db
                if (field.TargetFinalField.FieldType is FieldType.ManyToOne)
                {
                    EnsureCacheModelIsPresent(field.TargetFinalModel, [id]);
                }
                if (cachedModels.ContainsKey(id))
                {
                    var cachedField = cachedModels[id].Fields[field.TargetField];
                    object? oldCachedValue = cachedField.Value;
                    if (field.TargetFinalField.FieldType is FieldType.ManyToOne)
                    {
                        if (oldCachedValue is int i && i == recordId)
                            cachedModels[id].UpdateField(field.TargetFinalField, null);
                    }
                    else if (field.TargetFinalField.FieldType is FieldType.ManyToMany)
                    {
                        if (oldCachedValue is not List<int> lst)
                            throw new InvalidOperationException($"Retrieved value for field {field.FieldName} should be a list of int but is {oldCachedValue}");
                        // Update the field by removing the ID from the list
                        List<int> newList = [..lst];
                        newList.Remove(recordId);
                        cachedModels[id].UpdateField(field.TargetFinalField, newList);
                    }
                }
            }

            foreach (var id in added)
            {
                // If it's a ManyToOne, force it as we need to save it later in db
                if (field.TargetFinalField.FieldType is FieldType.ManyToOne)
                {
                    EnsureCacheModelIsPresent(field.TargetFinalModel, [id]);
                }
                if (cachedModels.ContainsKey(id))
                {
                    var cachedField = cachedModels[id].Fields[field.TargetField];
                    object? oldCachedValue = cachedField.Value;
                    if (field.TargetFinalField.FieldType is FieldType.ManyToOne)
                    {
                        cachedModels[id].UpdateField(field.TargetFinalField, recordId);
                    }
                    else if (field.TargetFinalField.FieldType is FieldType.ManyToMany)
                    {
                        if (oldCachedValue is not List<int> lst)
                            throw new InvalidOperationException($"Retrieved value for field {field.FieldName} should be a list of int but is {oldCachedValue}");
                        // Update the field by removing the ID from the list
                        List<int> newList = [..lst, recordId];
                        cachedModels[id].UpdateField(field.TargetFinalField, newList);
                    }
                }
            }
        }
    }

    /**
     * Set given field as modified for given records.
     */
    private void Modified(Model records, FinalField field, bool create)
    {
        foreach (var (root, recordsToFlag) in _Modified(records, field, create))
        {
            AddAsRecompute(root, recordsToFlag.Ids);
        }
    }

    private IEnumerable<(FinalField root, Model records)> _Modified(Model records, FinalField field, bool create)
    {
        return ModifiedTree(records, field.TreeDependency, create);
    }

    /**
     * Return stream with all fields and models that need to be recomputed
     */
    private IEnumerable<(FinalField root, Model records)> ModifiedTree(Model records, TreeDependency tree, bool create)
    {
        yield return (tree.Root, records);
        foreach (var (_, node) in tree.Items)
        {
            FinalField field = node.Field;
            // If it's on the same model, we have access to the field
            if (node.IsSameModel)
            {
                // yield return (field, records);
                foreach (var (root2, records2) in ModifiedTree(records, field.TreeDependency, create))
                {
                    yield return (root2, records2);
                }
            }
            Model? model = null;
            // If there is an inverse, use it
            if (field.TargetField != null)
            {
                var msg = $"Field {field.FinalModel.Name}.{field.FieldName} is targeting {field.TargetType.Name}.{field.TargetField}";
                FinalField targetField;
                if (node.IsSameModel)
                    targetField = field;
                else
                    targetField = field.TargetFinalField ?? throw new InvalidOperationException($"{msg}, but we can't find this target field. This should not occur.");
                if (targetField.FinalModel != tree.Root.FinalModel)
                    throw new InvalidOperationException($"{msg}, but should target model {tree.Root.FinalModel.Name}");
                
                // If we already need to compute this field, we don't need to compute it and then mark them again as dirty
                // So, only take models that do not already need to be recomputed
                Model? result = records.Get<Model>(targetField.FieldName, recompute: false);
                if (result == null)
                    throw new NullReferenceException("result is null but should not be null");
                // Remove ids that already need to be recomputed
                List<int> ids = result.Ids.Where(id => !IsToRecompute(targetField, id)).ToList();
                if (ids.Count == 0)
                    continue;
                // Here, both fields need to be in ToRecompute=true
                model = records.Env.Get(ids, field.FinalModel.Name);
                
                foreach (var (root2, records2) in ModifiedTree(records, targetField.TreeDependency, create))
                {
                    yield return (root2, records2);
                }
            }
            // If there is no inverse, we don't need to run it if it's the same model
            else if (!node.IsSameModel)
            {
                // If we are in the create method, there is no models referring to this specific record
                if (create)
                    yield break;
                // TODO Search
                // No inverse, we need to perform a search on it.
                // Performing a search on the field will compute this field if it's a computed field
                throw new NotImplementedException("Not implemented right now");
                // model = field.FinalModel.Search(records.Env, [(fieldName, "in", records.Ids)]);
            }

            if (model == null)
                continue;
            foreach (var (root2, records2) in ModifiedTree(model, field.TreeDependency, create))
            {
                yield return (root2, records2);
            }
        }
    }

    /**
     * Ensure given ids are in the _cachedModels field.
     * Do not retrieve any information from the database.
     */
    public void EnsureCacheModelIsPresent(FinalModel finalModel, List<int> ids, bool create = false)
    {
        foreach (var id in ids)
        {
            if (CachedModels[finalModel.Name].ContainsKey(id))
                continue;
            CachedModel cachedModel = new CachedModel
            {
                Env = Environment,
                Model = finalModel,
                Id = id,
            };
            var fields = finalModel.Fields.ToDictionary(
                f => f.Key,
                f =>
                {
                    object? defaultValue = null;
                    if (f.Value.FieldType is FieldType.OneToMany or FieldType.ManyToMany)
                        defaultValue = new List<int>();
                    return new CachedField
                    {
                        CachedModel = cachedModel,
                        Field = f.Value,
                        Value = defaultValue,
                        ToRetrieve = !create,
                    };
                }
            );
            cachedModel.Fields = fields;
            CachedModels[finalModel.Name].Add(id, cachedModel);
        }

        if (create)
        {
            // We need to compute all computed fields, so mark them
            foreach (var (_, field) in finalModel.Fields)
            {
                if (!field.IsComputed)
                    continue;
                AddAsRecompute(field, ids);
            }
        }
    }

    public void AddAsRecompute(FinalField field, List<int> ids)
    {
        foreach (var id in ids)
        {
            ToRecompute[field.FinalModel.Name][field.FieldName].Add(id);
        }
    }

    public void RemoveAsRecompute(FinalField field, List<int> ids)
    {
        ToRecompute[field.FinalModel.Name][field.FieldName].RemoveWhere(ids.Contains);
    }

    public bool IsToRecompute(FinalField field, int id)
    {
        return ToRecompute[field.FinalModel.Name][field.FieldName].Contains(id);
    }
}
