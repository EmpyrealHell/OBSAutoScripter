using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;

namespace OBSAutoScripter.Model
{
    internal enum StepAction
    {
        None,
        Wait,
        EnableSource,
        DisableSource,
        ChangeScene,
        CopyPosition,
        CopySize,
        CopyRotation,
        CopyCrop,
        SetPosition,
        SetSize,
        SetRotation,
        SetCrop,
    }

    internal class Step
    {
        private static Dictionary<StepAction, Action<OBSWebsocket, Step>> _actionFuncs = new Dictionary<StepAction, Action<OBSWebsocket, Step>>()
        {
            { StepAction.None, (o, s) => { } },
            { StepAction.Wait, (o, s) => { Thread.Sleep(s.Duration); } },
            { StepAction.EnableSource, (o, s) => { o.SetSceneItemEnabled(s.Target.SceneName, s.Target.ItemId, true); } },
            { StepAction.DisableSource, (o, s) => { o.SetSceneItemEnabled(s.Target.SceneName, s.Target.ItemId, false); } },
            { StepAction.ChangeScene, (o, s) => { o.SetCurrentProgramScene(s.Target.SceneName); } },
            { StepAction.CopyPosition, (o, s) =>
                {
                    var transform = o.GetSceneItemTransform(s.Source.SceneName, s.Source.ItemId);
                    var json = JObject.Parse($"{{ positionX: {transform.X + s.OffsetX}, positionY: {transform.Y + s.OffsetY} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.CopySize, (o, s) =>
                {
                    var sourceTransform = o.GetSceneItemTransform(s.Source.SceneName, s.Source.ItemId);
                    var targetTransform = o.GetSceneItemTransform(s.Target.SceneName, s.Target.ItemId);
                    var coefWidth = sourceTransform.SourceWidth / targetTransform.SourceWidth;
                    var coefHeight = sourceTransform.SourceHeight / targetTransform.SourceHeight;
                    var json = JObject.Parse($"{{ scaleX: {sourceTransform.ScaleX * coefWidth}, scaleY: {sourceTransform.ScaleY * coefHeight} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.CopyRotation, (o, s) =>
                {
                    var transform = o.GetSceneItemTransform(s.Source.SceneName, s.Source.ItemId);
                    var json = JObject.Parse($"{{ rotation: {transform.Rotation} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.CopyCrop, (o, s) =>
                {
                    var transform = o.GetSceneItemTransform(s.Source.SceneName, s.Source.ItemId);
                    var json = JObject.Parse($"{{ cropLeft: {transform.CropLeft}, cropTop: {transform.CropTop}, cropRight: {transform.CropRight}, cropBottom: {transform.CropBottom} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.SetPosition, (o, s) =>
                {
                    var json = JObject.Parse($"{{ positionX: {s.X}, positionY: {s.Y} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.SetSize, (o, s) =>
                {
                    var transform = o.GetSceneItemTransform(s.Target.SceneName, s.Target.ItemId);
                    var json = JObject.Parse($"{{ scaleX: {transform.Width / s.Width}, scaleY: {transform.Height / s.Height} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.SetRotation, (o, s) =>
                {
                    var json = JObject.Parse($"{{ rotation: {s.Angle} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
            { StepAction.SetCrop, (o, s) =>
                {
                    var transform = o.GetSceneItemTransform(s.Source.SceneName, s.Source.ItemId);
                    var json = JObject.Parse($"{{ cropLeft: {s.X}, cropTop: {s.Y}, cropRight: {s.X + s.Width}, cropBottom: {s.Y + s.Height} }}");
                    o.SetSceneItemTransform(s.Target.SceneName, s.Target.ItemId, json);
                }
            },
        };

        public StepAction Action { get; set; } = StepAction.None;
        [JsonProperty("Source")]
        public string SourceName { get; set; } = string.Empty;
        [JsonIgnore]
        public Key Source { get; set; } = new Key();
        [JsonProperty("Target")]
        public string TargetName { get; set; } = string.Empty;
        [JsonIgnore]
        public Key Target { get; set; } = new Key();
        public int Duration { get; set; } = -1;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
        public int OffsetX { get; set; } = -1;
        public int OffsetY { get; set; } = -1;
        public int Width { get; set; } = -1;
        public int Height { get; set; } = -1;
        public int Angle { get; set; } = -1;

        public bool Execute(OBSWebsocket socket)
        {
            if (_actionFuncs.TryGetValue(Action, out var funcValue))
            {
                try
                {
                    funcValue(socket, this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error encountered while executing sequence step.");
                    Console.WriteLine(ToString());
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"Invalid step action {Action}.");
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            var output = $"Action: {Action}, Target: {Target?.Name}";
            if (Action == StepAction.Wait)
            {
                output += $", Duration: {Duration}";
            }
            else if (Action == StepAction.CopyPosition)
            {
                output += $", Source: {Source?.Name}, OffsetX: {OffsetX}, OffsetY: {OffsetY}";
            }
            else if (Action == StepAction.CopySize
                || Action == StepAction.CopyRotation
                || Action == StepAction.CopyCrop)
            {
                output += $", Source: {Source?.Name}";
            }
            else if (Action == StepAction.SetPosition)
            {
                output += $", X: {X}, Y: {Y}";
            }
            else if (Action == StepAction.SetSize)
            {
                output += $", Width: {Width}, Height: {Height}";
            }
            else if (Action == StepAction.SetRotation)
            {
                output += $", Angle: {Angle}";
            }
            else if (Action == StepAction.SetCrop)
            {
                output += $", X: {X}, Y: {Y}, Width: {Width}, Height: {Height}";
            }
            return output;
        }
    }
}
