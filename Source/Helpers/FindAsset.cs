[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FindAssetAttribute(string typeName) : Attribute {
    public string TypeName { get; } = typeName;
}
