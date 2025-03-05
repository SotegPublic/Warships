using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class SignInWindow : MonoBehaviour
{
    [field: SerializeField] public TMP_InputField EmailField { get; private set; }
    [field: SerializeField] public TMP_InputField PasswordField { get; private set; }
    [field: SerializeField] public TMP_InputField ResetPasswordEmailField { get; private set; }
    [field: SerializeField] public TMP_Text ErrorText { get; private set; }

    [SerializeField] private Canvas _canvas;
    [SerializeField] private Button _signInButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _resetPasswordButton;
    [SerializeField] private Toggle _rememberMeToggle;
    [SerializeField] private AnimatedButton _passwordVisibilityButton;
    [SerializeField] private Image _passwordVisibilityButtonShowImage;
    [SerializeField] private Image _passwordVisibilityButtonHideImage;
    [SerializeField] private GameObject _recoveryStartPanel;
    [SerializeField] private GameObject _recoveryEndPanel;
    [SerializeField] private Button _recoveryStartPanelBackButton;
    [SerializeField] private Button _recoveryStartPanelResetPasswordButton;
    [SerializeField] private Button _recoveryEndPanelBackButton;
    [SerializeField] private TMP_Text _emailText;

    public Button SignInButton => _signInButton;
    public Button BackButton => _backButton; 
    public Button ExitButton => _exitButton;
    public Button ResetPasswordButton => _resetPasswordButton;
    public Toggle RememberMeToggle => _rememberMeToggle;
    public AnimatedButton PasswordVisibilityButton => _passwordVisibilityButton;
    public Image PasswordVisibilityButtonShowImage => _passwordVisibilityButtonShowImage;
    public Image PasswordVisibilityButtonHideImage => _passwordVisibilityButtonHideImage;
    public Button RecoveryStartPanelBackButton => _recoveryStartPanelBackButton;
    public Button RecoveryStartPanelResetPasswordButton => _recoveryStartPanelResetPasswordButton;
    public Button RecoveryEndPanelBackButton => _recoveryEndPanelBackButton;

    public void SetDefaultWindowElementsParameters()
    {
        ErrorText.enabled = false;
        _passwordVisibilityButtonShowImage.enabled = false;
        PasswordField.contentType  = TMP_InputField.ContentType.Password;
        _recoveryStartPanel.SetActive(false);
        _recoveryEndPanel.SetActive(false);
        SignInButton.interactable = true;
        EmailField.interactable = true;
        RememberMeToggle.isOn = false;
    }

    public void HideStartRecoveryPanel()
    {
        _recoveryStartPanel.SetActive(false);
        ResetPasswordEmailField.text = "";
    }

    public void HideEndRecoveryPanel()
    {
        _recoveryEndPanel.SetActive(false);
    }

    public void OpenWindow()
    {
        _canvas.enabled = true;
    }

    public void CloseWindow()
    {
        _canvas.enabled = false;
    }

    public void OpenRecoveryStartPanel()
    {
        _recoveryStartPanel.SetActive(true);
    }

    public void OpenRecoveryEndPanel()
    {
        _recoveryEndPanel.SetActive(true);
        _recoveryStartPanel.SetActive(false);
        ResetPasswordEmailField.text = "";
        _emailText.text = ResetPasswordEmailField.text;
    }
}
