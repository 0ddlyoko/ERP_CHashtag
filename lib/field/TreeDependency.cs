using System.Diagnostics.CodeAnalysis;

namespace lib.field;

/**
 * A Tree structure representing a dependency between a field "source" and fields "destination".
 * If "source" field is modified, "destination" fields needs to be recomputed.
 * "destination" fields are always Computed fields
 */
[method: SetsRequiredMembers]
public class TreeDependency(FinalField root)
{
    public required FinalField Root = root;
    public readonly Dictionary<string, TreeNode> Items = new();

    public bool IsLeaf => Items.Count == 0;
}

[method: SetsRequiredMembers]
public class TreeNode(FinalField field, bool isSameModel = false)
{
    public required FinalField Field = field;
    public readonly bool IsSameModel = isSameModel;

    protected bool Equals(TreeNode other)
    {
        return Field.Equals(other.Field) && IsSameModel == other.IsSameModel;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TreeNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Field, IsSameModel);
    }
}
