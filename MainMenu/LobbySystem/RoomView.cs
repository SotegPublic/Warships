using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomView : MonoBehaviour
{
    [SerializeField] private Canvas _roomCanvas;
    [SerializeField] private GameObject _ownerField;
    [SerializeField] private GameObject _opponentField;
    [SerializeField] private GameObject _opponentWaitingField;

    [field: SerializeField] public Button DeclineButton { get; private set; }
    [field: SerializeField] public Button StartGameButton { get; private set; }
    [field: SerializeField] public Button BackButton { get; private set; }
    [field: SerializeField] public Toggle VisibilityToggle { get; private set; }

    private TMP_Text _ownerNameText;
    private TMP_Text _ownerWinRateText;
    private TMP_Text _ownerRankText;
    private TMP_Text _opponentNameText;
    private TMP_Text _opponentWinRateText;
    private TMP_Text _opponentRankText;
    private Player _owner;
    private Player _opponent;

    public Player Opponent => _opponent;
    public Player Owner => _owner;

    private void Start()
    {
        StartInit();
        VisibilityToggle.onValueChanged.AddListener(ChangePrivateStatus);
        StartGameButton.onClick.AddListener(() => SceneManager.LoadScene(2));   //todo переделать на запуск сценлоадера
    }

    private void ChangePrivateStatus(bool isPrivate)
    {
        var privateStatus = isPrivate ? PhotonConstants.PRIVATE : PhotonConstants.FREE;

        var customParameters = PhotonNetwork.CurrentRoom.CustomProperties;

        customParameters[PhotonConstants.PRIVATE_STATUS] = privateStatus;

        PhotonNetwork.CurrentRoom.SetCustomProperties(customParameters);
    }

    public void StartInit()
    {
        InitTexts();
        SetWaitingState();
        VisibilityToggle.isOn = false;
    }

    public void CloseWindow()
    {
        _roomCanvas.enabled = false;
    }

    public void OpenWindow()
    {
        _roomCanvas.enabled = true;
        var privateProperty = PhotonNetwork.CurrentRoom.CustomProperties[PhotonConstants.PRIVATE_STATUS].ToString();

        var isPrivate = privateProperty.Equals(PhotonConstants.PRIVATE) ? true : false;

        if (isPrivate)
        {
            VisibilityToggle.isOn = true;
        }
    }

    public void SetWaitingState()
    {
        _opponentField.SetActive(false);
        _opponentWaitingField.SetActive(true);

        _opponentNameText.text = "";
        _opponentWinRateText.text = "";
        _opponentRankText.text = "";
        _opponent = null;

        StartGameButton.interactable = false;
        DeclineButton.interactable = false;
    }

    public void SetDefaultState()
    {
        _ownerNameText.text = "";
        _ownerWinRateText.text = "";
        _ownerRankText.text = "";
        _owner = null;

        StartGameButton.gameObject.SetActive(true);
        DeclineButton.gameObject.SetActive(true);
        VisibilityToggle.gameObject.SetActive(true);
        StartGameButton.interactable = true;
        DeclineButton.interactable = true;

        SetWaitingState();

        VisibilityToggle.isOn = false;
    }

    public void ActivateMasterClientButtons()
    {
        StartGameButton.interactable = true;
        DeclineButton.interactable = true;
    }

    public void HideMasterClientButtons()
    {
        StartGameButton.gameObject.SetActive(false);
        DeclineButton.gameObject.SetActive(false);
        VisibilityToggle.gameObject.SetActive(false);
    }

    public void InitOwner(string ownerName, string ownerWinRate, string ownerExp, Player owner)
    {
        _ownerNameText.text = ownerName;
        _ownerWinRateText.text = ownerWinRate;
        _ownerRankText.text = ownerExp;

        _owner = owner;
    }
    public void InitOpponent(string opponentName, string opponentWinRate, string opponentExp, Player opponent)
    {
        _opponentNameText.text = opponentName;
        _opponentWinRateText.text = opponentWinRate;
        _opponentRankText.text = opponentExp;

        _opponent = opponent;

        _opponentField.SetActive(true);
        _opponentWaitingField.SetActive(false);
    }

    private (TMP_Text firstTXT, TMP_Text secontdTXT, TMP_Text thirdTXT) GetFieldTexts(GameObject holder)
    {
        var texts = holder.GetComponentsInChildren<TMP_Text>();
        return (texts[0], texts[1], texts[2]);
    }

    private void InitTexts()
    {
        var ownerTexts = GetFieldTexts(_ownerField);
        _ownerNameText = ownerTexts.firstTXT;
        _ownerWinRateText = ownerTexts.secontdTXT;
        _ownerRankText = ownerTexts.thirdTXT;

        var opponentTexts = GetFieldTexts(_opponentField);
        _opponentNameText = opponentTexts.firstTXT;
        _opponentWinRateText = opponentTexts.secontdTXT;
        _opponentRankText = opponentTexts.thirdTXT;
    }

    public void ClearSubscribes()
    {
        VisibilityToggle.onValueChanged.RemoveAllListeners();
        StartGameButton.onClick.RemoveAllListeners();
    }
}
