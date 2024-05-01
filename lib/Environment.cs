using System.Reflection;
using lib.field;
using lib.model;
using lib.plugin;

namespace lib;

public class Environment
{
    public readonly PluginManager PluginManager;
    private readonly Dictionary<string, Dictionary<int, CachedModel>> _cachedModels = new();
    private static int _id = 1;

    public Environment(PluginManager pluginManager)
    {
        PluginManager = pluginManager;
    }

    /**
     * Create a new record of type T
     */
    public T Create<T>() where T : Model => Create<T>(new Dictionary<string, object?>());

    public T Create<T>(Dictionary<string, object?> data) where T: Model
    {
        PluginModel pluginModel = PluginManager.GetPluginModelFromType(typeof(T));
        FinalModel finalModel = PluginManager.GetFinalModel(pluginModel.Name);
        var id = _id++;
        GetDefaultCachedModel(id, data, finalModel);
        return Get<T>(id, pluginModel);
    }

    /**
     * Retrieves given record from the cache, or from the database if not found
     */
    public T Get<T>(int id) where T : Model => 
        Get<T>(id, PluginManager.GetPluginModelFromType(typeof(T)));

    private T Get<T>(int id, PluginModel pluginModel) where T : Model
    {
        // Load cache if not found
        if (!_cachedModels.ContainsKey(pluginModel.Name))
            _cachedModels[pluginModel.Name] = new Dictionary<int, CachedModel>();
        if (!_cachedModels[pluginModel.Name].ContainsKey(id))
        {
            // TODO Load from database
            throw new KeyNotFoundException($"Cannot find id {id} from model {pluginModel.Name}");
        }
        T t = Activator.CreateInstance<T>();
        t.Id = id;
        t.Env = this;
        ResetModelToCacheState(t, pluginModel);
        // Next code should throw an error if cache not found
        _cachedModels[pluginModel.Name][t.Id].CreatedModels[t] = pluginModel;
        return t;
    }

    public void Save<T>(T model) where T : Model =>
        Save(model, PluginManager.GetPluginModelFromType(model.GetType()));

    private void Save<T>(T model, PluginModel pluginModel) where T : Model
    {
        // Save model in cache
        SaveModelToCache(model, pluginModel);
        // Save model in database
    }

    public void Update<T>(T model, IReadOnlyDictionary<string, object?> data) where T : Model =>
        Update(model, data, PluginManager.GetPluginModelFromType(model.GetType()));

    private void Update<T>(T model, IReadOnlyDictionary<string, object?> data, PluginModel pluginModel) where T : Model
    {
        // Save model, then update it with new values
        SaveModelToCache(model, pluginModel);
        // Now, update
        UpdateModelToCache(model, data, pluginModel);
        // Save it in database
    }

    /**
     * Create a default cached model based on given data, and save it to cache
     */
    private CachedModel GetDefaultCachedModel(int id, FinalModel finalModel) =>
        GetDefaultCachedModel(id, new Dictionary<string, object?>(), finalModel);

    private CachedModel GetDefaultCachedModel(int id, Dictionary<string, object?> data, FinalModel finalModel)
    {
        Dictionary<string, object> defaultValues = finalModel.GetDefaultValues(id);
        foreach (var (key, value) in data)
        {
            if (value == null)
                defaultValues.Remove(key);
            else
                defaultValues[key] = value;
        }
        CachedModel cachedModel = new CachedModel
        {
            Env = this,
            Id = id,
            Model = finalModel,
            Dirty = false,
            Data = defaultValues,
        };
        InsertToCache(cachedModel);
        // Once inserted, fill computed fields
        finalModel.FillComputedValues(cachedModel);
        return cachedModel;
    }

    /**
     * Save given cached model to cache
     */
    private void InsertToCache(CachedModel cachedModel)
    {
        string modelName = cachedModel.Model.Name;
        if (!_cachedModels.ContainsKey(modelName))
            _cachedModels[modelName] = new Dictionary<int, CachedModel>();
        _cachedModels[modelName][cachedModel.Id] = cachedModel;
    }

    /**
     * Reset given model to the cache state. If cache not found, throw an error
     */
    public void ResetModelToCacheState<T>(T t) where T: Model => 
        ResetModelToCacheState(t, PluginManager.GetPluginModelFromType(t.GetType()));

    private void ResetModelToCacheState<T>(T t, PluginModel pluginModel) where T : Model
    {
        // Next code should throw an error if cache not found
        CachedModel cacheModel = _cachedModels[pluginModel.Name][t.Id];
        t.Id = cacheModel.Id;
        // Fill fields
        foreach ((string fieldName, _) in pluginModel.Fields)
        {
            Type type = t.GetType();
            FieldInfo? fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName}: Cannot retrieve field!");
            fieldInfo.SetValue(t, cacheModel.Data.GetValueOrDefault(fieldName));
        }
    }

    private void SaveModelToCache<T>(T model, PluginModel pluginModel) where T: Model
    {
        if (!_cachedModels.ContainsKey(pluginModel.Name))
            _cachedModels[pluginModel.Name] = new Dictionary<int, CachedModel>();
        if (!_cachedModels[pluginModel.Name].TryGetValue(model.Id, out CachedModel? cachedModel))
        {
            // It looks like cached model is not present in cache. Let's create it
            var finalModel = PluginManager.GetFinalModel(pluginModel.Name);
            cachedModel = GetDefaultCachedModel(model.Id, finalModel);
        }
        // Now, update values in cache
        cachedModel.UpdateCacheFromModel(model, pluginModel);
        cachedModel.Dirty = true;
    }

    private void UpdateModelToCache<T>(T model, IReadOnlyDictionary<string, object?> data, PluginModel pluginModel)
        where T : Model
    {
        // We assume the model & id already exist in cache
        var cachedModel = _cachedModels[pluginModel.Name][model.Id];
        cachedModel.UpdateCacheFromData(model, data);
        cachedModel.Dirty = true;
    }
}
