using System.Reflection;
using lib.field;
using lib.field.attributes;
using lib.plugin;

namespace lib.model;

/**
 * Model defined in a plugin
 */
public class PluginModel
{
    public readonly APlugin Plugin;
    public readonly Type Type;
    public readonly string Name;
    public readonly string? Description;
    public readonly Dictionary<string, PluginField> Fields;

    public PluginModel(APlugin plugin, ModelDefinitionAttribute definition, Type type)
    {
        Type = type;
        Plugin = plugin;
        Name = definition.Name;
        Description = definition.Description;
        Fields = new Dictionary<string, PluginField>();
        var fieldInfos = type.GetFields();
        foreach (var fieldInfo in fieldInfos)
        {
            var fieldDefinition = fieldInfo.GetCustomAttribute<FieldDefinitionAttribute>();
            if (fieldDefinition == null)
                continue;
            if (fieldInfo.Name == "Id")
                continue;
            var pluginField = new PluginField(this, fieldDefinition, fieldInfo);
            Fields[pluginField.FieldName] = pluginField;
        }
    }
    
    // TODO Add environment once implemented
    public T? CreateNewInstance<T>() => (T?) Activator.CreateInstance(Type);
}
