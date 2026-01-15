[AttributeUsage(AttributeTargets.Property)]
internal class LabelAttribute(string value) : Attribute {
    
    public string Value { get; } = value;
}