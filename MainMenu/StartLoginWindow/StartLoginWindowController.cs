using System;
using UnityEngine;

public class StartLoginWindowController : IViewController
{
    public Action<WindowsTypes> OnOpenWindowByType;
    public Action<SceneType> OnOpenStartScene;

    private StartLoginWindow _view;

    public StartLoginWindowController(Canvas mainCanvas)
    {
        _view = mainCanvas.GetComponentInChildren<StartLoginWindow>();

        _view.LoginButton.onClick.AddListener(OpenSignInWindow);
        _view.CreateAccountButton.onClick.AddListener(OpenCreateAccountWindow);
        _view.ExitButton.onClick.AddListener(OpenStartScene);
    }

    public void UpdateController()
    {

    }

    public void OpenView()
    {
        _view.OpenWindow();
    }

    public void CloseView()
    {
        _view.CloseWindow();
    }

    private void OpenSignInWindow()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.SignIn);
    }

    private void OpenCreateAccountWindow()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.CreateAccount);
    }

    private void OpenStartScene()
    {
        OnOpenStartScene?.Invoke(SceneType.StartScene);
    }

    public void ClearSubscribes()
    {
        _view.LoginButton.onClick.RemoveAllListeners();
        _view.CreateAccountButton.onClick.RemoveAllListeners();
        _view.ExitButton.onClick.RemoveAllListeners();
    }
}
