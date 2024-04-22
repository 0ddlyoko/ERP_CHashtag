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
        Dictionary<string, object> defaultValues = finalModel.GetDefaultValues();
        var id = _id++;
        defaultValues["Id"] = id;
        var cachedModel = new CachedModel
        {
            Id = id,
            Model = pluginModel.Name,
            Dirty = false,
            Data = defaultValues,
        };
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
        ResetModelToCacheState(t, pluginModel);
        return t;
    }

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
        ResetModelToCacheState(t, PluginManager.GetPluginModelFromType(typeof(T)));

    private void ResetModelToCacheState<T>(T t, PluginModel pluginModel) where T : Model
    {
        // Next code should throw an error if cache not found
        CachedModel cacheModel = _cachedModels[pluginModel.Name][t.Id];
        t.Id = cacheModel.Id;
        // Fill fields
        foreach ((string fieldName, PluginField field) in pluginModel.Fields)
        {
            if (fieldName == "Id")
                continue;
            Type type = t.GetType();
            FieldInfo? fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName}: Cannot retrieve field!");
            fieldInfo.SetValue(t, cacheModel.Data.GetValueOrDefault(fieldName));
        }
    }
}
