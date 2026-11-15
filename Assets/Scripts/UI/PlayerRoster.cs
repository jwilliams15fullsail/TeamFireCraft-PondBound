using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

// The multiplayer roster on the HUD: a card for every frog in the game, with its picture,
// name, health / hunger / hydration bars and how far away it is, plus "Bob joined the pond"
// and "Bob left the pond" messages. Purely local, like PlayerHUD: it reads the FrogIdentity and
// FrogStats values the host already shares with everyone, so it sends nothing over the network.
// Put it on the HUD canvas.
public class PlayerRoster : MonoBehaviour
{
    [Tooltip("The panel that holds the cards (with a Vertical Layout Group).")]
    [SerializeField] GameObject panel;
    [Tooltip("A card inside the panel. It's copied for each player and kept switched off as the template.")]
    [SerializeField] PlayerRosterCard cardTemplate;
    [Tooltip("Hide the panel while yours is the only frog (offline, or waiting for friends).")]
    [SerializeField] bool hideWhenAlone = true;
    [Tooltip("Show a card for your own frog too, first in the list.")]
    [SerializeField] bool showYourself = true;

    [Header("Pictures")]
    [Tooltip("Everyone's picture, tinted with their frog's colour. A light picture tints best.")]
    [SerializeField] Sprite portrait;
    [Tooltip("Optional: a picture of its own for each player slot (Player 1 first), shown untinted. Empty entries use Portrait.")]
    [SerializeField] Sprite[] slotPortraits;

    [Header("Cards")]
    [Tooltip("How fast the bars slide to a new value (full bars per second).")]
    [SerializeField] float barSpeed = 1.5f;
    [Tooltip("How see-through a dead frog's card is.")]
    [SerializeField, Range(0f, 1f)] float deadAlpha = 0.4f;

    [Header("Join and leave messages (optional)")]
    [Tooltip("A text OUTSIDE the panel, so it still shows when the panel hides.")]
    [SerializeField] TMP_Text messageText;
    [SerializeField] float messageSeconds = 4f;
    [SerializeField] int maxMessages = 3;
    [Tooltip("How long to wait for a new player's name before announcing them as Player N.")]
    [SerializeField] float nameWait = 2f;

    readonly List<PlayerRosterCard> cards = new();
    readonly List<FrogIdentity> frogs = new();
    readonly Dictionary<int, string> names = new();  // client id -> their name, kept for "left" messages
    readonly Dictionary<int, float> joining = new(); // client id -> when their frog showed up (-1: not yet)
    readonly List<string> messages = new();
    readonly List<float> messageEnds = new();
    NetworkManager manager;
    bool messagesChanged;

    void Start()
    {
        if (cardTemplate != null)
            cardTemplate.gameObject.SetActive(false);
        if (messageText != null)
            messageText.text = "";

        // FishNet tells every player when someone else connects or leaves (Server Manager's
        // Share Ids, on by default).
        manager = InstanceFinder.NetworkManager;
        if (manager != null)
            manager.ClientManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    void OnDestroy()
    {
        if (manager != null && manager.ClientManager != null)
            manager.ClientManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    void OnRemoteConnectionState(RemoteConnectionStateArgs args)
    {
        int id = args.ConnectionId;
        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            // Their frog spawns once they've loaded the level; it's announced then, with its name.
            if (id != manager.ClientManager.Connection.ClientId)
                joining[id] = -1f;
            return;
        }
        joining.Remove(id);
        if (names.TryGetValue(id, out string name))
        {
            Announce($"{name} left the pond");
            names.Remove(id);
        }
    }

    void Update()
    {
        CollectFrogs();
        AnnounceJoins();
        UpdateCards();
        UpdateMessages();
    }

    // Every frog that's in and has a slot: yours first, then by player slot.
    void CollectFrogs()
    {
        frogs.Clear();
        foreach (FrogIdentity frog in FrogIdentity.Active)
        {
            if (frog == null || !frog.IsSpawned || frog.Slot.Value < 0)
                continue;
            frogs.Add(frog);
            names[frog.OwnerId] = frog.DisplayName;
        }
        frogs.Sort(YoursFirst);
    }

    static int YoursFirst(FrogIdentity a, FrogIdentity b)
    {
        if (a.IsOwner != b.IsOwner)
            return a.IsOwner ? -1 : 1;
        return a.Slot.Value.CompareTo(b.Slot.Value);
    }

    // Someone who connected while we were here: announced once their name arrives, or after
    // Name Wait if they never typed one.
    void AnnounceJoins()
    {
        foreach (FrogIdentity frog in frogs)
        {
            int id = frog.OwnerId;
            if (frog.IsOwner || !joining.TryGetValue(id, out float since))
                continue;
            if (since < 0f)
                joining[id] = since = Time.unscaledTime;
            if (!string.IsNullOrEmpty(frog.PlayerName.Value) || Time.unscaledTime - since >= nameWait)
            {
                Announce($"{frog.DisplayName} joined the pond");
                joining.Remove(id);
            }
        }
    }

    void UpdateCards()
    {
        if (panel == null || cardTemplate == null)
            return;
        bool show = frogs.Count >= (hideWhenAlone ? 2 : 1);
        if (panel.activeSelf != show)
            panel.SetActive(show);
        if (!show)
            return;

        FrogIdentity local = frogs.Count > 0 && frogs[0].IsOwner ? frogs[0] : null;
        int used = 0;
        foreach (FrogIdentity frog in frogs)
        {
            if (frog.IsOwner && !showYourself)
                continue;
            PlayerRosterCard card = CardAt(used++);
            Sprite own = slotPortraits != null && slotPortraits.Length > 0 ? slotPortraits[frog.Slot.Value % slotPortraits.Length] : null;
            card.Refresh(frog, local, own != null ? own : portrait, own == null, barSpeed, deadAlpha);
        }
        for (int i = used; i < cards.Count; i++)
        {
            if (cards[i].gameObject.activeSelf)
                cards[i].gameObject.SetActive(false);
        }
    }

    PlayerRosterCard CardAt(int index)
    {
        while (cards.Count <= index)
        {
            PlayerRosterCard copy = Instantiate(cardTemplate, cardTemplate.transform.parent);
            copy.name = $"{cardTemplate.name} {cards.Count + 1}";
            cards.Add(copy);
        }
        PlayerRosterCard card = cards[index];
        if (!card.gameObject.activeSelf)
            card.gameObject.SetActive(true);
        return card;
    }

    void Announce(string text)
    {
        messages.Add(text);
        messageEnds.Add(Time.unscaledTime + messageSeconds);
        while (messages.Count > Mathf.Max(1, maxMessages))
        {
            messages.RemoveAt(0);
            messageEnds.RemoveAt(0);
        }
        messagesChanged = true;
    }

    void UpdateMessages()
    {
        while (messageEnds.Count > 0 && messageEnds[0] <= Time.unscaledTime)
        {
            messages.RemoveAt(0);
            messageEnds.RemoveAt(0);
            messagesChanged = true;
        }
        if (!messagesChanged || messageText == null)
            return;
        messagesChanged = false;
        messageText.text = string.Join("\n", messages);
    }
}