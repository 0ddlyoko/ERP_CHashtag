using System.Reflection;
using lib.field;
using lib.plugin;

namespace lib.model;

/**
 * Model cached per environment
 */
public class CachedModel
{
    public Environment Env;
    public int Id = 0;
    public FinalModel Model;
    public bool Dirty = false;
    // TODO Add a way to know which value have changed
    public Dictionary<string, object> Data = new();
    // Track all created model in order to update their values if one of the existing model is modified & saved
    public Dictionary<Model, PluginModel> CreatedModels = new();

    /**
     * Update the cache based on given model
     */
    public void UpdateCacheFromModel<T>(T model, PluginModel pluginModel, bool updateDirty = true) where T : Model
    {
        if (pluginModel.Fields.Count == 0)
            return;
        var type = model.GetType();
        foreach ((string fieldName, _) in pluginModel.Fields)
        {
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName} in {model}: Cannot retrieve field!");
            object? newValue = fieldInfo.GetValue(model);
            Data.TryGetValue(fieldName, out object? existingValue);
            if (existingValue == newValue)
                continue;
            ModifyField(fieldName, newValue, model);
            if (updateDirty)
                Dirty = true;
        }
    }

    /**
     * Update the cache based on given data
     */
    public void UpdateCacheFromData<T>(T model, IReadOnlyDictionary<string, object?> data, bool updateDirty = true) where T: Model
    {
        var type = model.GetType();
        foreach ((string fieldName, object? newValue) in data)
        {
            var fieldInfo = type.GetField(fieldName);
            if (fieldInfo == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName} in {model}: Cannot retrieve field!");
            Data.TryGetValue(fieldName, out object? existingValue);
            if (existingValue == newValue)
                continue;
            ModifyField(fieldName, newValue, model, skipOriginalModel: false);
            if (updateDirty)
                Dirty = true;
        }
    }

    private void ModifyField(string fieldName, object? newValue, Model originalModel, bool skipOriginalModel = true)
    {
        if (newValue == null)
            Data.Remove(fieldName);
        else
            Data[fieldName] = newValue;
        foreach (var (model, _) in CreatedModels.Where(model => model.Value.Fields.ContainsKey(fieldName) && (!skipOriginalModel || model.Key != originalModel)))
        {
            var type = model.GetType();
            var field = type.GetField(fieldName);
            if (field == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName} in model {originalModel}: Cannot retrieve field!");
            field.SetValue(model, newValue);
        }
        // Now, check if we need to call some computed method
        CheckComputedMethods(fieldName);
    }

    private void CheckComputedMethods(string fieldName)
    {
        string[] fieldsToUpdate = Model.Fields[fieldName].InverseCompute;
        if (fieldsToUpdate.Length == 0)
            return;
        
    }
}
