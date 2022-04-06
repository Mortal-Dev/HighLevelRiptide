using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HLRiptide;
using TMPro;

public class StartMenuUI : UI
{
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;

    public override void Initialize()
    {
        startButton.onClick.AddListener(OnStartButtonPress);
    }

    private void OnStartButtonPress()
    {
        string ip = ipInputField.text;
        ushort port = ushort.Parse(portInputField.text);

        NetworkManager.Singleton.StartClient(ip, port);

        UIManager.Singleton.Show<LoadingUI>();
    }
}
