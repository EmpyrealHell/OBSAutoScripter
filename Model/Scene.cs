namespace OBSAutoScripter.Model
{
    internal class Scene
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IEnumerable<Source> Sources { get; set; } = Enumerable.Empty<Source>();
    }
}
