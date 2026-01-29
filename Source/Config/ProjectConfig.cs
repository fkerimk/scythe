using Newtonsoft.Json;

internal class ProjectConfig {

    [JsonIgnore] public static ProjectConfig Current = new();

    public string Name = "SCYTHE";
}