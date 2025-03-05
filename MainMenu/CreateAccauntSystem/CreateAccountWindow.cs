using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class CreateAccountWindow : MonoBehaviour
{
    public Action OnEmailInputEnd;

    [SerializeField] private TMP_InputField _passwordField;
    [SerializeField] private TMP_Text _passwordErrorText;
    [SerializeField] private Canvas _createAccountCanvas;
    [SerializeField] private TMP_InputField _emailField;
    [SerializeField] private TMP_InputField _confirmPasswordField;
    [SerializeField] private TMP_Text _confirmPasswordErrorText;
    [SerializeField] private Image _emailCheckImage;
    [SerializeField] private Image _emailAcceptImage;
    [SerializeField] private Image _passwordAcceptImage;
    [SerializeField] private Image _confirmPasswordAcceptImage;
    [SerializeField] private TMP_Text _emailErrorText;
    [SerializeField] private TMP_Text _emailIsBusyText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private AnimatedButton _showPasswordButton;
    [SerializeField] private Image _passwordButtonShowImage;
    [SerializeField] private Image _passwordButtonHideImage;
    [SerializeField] private AnimatedButton _showConfirmPasswordButton;
    [SerializeField] private Image _confirmPasswordButtonShowImage;
    [SerializeField] private Image _confirmPasswordButtonHideImage;


    private float _waitEmailInputTimer;
    private bool _isWhaitMailCorutineInProgress;
    private bool _isWindowShow;

    public bool IsWindowShow => _isWindowShow;
    public Button ConfirmButton => _confirmButton;
    public Button BackButton => _backButton;
    public Button ExitButton => _exitButton;
    public TMP_InputField EmailField => _emailField;
    public TMP_InputField PasswordField => _passwordField;
    public TMP_InputField ConfirmPasswordField => _confirmPasswordField;
    public Image EmailCheckImage => _emailCheckImage;
    public Image EmailAcceptImage => _emailAcceptImage;
    public Image PasswordAcceptImage => _passwordAcceptImage;
    public Image ConfirmPasswordAcceptImage => _confirmPasswordAcceptImage;
    public TMP_Text EmailErrorText => _emailErrorText;
    public TMP_Text EmailIsBusyText => _emailIsBusyText;
    public TMP_Text PasswordErrorText => _passwordErrorText;
    public TMP_Text ConfirmPasswordErrorText => _confirmPasswordErrorText;

    public AnimatedButton ShowPasswordButton => _showPasswordButton;
    public Image PasswordButtonShowImage => _passwordButtonShowImage;
    public Image PasswordButtonHideImage => _passwordButtonHideImage;
    public AnimatedButton ShowConfirmPasswordButton => _showConfirmPasswordButton;
    public Image ConfirmPasswordButtonShowImage => _confirmPasswordButtonShowImage;
    public Image ConfirmPasswordButtonHideImage => _confirmPasswordButtonHideImage;

    private const float INPUT_WAIT_TIME = 5f;

    public void OpenWindow()
    {
        _createAccountCanvas.enabled = true;
        _isWindowShow = true;
        SetDefaultWindowElementsParameters();
    }

    public void SetDefaultWindowElementsParameters()
    {
        _emailAcceptImage.enabled = false;
        _passwordAcceptImage.enabled = false;
        _confirmPasswordAcceptImage.enabled = false;
        _emailCheckImage.enabled = false;
        _passwordButtonShowImage.enabled = false;
        _confirmPasswordButtonShowImage.enabled = false;
        _emailErrorText.enabled = false;
        _emailIsBusyText.enabled = false;
        _passwordErrorText.enabled = false;
        _confirmPasswordErrorText.enabled = false;

        _emailField.text = "";
        _passwordField.text = "";
        _confirmPasswordField.text = "";

        _passwordField.contentType = TMP_InputField.ContentType.Password;
        _confirmPasswordField.contentType = TMP_InputField.ContentType.Password;

        _confirmButton.interactable = false;
    }

    public void CloseWindow()
    {
        _createAccountCanvas.enabled = false;
        _isWindowShow = false;
    }

    public void StartWaitCheckState()
    {
        _waitEmailInputTimer = 0f;
        _emailErrorText.enabled = false;
        _emailIsBusyText.enabled = false;

        if (!_isWhaitMailCorutineInProgress)
        {
            _isWhaitMailCorutineInProgress = true;
            _emailCheckImage.enabled = true;
            StartCoroutine(WhaitEmailInputCoroutine());
        }
    }

    private IEnumerator WhaitEmailInputCoroutine()
    {
        while (_waitEmailInputTimer <= INPUT_WAIT_TIME)
        {
            ContinueWaitInput(ref _waitEmailInputTimer, _emailCheckImage);
            yield return new WaitForEndOfFrame();
        }

        _waitEmailInputTimer = 0f;
        _isWhaitMailCorutineInProgress = false;
        HideWaitImage(_emailCheckImage);
        OnEmailInputEnd?.Invoke();
    }

    private void ContinueWaitInput(ref float waitInputTimer, Image waitImage)
    {
        waitInputTimer += Time.deltaTime;
        waitImage.transform.Rotate(-Vector3.forward * Time.deltaTime * 100);
    }

    private void HideWaitImage(Image image)
    {
        image.transform.rotation = Quaternion.identity;
        image.enabled = false;
    }
}
