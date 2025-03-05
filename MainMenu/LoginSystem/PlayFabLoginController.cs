using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using PlayFab.ProfilesModels;
using System;
using UnityEngine;

public class PlayFabLoginController
{
    public Action<LoginResult> OnLogin;
    public Action<PlayFabError> OnLoginError;

    private string _email;
    private string _playFabID;
    private LoginResult _loginResult;

    private bool _isRegistrationDone = false;
    private bool _isCounted = false;
    private bool _isMailSent = false;

    private int _wons = 0;
    private int _losses = 0;
    private int _rating = 0;
    private int _experience = 0;
    private int _level = 0;
    private int _playerNumber = 0;

    public void Login (string email, string password)
    {
        _email = email;

        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            TitleId = PlayFabConstants.TITLE_ID,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true, GetUserData = true, GetPlayerStatistics = true }
        },
        result =>
        {
            _loginResult = result;
            _playFabID = result.PlayFabId;

            CheckFlags(result);

            if (!_isRegistrationDone)
            {
                if (!_isCounted)
                {
                    ExecutUpdateParametersFunction();
                }
                else if (!_isMailSent)
                {
                    SetContactEmail();
                }
                else
                {
                    SetProfileLanguage();
                }
            }
            else
            {
                Debug.Log($"Success: {email}");

                OnLogin?.Invoke(_loginResult);
            }
        },
        OnError
        );
    }

    private void CheckFlags(LoginResult result)
    {
        var statistics = result.InfoResultPayload.PlayerStatistics;

        for (int i = 0; i < statistics.Count; i++)
        {
            if (statistics[i].StatisticName == PlayFabConstants.IS_REGISTRATION_DONE)
            {
                if (statistics[i].Value == 1)
                {
                    _isRegistrationDone = true;
                }
            }

            if (statistics[i].StatisticName == PlayFabConstants.IS_COUNTED_PARAMETER)
            {
                if (statistics[i].Value == 1)
                {
                    _isCounted = true;
                }
            }

            if (statistics[i].StatisticName == PlayFabConstants.IS_MAIL_SENT_PARAMETER)
            {
                if (statistics[i].Value == 1)
                {
                    _isMailSent = true;
                }
            }

            if (statistics[i].StatisticName == PlayFabConstants.PLAYER_NUMBER)
            {
                _playerNumber = statistics[i].Value;
            }
        }
    }

    private void ExecutUpdateParametersFunction()
    {

        var request = new ExecuteCloudScriptRequest
        {

            FunctionName = CloudScriptFunctions.SET_START_PARAMETERS,
            FunctionParameter = new
            {
                currentPlayerId = _playFabID
            }
        };

        PlayFabClientAPI.ExecuteCloudScript(request,
            SetAccauntSettings,
            error =>
            {
                PlayFabClientAPI.ForgetAllCredentials();
                OnError(error);
            }
        );
    }

    private void SetAccauntSettings(ExecuteCloudScriptResult result)
    {
        JsonObject jsonResult = (JsonObject)result.FunctionResult;

        if (jsonResult.TryGetValue(PlayFabConstants.PLAYER_RATING, out var ratingValue))
        {
            _rating = Convert.ToInt32(ratingValue);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.WONS, out var wonsCount))
        {
            _wons = Convert.ToInt32(wonsCount);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.LOSSES, out var lossesCount))
        {
            _losses = Convert.ToInt32(lossesCount);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.EXPERIENCE, out var playerExperience))
        {
            _experience = Convert.ToInt32(playerExperience);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.LEVEL, out var playerLevel))
        {
            _level = Convert.ToInt32(playerLevel);
        }

        if (jsonResult.TryGetValue(PlayFabConstants.PLAYER_NUMBER, out var number))
        {
            _playerNumber = Convert.ToInt32(number);
        }

        _loginResult.InfoResultPayload.UserData[PlayFabConstants.AVATAR].Value = PlayFabConstants.DEFAULT_AVATAR_ID;

        var statistics = _loginResult.InfoResultPayload.PlayerStatistics;

        for (int i = 0; i < statistics.Count; i++)
        {
            if (statistics[i].StatisticName == PlayFabConstants.PLAYER_RATING)
            {
                statistics[i].Value = _rating;
            }
            if (statistics[i].StatisticName == PlayFabConstants.WONS)
            {
                statistics[i].Value = _wons;
            }
            if (statistics[i].StatisticName == PlayFabConstants.LOSSES)
            {
                statistics[i].Value = _losses;
            }
            if (statistics[i].StatisticName == PlayFabConstants.EXPERIENCE)
            {
                statistics[i].Value = _experience;
            }
            if (statistics[i].StatisticName == PlayFabConstants.LEVEL)
            {
                statistics[i].Value = _level;
            }
            if (statistics[i].StatisticName == PlayFabConstants.PLAYER_NUMBER)
            {
                statistics[i].Value = _playerNumber;
            }
        }

        SetContactEmail();
    }

    private void SetContactEmail()
    {
        var request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = _email
        };

        PlayFabClientAPI.AddOrUpdateContactEmail(request,
            result =>
            {
                SetProfileLanguage();
                Debug.Log("Была обновлена контактная почта");
            },
            error =>
            {
                PlayFabClientAPI.ForgetAllCredentials();
                OnError(error);
            }
        );
    }

    private void SetProfileLanguage()
    {
        var languageRequest = new SetProfileLanguageRequest
        {
            Language = PlayFabConstants.EN_LENGUAGE // сюда добавить установку языка из выбранного в клиенте
        };

        PlayFabProfilesAPI.SetProfileLanguage(languageRequest,
            result =>
            {
                Debug.Log("Done");
                SetDisplayName();
            },
            error =>
            {
                PlayFabClientAPI.ForgetAllCredentials();
                OnError(error);
            }
        );
    }

    private void SetDisplayName()
    {
        var newPlayerName = "UID#" + _playerNumber;
        var displayNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newPlayerName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            displayNameRequest,
            result =>
            {
                Debug.Log("Обновили имя: " + newPlayerName);

                _loginResult.InfoResultPayload.AccountInfo.TitleInfo.DisplayName = newPlayerName;

                var request = new ExecuteCloudScriptRequest
                {

                    FunctionName = CloudScriptFunctions.SET_REGISTRATION_DONE,
                    FunctionParameter = new
                    {
                        currentPlayerId = _playFabID
                    }
                };

                PlayFabClientAPI.ExecuteCloudScript(request, result => OnLogin?.Invoke(_loginResult),
                    error => 
                    { 
                        PlayFabClientAPI.ForgetAllCredentials();
                        OnError(error);
                    });
            },
            error => 
            { 
                PlayFabClientAPI.ForgetAllCredentials();
                OnError(error);
            }
        );
    }

    private void OnError(PlayFabError error)
    {
        OnLoginError?.Invoke(error);
    }

}
