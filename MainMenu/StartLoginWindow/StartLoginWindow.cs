using UnityEngine;
using UnityEngine.UI;

public class StartLoginWindow : MonoBehaviour
{  
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _createAccountButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Canvas _startLoginWindowCanvas;        

    public Button LoginButton => _loginButton;
    public Button CreateAccountButton => _createAccountButton;
    public Button ExitButton => _exitButton;

    public void OpenWindow()
    {   
        _startLoginWindowCanvas.enabled = true;
    }

    public void CloseWindow()
    {
        _startLoginWindowCanvas.enabled = false;
    }      
}
