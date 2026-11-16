using System;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

// One line in the Browse list: game name, player count and a Join button.
// Optional extras: a frog pip per player slot, and a greyed-out FULL look when there's no room.
public class SessionRow : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text playersText;
    [SerializeField] Button joinButton;

    [Header("Optional look")]
    [Tooltip("Small text under the name, e.g. \"Waiting for frogs\" or \"Pond is full\".")]
    [SerializeField] TMP_Text subtitleText;
    [Tooltip("One image per player slot, left to right. Filled ones get Pip Filled, the rest Pip Empty.")]
    [SerializeField] Image[] playerPips = new Image[0];
    [SerializeField] Color pipFilled = new Color32(0x8F, 0xD1, 0x4F, 0xFF);
    [SerializeField] Color pipEmpty = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    [Tooltip("The Join button's text. Says FULL when the game has no room.")]
    [SerializeField] TMP_Text joinLabel;
    [Tooltip("Fades the whole row when the game is full.")]
    [SerializeField] CanvasGroup group;
    [SerializeField, Range(0f, 1f)] float fullAlpha = 0.6f;

    public void Show(ISessionInfo info, Action<string> join)
    {
        int players = info.MaxPlayers - info.AvailableSlots;
        bool full = info.AvailableSlots <= 0;

        nameText.text = info.Name;
        playersText.text = $"{players}/{info.MaxPlayers}";
        if (subtitleText != null)
            subtitleText.text = full ? "Pond is full" : "Waiting for frogs";

        for (int i = 0; i < playerPips.Length; i++)
        {
            if (playerPips[i] == null)
                continue;
            playerPips[i].gameObject.SetActive(i < info.MaxPlayers);
            playerPips[i].color = i < players ? pipFilled : pipEmpty;
        }

        joinButton.interactable = !full;
        if (joinLabel != null)
            joinLabel.text = full ? "FULL" : "JOIN";
        if (group != null)
            group.alpha = full ? fullAlpha : 1f;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => join(info.Id));
    }
}