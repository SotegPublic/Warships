public struct PlayerLoginData
{
    public string PlayerEmail { get; private set; }
    public string PlayerName { get; private set; }
    public string PlayFabID { get; private set; }
    public string PlayerAvatarID { get; private set; }
    public string PlayerRating { get; private set; }
    public string PlayerWinRate { get; private set; }
    public string PlayerExperience { get; private set; }
    public string PlayerLevel { get; private set; }

    public PlayerLoginData(string playerEmail, string playerName, string playFabID, string playerAvatarID, string playerRating,
        string playerWinRate, string playerExperience, string playerLevel)
    {
        PlayerEmail = playerEmail;
        PlayerName = playerName;
        PlayFabID = playFabID;
        PlayerAvatarID = playerAvatarID;
        PlayerRating = playerRating;
        PlayerWinRate = playerWinRate;
        PlayerExperience = playerExperience;
        PlayerLevel = playerLevel;
    }
}