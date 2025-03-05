using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmailVerificationView : MonoBehaviour
{
    public Action OnResendEmailTimerEnd;
    public Action OnCheckTimerEnd;

    [field: SerializeField] public Button BackButton { get; private set; }
    [field: SerializeField] public Button ConfirmButton { get; private set; }
    [field: SerializeField] public Button SendAgainButton { get; private set; }

    [SerializeField] private Canvas _emailVerificationCanvas;
    [SerializeField] private TMP_Text _mailText;
    [SerializeField] private TMP_Text _infoMessageText;
    [SerializeField] private TMP_Text _acceptMessageText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Image _acceptImage;
    [SerializeField] GameObject _timerPanel;
    [SerializeField] float _resendTimer;
    [SerializeField] float _checkTimer;

    private bool _isWaitToResend;
    private bool _isWaitToCheck;
    private float _resendTimerCurrentValue;
    private float _checkTimerCurrentValue;

    private void Update()
    {
        if(_isWaitToResend)
        {
            _resendTimerCurrentValue += Time.deltaTime;
            if(_resendTimerCurrentValue >= _resendTimer)
            {
                SetTimerValue(0f);
                OnResendEmailTimerEnd?.Invoke();
                _isWaitToResend = false;
            }
            else
            {
                var tmpTimerValue = _resendTimer;
                SetTimerValue(tmpTimerValue - _resendTimerCurrentValue);
            }
        }

        if(_isWaitToCheck)
        {
            _checkTimerCurrentValue += Time.deltaTime;

            if (_checkTimerCurrentValue >= _checkTimer)
            {
                OnCheckTimerEnd?.Invoke();
                _checkTimerCurrentValue = 0f;
            }
        }
    }

    public void StopAllTimers()
    {
        _isWaitToCheck = false;
        _isWaitToResend = false;
    }

    public void RestartResendWaitTimer()
    {
        _resendTimerCurrentValue = 0f;
        _isWaitToResend = true;
    }

    public void SetEmail(string mail)
    {
        _mailText.text = mail;
    }

    public void SetInfoState()
    {
        _infoMessageText.enabled = true;
        _acceptMessageText.enabled = false;
        _acceptImage.enabled = false;
        _timerPanel.SetActive(true);
        ConfirmButton.interactable = false;
    }

    public void SetConfirmedState()
    {
        _infoMessageText.enabled = false;
        _acceptMessageText.enabled = true;
        _acceptImage.enabled = true;
        _timerPanel.SetActive(false);
        ConfirmButton.interactable = true;
    }

    private void SetTimerValue(float value)
    {
        _timerText.text = value.ToString();
    }

    public void OpenWindow()
    {
        _emailVerificationCanvas.enabled = true;
        SetInfoState();
        _isWaitToResend = true;
        _isWaitToCheck = true;
        _resendTimerCurrentValue = 0;
        _checkTimerCurrentValue = 0;
        SendAgainButton.interactable = false;
    }

    public void CloseWindow()
    {
        _emailVerificationCanvas.enabled = false;
        _isWaitToCheck = false;
        _isWaitToResend = false;
    }
}
