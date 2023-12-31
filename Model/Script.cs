﻿using Newtonsoft.Json;
using OBSWebsocketDotNet;

namespace OBSAutoScripter.Model
{
    internal class Script
    {
        public IEnumerable<Scene> Scenes { get; set; } = Enumerable.Empty<Scene>();
        public Dictionary<string, Key> Keys { get; set; } = new Dictionary<string, Key>();
        public IEnumerable<Sequence> Sequences { get; set; } = Enumerable.Empty<Sequence>();
        public IEnumerable<Condition> Conditions { get; set; } = Enumerable.Empty<Condition>();

        public static Script Load(string path, OBSWebsocket socket)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }
            var content = File.ReadAllText(path);
            var script = JsonConvert.DeserializeObject<Script>(content);
            if (script == null)
            {
                throw new JsonSerializationException($"Failed to parse {path}");
            }
            var keys = new Dictionary<string, Key>();
            var scenes = socket.GetSceneList();
            foreach (var scene in script.Scenes)
            {
                if (!scenes.Scenes.Any(x => x.Name == scene.Name))
                {
                    throw new Exception($"No scene in list matches name {scene.Name}.");
                }
                keys.Add(scene.Key, new Key() { Name = scene.Key, SceneName = scene.Name });
                foreach (var source in scene.Sources)
                {
                    var id = -1;
                    try
                    {
                        id = socket.GetSceneItemId(scene.Name, source.Name, 0);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to fetch scene item id for source {source.Name} in scene {scene.Name}", ex);
                    }
                    keys.Add(source.Key, new Key() { Name = source.Key, SceneName = scene.Name, ItemId = id });
                }
            }
            script.Keys = keys;

            foreach (var condition in script.Conditions)
            {
                if (script.Keys.TryGetValue(condition.Value, out var value))
                {
                    condition.Key = value;
                }
            }

            foreach (var sequence in script.Sequences)
            {
                foreach (var step in sequence.Steps)
                {
                    if (script.Keys.TryGetValue(step.TargetName, out var targetValue))
                    {
                        step.Target = targetValue;
                    }
                    if (script.Keys.TryGetValue(step.SourceName, out var sourceValue))
                    {
                        step.Source = sourceValue;
                    }
                }
            }
            return script;
        }

        public Sequence? GetSequenceToExecute(OBSWebsocket socket)
        {
            if (Conditions == null)
            {
                return Sequences?.FirstOrDefault();
            }
            var met = Conditions.FirstOrDefault(x => x.Evaluate(socket));
            if (met != null)
            {
                return Sequences?.FirstOrDefault(x => x.Name.Equals(met.Sequence));
            }
            return null;
        }
    }
}
