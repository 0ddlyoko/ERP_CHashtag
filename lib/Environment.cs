using System.Reflection;
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
        t.Env = this;
        t.CachedModel = _cachedModels[pluginModel.Name][id];
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
        UpdateModelToCache(modelName, id, data);
        // Save it in database
    }

    /**
     * Create a default cached model based on given data, and save it to cache
     */
    private CachedModel GetDefaultCachedModel(int id, FinalModel finalModel) =>
        GetDefaultCachedModel(id, new Dictionary<string, object?>(), finalModel);

    private CachedModel GetDefaultCachedModel(int id, Dictionary<string, object?> data, FinalModel finalModel)
    {
        var cachedModel = new CachedModel
        {
            Env = this,
            Id = id,
            Model = finalModel,
            Dirty = false,
        };
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
        Dictionary<string, CachedField> fields = finalModel.Fields.ToDictionary(
            f => f.Key,
            f => new CachedField
            {
                Env = this,
                CachedModel = cachedModel,
                Field = f.Value,
                Value = defaultValues!.GetValueOrDefault(f.Key, null)
            });
        cachedModel.Fields = fields;
        // Save cache
        InsertToCache(cachedModel);
        // Once inserted, flag computed fields as ToRecompute
        finalModel.FlagComputedValues(cachedModel);
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

    private void UpdateModelToCache(string modelName, int id, IReadOnlyDictionary<string, object?> data)
    {
        // We assume the model & id already exist in cache
        var cachedModel = _cachedModels[modelName][id];
        cachedModel.UpdateCacheFromData(data);
        cachedModel.Dirty = true;
    }
}
