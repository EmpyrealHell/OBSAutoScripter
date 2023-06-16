namespace OBSAutoScripter.Model
{
    internal class Sequence
    {
        public string Name { get; set; } = string.Empty;
        public IEnumerable<Step> Steps { get; set; } = Enumerable.Empty<Step>();
    }
}
