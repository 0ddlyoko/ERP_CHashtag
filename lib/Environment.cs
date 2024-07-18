using lib.cache;
using lib.database;
using lib.field;
using lib.model;
using lib.plugin;

namespace lib;

public class Environment
{
    public readonly PluginManager PluginManager;
    public readonly Cache Cache;
    public readonly DatabaseConnection Connection;
    // TODO Remove next line
    private int _id = 1;

    public Environment(PluginManager pluginManager, DatabaseConnection? connection = null)
    {
        PluginManager = pluginManager;
        Cache = new Cache(this);
        Connection = connection ?? pluginManager.DatabaseConnectionConfig.Open(this);
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
            Dictionary<string, object?> defaultValues = GetDefaultValues(finalModel, d);

            Cache.EnsureCacheModelIsPresent(finalModel, [id], create: true);
            var dateTime = DateTimeProvider.Now;
            var cachedModel = Cache.CachedModels[finalModel.Name][id];
            // cachedModel.Fields["Id"].UpdateField(id);
            cachedModel.Fields["CreationDate"].UpdateField(dateTime);
            cachedModel.Fields["UpdateDate"].UpdateField(dateTime);
            
            Model record = Get<T>([id]);
            foreach (var (fieldName, value) in defaultValues)
            {
                var finalField = finalModel.Fields[fieldName];
                Cache.Set(record, finalField, value, create: true);
            }
            
            ids.Add(id);
        }
        return Get<T>(ids);
    }

    /**
     * Compute default values for given model with given data
     */
    private Dictionary<string, object?> GetDefaultValues(FinalModel model, Dictionary<string, object?> data)
    {
        Dictionary<string, object?> result = new Dictionary<string, object?>(data);

        foreach (var (key, value) in model.GetDefaultValues(result.Keys.ToList()))
        {
            result[key] = value;
        }

        return result;
    }

    /**
     * Retrieves given record from the cache, or from the database if not found
     */
    public T Get<T>(int id) where T : Model => 
        Get<T>([id]);

    public object Get(int id, Type type) =>
        Get([id], type);

    public T Get<T>(List<int> ids) where T : Model =>
        (T) Get(ids, typeof(T));

    public Model Get(List<int> ids, Type type)
    {
        var pluginModel = PluginManager.GetPluginModelFromType(type);
        var t = Activator.CreateInstance(type);
        if (t == null)
            throw new InvalidOperationException($"Cannot create an instance of Model with ids {ids}: null");
        if (t is not Model model)
            throw new InvalidOperationException($"This should not occur");
        model.Env = this;
        model.Ids = ids;
        model.ModelName = pluginModel.Name;
        return model;
    }

    public Model Get(List<int> ids, string modelName)
    {
        var t = Activator.CreateInstance(typeof(Model));
        if (t == null)
            throw new InvalidOperationException($"Cannot create an instance of Model with ids {ids}: null");
        if (t is not Model model)
            throw new InvalidOperationException($"This should not occur");
        model.Env = this;
        model.Ids = ids;
        model.ModelName = modelName;
        return model;
    }

    public void Save<T>(T model) where T : Model =>
        Save(model, PluginManager.GetPluginModelFromType(model.GetType()));

    private void Save<T>(T model, PluginModel pluginModel) where T : Model
    {
        // Save model in database
    }

    /**
     * Retrieve given field for given records.
     * If field is ManyToOne, OneToMany or ManyToMany, the returned value is an instance of a Model and is not null
     * List order is preserved
     */
    public List<object?> RetrieveField(List<int> ids, string modelName, string fieldName, bool recompute = true) => 
        RetrieveField(ids, PluginManager.GetFinalModel(modelName).Fields[fieldName], recompute: recompute);

    /**
     * Retrieve given field for given records.
     * If field is ManyToOne, OneToMany or ManyToMany, the returned value is an instance of a Model and is not null
     * List order is preserved
     */
    public List<object?> RetrieveField(List<int> ids, FinalField field, bool recompute = true) =>
        RetrieveField(Get(ids, field.FinalModel.Name), field, recompute: recompute);

    /**
     * Retrieve given field for given records.
     * If field is ManyToOne, OneToMany or ManyToMany, the returned value is an instance of a Model and is not null
     * List order is preserved
     */
    public List<object?> RetrieveField(Model records, FinalField field, bool recompute = true)
    {
        List<object?> result = Cache.Get(records, field, recompute: recompute);
        if (field.FieldType is FieldType.ManyToOne or FieldType.OneToMany or FieldType.ManyToMany)
        {
            // Convert List<int> to Model instance
            // We know each element is a list of int, or a single int
            result = result.Select(elem =>
            {
                List<int> r = elem switch
                {
                    null => [],
                    List<int> lst => lst,
                    int i => [i],
                    _ => throw new InvalidOperationException($"Elem should either be an int or a list of int")
                };

                return (object?) Get(r, field.FinalModel.Name);
            }).ToList();
        }
        return result;
    }

    public void UpdateFields(string modelName, int id, IReadOnlyDictionary<string, object?> data)
    {
        UpdateFields(modelName, [id], data);
    }

    public void UpdateFields(string modelName, List<int> ids, IReadOnlyDictionary<string, object?> data)
    {
        var finalModel = PluginManager.GetFinalModel(modelName);
        UpdateFields(Get(ids, finalModel.Name), data);
    }

    public void UpdateFields(Model records, IReadOnlyDictionary<string, object?> data)
    {
        var finalModel = PluginManager.GetFinalModel(records.ModelName);
        foreach (var (fieldName, value) in data)
        {
            var finalField = finalModel.Fields[fieldName];
            UpdateField(finalField, records, value);
        }
        UpdateField(finalModel.Fields["UpdateDate"], records, DateTimeProvider.Now);
        // Field updated, remove them from the ToCompute
        foreach (var (fieldName, _) in data)
        {
            var finalField = finalModel.Fields[fieldName];
            Cache.RemoveAsRecompute(finalField, records.Ids);
        }
    }

    private void UpdateField(FinalField field, Model records, object? value)
    {
        Cache.Set(records, field, value);
    }
    
    // Compute methods

    /**
     * Compute given field for all records that need to be computed
     */
    public void ComputeField(FinalField field)
    {
         // Maybe a field is a dependent of himself.
         // Make sure everything is correctly computed.
         // TODO sort correctly the list of ids to avoid this situation
        for (var i = 0; i < 100; i++)
        {
            var idsToCompute = Cache.ToRecompute[field.FinalModel.Name][field.FieldName].ToList();
            if (idsToCompute.Count == 0)
                break;
            Cache.ComputeFields(field, idsToCompute);
        }
    }

    /**
     * Compute given model for all records that need to be computed
     */
    public void ComputeModel(FinalModel model)
    {
        for (var i = 0; i < 100; i++)
        {
            foreach (var (_, field) in model.Fields)
            {
                ComputeField(field);
            }
        }
    }

    /**
     * Compute given field for given records
     */
    public void ComputeField(FinalField field, Model records)
    {
        for (var i = 0; i < 100; i++)
        {
            Cache.ComputeFields(field, records.Ids);
            if (!records.Ids.Any(id => Cache.IsToRecompute(field, id)))
                break;
        }
    }

    /**
     * Compute given model
     */
    public void ComputeRecords(Model records)
    {
        var model = PluginManager.GetFinalModel(records.ModelName);
        for (var i = 0; i < 100; i++)
        {
            foreach (var (_, field) in model.Fields)
            {
                ComputeField(field, records);
            }
        }
    }
}
