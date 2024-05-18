using lib.field;
using lib.model;
using lib.plugin;

namespace lib;

public class Environment
{
    public readonly PluginManager PluginManager;
    private readonly Dictionary<string, Dictionary<int, CachedModel>> _cachedModels = new();
    private int _id = 1;

    public Environment(PluginManager pluginManager)
    {
        PluginManager = pluginManager;
        foreach (var finalModel in pluginManager.Models)
        {
            _cachedModels[finalModel.Name] = new Dictionary<int, CachedModel>();
        }
    }

    /**
     * Create a new record of type T
     */
    public T Create<T>(List<Dictionary<string, object?>> data) where T: Model
    {
        if (data.Count == 0)
            throw new InvalidOperationException($"No value given to create records of type {typeof(T)}");
        PluginModel pluginModel = PluginManager.GetPluginModelFromType(typeof(T));
        FinalModel finalModel = PluginManager.GetFinalModel(pluginModel.Name);
        // TODO Replace this
        List<int> ids = [];
        foreach (var d in data)
        {
            var id = _id++;
            GetDefaultCachedModel(id, d, finalModel);
            ids.Add(id);
        }
        return Get<T>(ids, pluginModel);
    }

    /**
     * Retrieves given record from the cache, or from the database if not found
     */
    public T Get<T>(int id) where T : Model => 
        Get<T>([id], PluginManager.GetPluginModelFromType(typeof(T)));

    public T Get<T>(List<int> ids) where T : Model =>
        Get<T>(ids, PluginManager.GetPluginModelFromType(typeof(T)));

    private T Get<T>(List<int> ids, PluginModel pluginModel) where T : Model
    {
        T t = Activator.CreateInstance<T>();
        t.Env = this;
        t.Ids = ids;
        t.ModelName = pluginModel.Name;
        return t;
    }

    public void Save<T>(T model) where T : Model =>
        Save(model, PluginManager.GetPluginModelFromType(model.GetType()));

    private void Save<T>(T model, PluginModel pluginModel) where T : Model
    {
        // Save model in database
    }

    public void Update(string modelName, int id, IReadOnlyDictionary<string, object?> data)
    {
        Update(modelName, [id], data);
    }

    public void Update(string modelName, List<int> ids, IReadOnlyDictionary<string, object?> data)
    {
        UpdateModelToCache(modelName, ids, data);
    }

    /**
     * Create a default cached model based on given data, and save it to cache
     */
    private CachedModel GetDefaultCachedModel(int id, FinalModel finalModel) =>
        GetDefaultCachedModel(id, new Dictionary<string, object?>(), finalModel);

    private CachedModel GetDefaultCachedModel(int id, Dictionary<string, object?> data, FinalModel finalModel)
    {
        var cachedModel = GetCachedModel(finalModel.Name, id);
        // Default values
        Dictionary<string, object> defaultValues = finalModel.GetDefaultValues(id);
        foreach (var (key, value) in data)
        {
            if (value == null)
                defaultValues.Remove(key);
            else
                defaultValues[key] = value;
        }

        var dateTime = DateTimeProvider.Now;
        defaultValues["CreationDate"] = dateTime;
        defaultValues["UpdateDate"] = dateTime;
        // Fields
        foreach (var (fieldName, cachedField) in cachedModel.Fields)
        {
            cachedField.Value = defaultValues!.GetValueOrDefault(fieldName, null);
            cachedField.ToRetrieve = false;
        }
        // Once inserted, flag computed fields as ToRecompute
        cachedModel.FlagComputedValues();
        return cachedModel;
    }

    /**
     * Ensure that the given model has a CachedModel for each id
     */
    private void EnsureCachedModelIsPresent(string modelName, List<int> ids)
    {
        foreach (var id in ids)
        {
            if (!_cachedModels[modelName].ContainsKey(id))
            {
                CachedModel cachedModel = new CachedModel
                {
                    Env = this,
                    Id = id,
                    Model = PluginManager.GetFinalModel(modelName),
                    Dirty = false,
                };
                Dictionary<string, CachedField> fields = PluginManager.GetFinalModel(modelName).Fields.ToDictionary(
                    f => f.Key,
                    f => new CachedField
                    {
                        Env = this,
                        CachedModel = cachedModel,
                        Field = f.Value,
                        Value = null,
                        ToRetrieve = true,
                    });
                cachedModel.Fields = fields;
                _cachedModels[modelName][id] = cachedModel;
            }
        }
    }

    /**
     * Update cache of given ids to given data
     */
    private void UpdateModelToCache(string modelName, List<int> ids, IReadOnlyDictionary<string, object?> data)
    {
        foreach (var id in ids)
        {
            // We assume the model & id already exist in cache
            var cachedModel = GetCachedModel(modelName, id);
            cachedModel.UpdateCacheFromData(data);
            cachedModel.Dirty = true;
        }
    }

    public object? GetField(string modelName, List<int> ids, string fieldName)
    {
        FinalField finalField = PluginManager.GetFinalModel(modelName).Fields[fieldName];
        if (ids.Count != 1 && finalField.FieldType is FieldType.Boolean or FieldType.Date or FieldType.Datetime
                or FieldType.Float or FieldType.Integer or FieldType.String)
            throw new InvalidOperationException($"Cannot unpack: there is more than one record ({ids.Count}");
        EnsureCachedModelIsPresent(modelName, ids);
        if (ids.Count == 1)
            return _cachedModels[modelName][ids[0]].Fields[fieldName].GetRealValue();
        // Not supported right now
        throw new NotSupportedException("Record with multiple ids is not supported in this method right now");
    }

    public CachedModel GetCachedModel(string modelName, int id)
    {
        EnsureCachedModelIsPresent(modelName, [id]);
        return _cachedModels[modelName][id];
    }
}
