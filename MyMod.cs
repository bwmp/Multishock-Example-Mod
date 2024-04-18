using MelonLoader;
using Multishock.API;
using System;
using UnityEngine;

[assembly: MelonInfo(typeof(Multishock_Example_Mod.MyMod), "Multishock Example Mod", "1.0.0", "N/A")]
[assembly: MelonGame("Landfall Games", "Content Warning")]
namespace Multishock_Example_Mod
{
    public class MyMod : MelonMod
    {
        private MultiShockAPI multiShockAPI;

        public override void OnInitializeMelon()
        {
            multiShockAPI = new MultiShockAPI();

            // Subscribe to log messages and errors
            multiShockAPI.LogMessage += LogMessage;
            multiShockAPI.LogError += LogError;

            // Connect to the WebSocket server
            multiShockAPI.ConnectToServer(new Uri("ws://localhost:8765"));
        }

        public override void OnApplicationQuit()
        {
            // Disconnect from the WebSocket server when the object is destroyed
            multiShockAPI?.DisconnectFromServer();
        }

        private void LogMessage(string message)
        {
            Debug.Log(message);
        }

        private void LogError(string error)
        {
            Debug.LogError(error);
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                multiShockAPI.SendMessageToServer("Hello from MelonLoader mod!");
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                multiShockAPI.SendShock(10, 1, "Content Warning", "Death");
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                multiShockAPI.SendShock(10, 1, "Content Warning", "Death");
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                multiShockAPI.SendShock(10, 1, "Content Warning", "Death");
            }
        }

        private void SendMessageToServer(string message)
        {
            multiShockAPI.SendShock(5, 100, "module", "event");
        }
    }
}
