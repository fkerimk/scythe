[AttributeUsage(AttributeTargets.Property)]
internal class Label(string value) : Attribute {
    
    public string Value { get; } = value;
}