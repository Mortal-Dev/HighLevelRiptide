using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton;

    [SerializeField] private UI startingUI;

    [SerializeField] private UI[] UIs;

    private UI currentUI;

    private readonly Stack<UI> pastUIs = new Stack<UI>();

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    private void Start()
    {
        foreach (UI ui in UIs)
        {
            ui.Initialize();
            ui.Hide();
        }

        startingUI.Show();
    }

    public T GetView<T>() where T : UI
    {
        foreach (UI ui in UIs)
        {
            if (ui is T tUI) return tUI;
        }

        return null;
    }

    public void Show<T>(bool putInPastUIs = true) where T : UI
    {
        foreach (UI ui in UIs)
        {
            if (currentUI != null)
            {
                if (putInPastUIs) pastUIs.Push(currentUI);

                currentUI.Hide();
            }

            ui.Show();

            currentUI = ui;
        }
    }

    public void Show(UI ui, bool putInPastUIs = true)
    {
        if (currentUI != null)
        {
            if (putInPastUIs) pastUIs.Push(currentUI);

            currentUI.Hide();
        }

        ui.Show();

        currentUI = ui;
    }
    
    public void ShowLast()
    {
        if (pastUIs.Count != 0)
        {
            Show(pastUIs.Pop(), false);
        }
    }
}