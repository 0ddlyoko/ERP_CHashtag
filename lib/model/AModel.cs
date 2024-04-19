namespace lib.model;

public class AModel(ModelDefinitionAttribute definition, Type type)
{

    // TODO Add environment once implemented
    public T? CreateNewInstance<T>() => (T?) Activator.CreateInstance(type);
}
