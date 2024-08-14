using lib.database;
using lib.field;
using lib.plugin;
using lib.util;

namespace lib.model;

/**
 * Final Model, representing the concatenation of a specific model implemented in multiple plugins
 */
public class FinalModel
{
    public readonly PluginManager PluginManager;
    public readonly string Name;
    public string SqlTableName => StringUtil.ToSnakeCase(Name);
    public readonly PluginModel FirstOccurence;
    public string Description;
    public readonly List<PluginModel> AllOccurences = [];
    public readonly Dictionary<string, FinalField> Fields;

    public FinalModel(PluginManager pluginManager, PluginModel firstOccurence)
    {
        PluginManager = pluginManager;
        Name = firstOccurence.Name;
        FirstOccurence = firstOccurence;
        Description = firstOccurence.Description ?? Name;
        AllOccurences.Add(firstOccurence);
        Fields = new Dictionary<string, FinalField>();
        AddFields(firstOccurence.Fields);
    }

    public void MergeWith(PluginModel pluginModel)
    {
        AllOccurences.Add(pluginModel);
        if (pluginModel.Description != null)
            Description = pluginModel.Description;
        AddFields(pluginModel.Fields);
    }

    private void AddFields(Dictionary<string, PluginField> fields)
    {
        foreach (var (id, field) in fields)
        {
            if (Fields.TryGetValue(id, out var finalField))
                finalField.MergeWith(field);
            else
                Fields[id] = new FinalField(this, field);
        }
    }

    /**
     * Execute some action once this class is fully loaded, and no more PluginModel will be merged in this model
     */
    public void PostLoading(PluginManager pluginManager)
    {
        CalculateInverseCompute();
        
        // ManyToOne, OneToMany, ManyToMany
        foreach (var (fieldName, field) in Fields)
        {
            if (field.FieldType is not FieldType.OneToMany and not FieldType.ManyToMany)
                continue;
            // Target should exist and be a ManyToOnes
            if (field.TargetField == null)
                throw new InvalidOperationException($"Field {Name}.{fieldName} is an OneToMany or a ManyToMany and should have a Target field");
            // New line will throw if TargetType does not exist
            FinalField? finalField = field.TargetFinalField;
            if (field.FieldType == FieldType.OneToMany)
            {
                if (finalField == null)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} should target a ManyToOne field");
                if (finalField.FieldType != FieldType.ManyToOne)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} should target a ManyToOne field, but is targeting field {finalField.FinalModel.Name}.{finalField.FieldName} which is {finalField.FieldType}");
                // Later, two OneToMany of the same model can target to a single ManyToOne
                // TODO Add support for multiple target
                if (finalField.TargetField != null)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} is targeting {finalField.FinalModel.Name}.{finalField.FieldName} which already has a link to {finalField.TargetFinalModel!.Name}.{finalField.TargetField}");
                if (finalField.TargetFinalModel != this)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} is targeting {finalField.FinalModel.Name}, but target field is targeting another model: {finalField.TargetFinalModel!.Name}");
                finalField.TargetField = fieldName;
            }
            else if (field.FieldType == FieldType.ManyToMany)
            {
                if (finalField == null)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} should target a ManyToMany field");
                if (finalField.FieldType != FieldType.ManyToMany)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} should target a ManyToMany field, but is targeting field {finalField.FinalModel.Name}.{finalField.FieldName} which is {finalField.FieldType}");
                if (finalField.TargetFinalModel != this)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} is targeting {finalField.FinalModel.Name}, but target field is targeting another model: {finalField.TargetFinalModel!.Name}");
                if (finalField.TargetField != fieldName)
                    throw new InvalidOperationException($"Field {Name}.{fieldName} is targeting {finalField.FinalModel.Name}.{finalField.FieldName}, but target field is targeting another field: {finalField.Name}");
            }
        }
    }

    /**
     * Calculate the tree of dependencies for each field present in this model.
     */
    private void CalculateInverseCompute()
    {
        
        foreach (var (fieldName, field) in Fields)
        {
            if (field.DefaultComputedMethod?.ComputedAttribute == null)
                continue;
            foreach (var depend in field.DefaultComputedMethod.ComputedAttribute.Fields)
            {
                FinalModel finalModel = this;
                FinalField finalField = field;
                bool first = true;
                foreach (var dependFieldName in depend.Split('.'))
                {
                    FinalField dependFinalField = finalModel.Fields[dependFieldName];
                    dependFinalField.TreeDependency.Items[$"{finalField.FinalModel.Name}.{finalField.FieldName}"] = new TreeNode(finalField, first);
                    finalField = dependFinalField;
                    if (finalField.TargetFinalModel != null)
                        finalModel = finalField.TargetFinalModel;
                    first = false;
                }
            }
        }
    }

    public Dictionary<string, object> GetDefaultValues(List<string> skipValues)
    {
        var dict = new Dictionary<string, object>();
        
        // Default values
        foreach (var (fieldName, finalField) in Fields)
        {
            if (skipValues.Contains(fieldName))
                continue;
            object? defaultValue = finalField.GetDefaultValue();
            if (defaultValue != null)
                dict[fieldName] = defaultValue;
        }
        
        return dict;
    }

    /**
     * Install given models in database.
     * Do not make link between tables, as it's possible the target table is not already created.
     */
    public async Task InstallModel(Environment env, List<PluginModel> models)
    {
        await DatabaseHelper.CreateTable(env.Connection, Name, Description);
        foreach (var pluginModel in models)
        {
            HashSet<FinalField> fields = [];
            foreach (var (fieldName, field) in pluginModel.Fields)
            {
                if (field.FieldName != "Id" &&
                    field.FieldType is not FieldType.OneToMany and not FieldType.ManyToMany)
                    fields.Add(Fields[fieldName]);
            }

            foreach (var finalField in fields)
            {
                await DatabaseHelper.CreateColumn(
                    connection: env.Connection,
                    tableName: SqlTableName,
                    columnName: finalField.SqlTableName,
                    columnType: DatabaseHelper.FieldTypeToString(finalField.FieldType),
                    required: false
                );
            }
        }
    }

    /**
     * Post install this model in database.
     * Make link between tables, as both columns should exist.
     */
    public async Task PostInstallModel(Environment env, List<PluginModel> models)
    {
        foreach (var pluginModel in models)
        {
            HashSet<FinalField> fields = [];
            foreach (var (fieldName, field) in pluginModel.Fields)
            {
                if (field.FieldName != "Id" &&
                    field.FieldType is not FieldType.OneToMany and not FieldType.ManyToMany)
                    fields.Add(Fields[fieldName]);
            }

            foreach (var finalField in fields)
            {
                string? targetTableName = finalField.TargetFinalModel?.SqlTableName;
                if (targetTableName != null)
                {
                    await DatabaseHelper.MakeRelationBetweenColumns(
                        connection: env.Connection,
                        tableName: SqlTableName,
                        columnName: finalField.SqlTableName,
                        targetTableName: targetTableName,
                        targetColumnName: "id"
                    );
                }
            }
        }
    }

    public override string ToString() => $"FinalModel[name={Name}]";
}
