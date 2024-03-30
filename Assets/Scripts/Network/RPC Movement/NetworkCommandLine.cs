using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{

    private NetworkManager networkManager;
    // Start is called before the first frame update
    void Start()
    {
        networkManager = GetComponentInParent<NetworkManager>();

        if (Application.isEditor) return;

        var args = GetCommandLineArgs();

        if(args.TryGetValue("-mode", out string mode))
        {
            if(mode == "server")
            {
                networkManager.StartServer();
            } else if (mode == "client")
            {
                networkManager.StartClient();
            } else if (mode == "host")
            {
                networkManager.StartHost();
            }
        }
    }

    private Dictionary<string, string> GetCommandLineArgs()
    {
        Dictionary<string, string> argDictionary = new Dictionary<string, string>();

        var args = System.Environment.GetCommandLineArgs();  

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argDictionary.Add(arg, value);
            }
        }

        return argDictionary;
    }
}
