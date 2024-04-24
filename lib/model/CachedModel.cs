using System.Reflection;
using lib.field;
using lib.plugin;

namespace lib.model;

/**
 * Model cached per environment
 */
public class CachedModel
{
    public int Id = 0;
    public string Model = "";
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
        foreach ((string fieldName, _) in pluginModel.Fields)
        {
            var type = model.GetType();
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

    private void ModifyField(string fieldName, object? newValue, Model originalModel)
    {
        if (newValue == null)
            Data.Remove(fieldName);
        else
            Data[fieldName] = newValue;
        foreach (var (model, _) in CreatedModels.Where(model => model.Key != originalModel && model.Value.Fields.ContainsKey(fieldName)))
        {
            var type = model.GetType();
            var field = type.GetField(fieldName);
            if (field == null)
                throw new InvalidOperationException($"Cannot fill field {fieldName} in model {originalModel}: Cannot retrieve field!");
            field.SetValue(model, newValue);
        }
    }
}
