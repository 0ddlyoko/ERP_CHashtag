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
    public T Create<T>() where T: Model
    {
        PluginModel pluginModel = PluginManager.GetPluginModelFromType(typeof(T));
        FinalModel finalModel = PluginManager.GetFinalModel(pluginModel.Name);
        var id = _id++;
        var cachedModel = GetDefaultCachedModel(id, finalModel);
        InsertToCache(cachedModel);
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
        // Save model in cache and in database
        SaveModelToCache(model, pluginModel);
    }

    /**
     * Create a default cached model based on given data
     */
    private CachedModel GetDefaultCachedModel(int id, FinalModel finalModel)
    {
        Dictionary<string, object> defaultValues = finalModel.GetDefaultValues();
        defaultValues["Id"] = id;
        return new CachedModel
        {
            Id = id,
            Model = finalModel.Name,
            Dirty = false,
            Data = defaultValues,
        };
    }

    /**
     * Save given cached model to cache
     */
    private void InsertToCache(CachedModel cachedModel)
    {
        if (!_cachedModels.ContainsKey(cachedModel.Model))
            _cachedModels[cachedModel.Model] = new Dictionary<int, CachedModel>();
        _cachedModels[cachedModel.Model][cachedModel.Id] = cachedModel;
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
            _cachedModels[pluginModel.Name][model.Id] = cachedModel;
        }
        // Now, update values in cache
        cachedModel.UpdateCacheFromModel(model, pluginModel);
        cachedModel.Dirty = true;
    }
}
