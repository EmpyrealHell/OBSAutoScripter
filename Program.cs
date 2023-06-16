using Newtonsoft.Json;
using OBSAutoScripter.Model;
using OBSWebsocketDotNet;

namespace OBSAutoScripter
{
    internal class Program
    {
        private static readonly string credentialsFile = "credentials.json";

        private static Credentials LoadCredentials()
        {
            if (!File.Exists(credentialsFile))
            {
                File.WriteAllText(credentialsFile, JsonConvert.SerializeObject(new Credentials()));
                Error($"Credentials file ({credentialsFile}) not found. A default version of this file has been created for you. Please update this file with your OBS data.");
            }
            var credentials = JsonConvert.DeserializeObject<Credentials>(credentialsFile);
            if (string.IsNullOrWhiteSpace(credentials?.Url) || string.IsNullOrWhiteSpace(credentials?.Password))
            {
                File.WriteAllText(credentialsFile, JsonConvert.SerializeObject(new Credentials()));
                Error($"Credentials file ({credentialsFile}) data is invalid. This file has been updated with the correct format. Please update this file with your OBS data.");
            }
            return credentials;
        }

        private static OBSWebsocket Connect(Credentials credentials)
        {
            var socket = new OBSWebsocket();
            var connected = false;
            socket.Connected += (s, e) =>
            {
                Console.WriteLine("Connected!");
                connected = true;
            };
            socket.Disconnected += (s, e) =>
            {
                Error("Connection failed! Please confirm OBS is open, your OBS version is >= 28.0.0, and your credentials are correct.");
            };
            Console.WriteLine($"Connecting to {credentials.Url}");
            socket.ConnectAsync(credentials.Url, credentials.Password);
            while (!connected)
            {
                Thread.Sleep(100);
            }
            return socket;
        }

        private static Script LoadScript(string file, OBSWebsocket socket)
        {
            if (!File.Exists(file))
            {
                Error($"Script file {file} not found.");
            }
            Console.WriteLine($"Loading {file}");
            return Script.Load(file, socket);
        }

        private static void Error(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        static void Main(string[] args)
        {
            var credentials = LoadCredentials();
            var socket = Connect(credentials);
            var script = LoadScript(args.FirstOrDefault() ?? "script.json", socket);
            var sequence = script.GetSequenceToExecute(socket);
            if (sequence == null)
            {
                Error("No sequence conditions met.");
            }
            Console.WriteLine($"Executing sequence {sequence.Name}");
            foreach (var step in sequence.Steps)
            {
                Console.WriteLine($"Executing step {step.ToString()}");
                if (!step.Execute(socket))
                {
                    Error("Step execution failed.");
                }
            }
            Console.WriteLine("Sequence complete!");
        }
    }
}