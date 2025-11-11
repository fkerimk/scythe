namespace scythe;

#pragma warning disable CS8981
[AttributeUsage(AttributeTargets.Property)]
internal class label(string value) : Attribute {
    public string value { get; } = value;
}