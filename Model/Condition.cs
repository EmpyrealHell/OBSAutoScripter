using OBSWebsocketDotNet;

namespace OBSAutoScripter.Model
{
    internal enum ConditionFunction
    {
        Always,
        Never,
        And,
        Or,
        Not,
        CurrentScene,
        SourceEnabled,
        SourceDisabled
    }

    internal class Condition
    {
        private static Dictionary<ConditionFunction, Func<OBSWebsocket, Condition, bool>> _evaluators = new Dictionary<ConditionFunction, Func<OBSWebsocket, Condition, bool>>
        {
            { ConditionFunction.Always, (o, t) => true },
            { ConditionFunction.Never, (o, t) => false },
            { ConditionFunction.And, (o, t) => t.SubConditions.All(x => x.Evaluate(o)) },
            { ConditionFunction.Or, (o, t) => t.SubConditions.Any(x => x.Evaluate(o)) },
            { ConditionFunction.Not, (o, t) => !t.SubConditions.Any(x => x.Evaluate(o)) },
            { ConditionFunction.CurrentScene, (o, t) => o.GetCurrentProgramScene().Equals(t.Key.SceneName, StringComparison.OrdinalIgnoreCase) },
            { ConditionFunction.SourceEnabled, (o, t) => o.GetSceneItemEnabled(t.Key.SceneName, t.Key.ItemId) },
            { ConditionFunction.SourceDisabled, (o, t) => !o.GetSceneItemEnabled(t.Key.SceneName, t.Key.ItemId) }
        };

        public ConditionFunction Variable { get; set; } = ConditionFunction.Always;
        public string Value { get; set; } = string.Empty;
        public Key Key { get; set; } = new Key();
        public string Sequence { get; set; } = string.Empty;
        public IEnumerable<Condition> SubConditions { get; set; } = Enumerable.Empty<Condition>();

        public bool Evaluate(OBSWebsocket socket)
        {
            if (_evaluators.TryGetValue(Variable, out var funcValue))
            {
                return funcValue(socket, this);
            }
            return false;
        }
    }
}
