using Photon.Pun;
using PlayFab;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EmailVerificationWindowController : IViewController
{
    public Action<SceneType> OnOpenStartScene;
    public Action<WindowsTypes> OnOpenWindowByType;

    private EmailVerificationView _emailVerificationView;
    private string _currentPlayFabID;
    private PlayFabServerInstanceAPI _serverAPI;

    public EmailVerificationWindowController(Canvas mainCanvas)
    {
        _emailVerificationView = mainCanvas.GetComponentInChildren<EmailVerificationView>();
        _emailVerificationView.SetInfoState();

        SubscribeUIElements();

        var ApiSettings = new PlayFabApiSettings()
        {
            TitleId = PlayFabConstants.TITLE_ID,
            DeveloperSecretKey = PlayFabConstants.DEVELOPER_SECRET_KEY,
        };

        _serverAPI = new PlayFabServerInstanceAPI(ApiSettings);
    }

    private void SubscribeUIElements()
    {

        _emailVerificationView.BackButton.onClick.AddListener(BackButtonClick);
        _emailVerificationView.ConfirmButton.onClick.AddListener(ConfirmButtonClick);
        _emailVerificationView.SendAgainButton.onClick.AddListener(ResendButtonClick);

        _emailVerificationView.OnResendEmailTimerEnd += ActiveResendButton;
        _emailVerificationView.OnCheckTimerEnd += CheckVerificationStatus;
    }

    private void CheckVerificationStatus()
    {
        var request = new PlayFab.ClientModels.GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { PlayFabConstants.IS_VERIFIED_PARAMETER },
        };

        PlayFabClientAPI.GetPlayerStatistics(
            request,
            CheckVerifedStatus,
            error =>
            {
                Debug.Log($"Fail: {error.ErrorMessage}");

                //ToDo: Сделать вывод сообщения в случае ошибки верификации

                OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
            }
            );
    }

    private void CheckVerifedStatus(PlayFab.ClientModels.GetPlayerStatisticsResult result)
    {
        if(result.Statistics.Count > 0)
        {
            if (result.Statistics[0].Value >= 1)
            {
                _emailVerificationView.SetConfirmedState();
                _emailVerificationView.StopAllTimers();
            }
        }
    }

    private void ResendButtonClick()
    {
        _emailVerificationView.RestartResendWaitTimer();
        _emailVerificationView.SendAgainButton.interactable = false;

        _serverAPI.SendEmailFromTemplate(
            new SendEmailFromTemplateRequest
            {
                EmailTemplateId = PlayFabConstants.VERIFICATION_MAIL_TEMPLATE_ID,
                PlayFabId = _currentPlayFabID
            },
            result => Debug.Log("Email Send"),
            error => Debug.Log("Email Not Send") // todo: сделать вывод сообщения о том что письмо не было отправлено
        );
    }

    private void ActiveResendButton()
    {
        _emailVerificationView.SendAgainButton.interactable = true;
    }

    public void SetCurrentUserParameters(string email, string playFabID)
    {
        _currentPlayFabID = playFabID;
        _emailVerificationView.SetEmail(email);
    }

    private void BackButtonClick()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.StartLogin);
        PhotonNetwork.Disconnect();
    }

    private void ConfirmButtonClick()
    {
        OnOpenWindowByType?.Invoke(WindowsTypes.SimpleCastomization);
    }

    public void UpdateController()
    {

    }

    public void OpenView()
    {
        _emailVerificationView.OpenWindow();
    }

    public void CloseView()
    {
        _emailVerificationView.CloseWindow();
    }

    public void ClearSubscribes()
    {

        _emailVerificationView.BackButton.onClick.RemoveAllListeners();
        _emailVerificationView.ConfirmButton.onClick.RemoveAllListeners();
        _emailVerificationView.SendAgainButton.onClick.RemoveAllListeners();

        _emailVerificationView.OnResendEmailTimerEnd -= ActiveResendButton;
        _emailVerificationView.OnCheckTimerEnd -= CheckVerificationStatus;
    }
}