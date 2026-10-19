using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

// One line in the Browse list: game name, player count and a Join button.
public class SessionRow : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text playersText;
    [SerializeField] Button joinButton;

    public void Show(ISessionInfo info, Action<string> join)
    {
        nameText.text = info.Name;
        playersText.text = $"{info.MaxPlayers - info.AvailableSlots}/{info.MaxPlayers}";
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => join(info.Id));
    }
}