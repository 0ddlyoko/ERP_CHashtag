using lib.field;

namespace lib.model;

/**
 * Model class that will be inherited from models
 */
public class Model
{
    [FieldDefinition(Name = "Id", Description = "Id of the record")]
    public int Id;
    // TODO Add creation date & update date
}
