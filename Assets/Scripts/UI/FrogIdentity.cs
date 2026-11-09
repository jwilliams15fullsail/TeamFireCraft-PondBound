using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

// Who a frog belongs to: the player's name floating above it, and a skin of its own.
// The owner sends the name typed on the main menu (saved as PlayerPrefs "PlayerName") when its
// frog spawns. The host gives each frog the lowest free player slot, and the slot picks the
// skin, so no two frogs in a game look alike and a slot is reused when someone leaves.
// Both are SyncVars, so a player who joins later sees everyone's name and skin.
public class FrogIdentity : NetworkBehaviour
{
    public const string NameKey = "PlayerName";
    public const int MaxNameLength = 16;

    [System.Serializable]
    public struct Look
    {
        [Tooltip("The skin texture. Empty: keeps the material's own.")]
        public Texture skin;
        [Tooltip("Multiplies the skin. White leaves it as it is.")]
        public Color tint;
    }

    [Tooltip("The frog's body: Visual/Squash/FROG1_1/Frog1_mesh.")]
    [SerializeField] Renderer body;
    [Tooltip("One look per player slot, in order: the first player gets the first one.")]
    [SerializeField]
    Look[] looks =
    {
        new Look { tint = Color.white },
        new Look { tint = Color.white },
        new Look { tint = Color.white },
        new Look { tint = new Color(1f, 0.55f, 1f) },
    };

    [Header("Name")]
    [Tooltip("A TextMeshPro (3D) text, a child of the frog. It's kept upright above the frog and faces the camera.")]
    [SerializeField] TMP_Text nameLabel;
    [Tooltip("How high above the frog's centre the name floats (m).")]
    [SerializeField] float labelHeight = 0.6f;
    [Tooltip("Hide the name above your own frog.")]
    [SerializeField] bool hideOwnName = true;

    public readonly SyncVar<string> PlayerName = new("");
    public readonly SyncVar<int> Slot = new(-1);

    // The name to show: what the player typed, or "Player 2" if they left it empty.
    public string DisplayName => string.IsNullOrEmpty(PlayerName.Value) ? $"Player {Slot.Value + 1}" : PlayerName.Value;

    static readonly List<FrogIdentity> spawned = new(); // host: every frog, for free slots
    MaterialPropertyBlock block;
    int shownSlot = -2;

    public override void OnStartServer()
    {
        base.OnStartServer();
        Slot.Value = FreeSlot();
        spawned.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        spawned.Remove(this);
    }

    static int FreeSlot()
    {
        for (int slot = 0; ; slot++)
        {
            bool taken = false;
            foreach (FrogIdentity frog in spawned)
                taken |= frog.Slot.Value == slot;
            if (!taken)
                return slot;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
            return;
        SubmitName(PlayerPrefs.GetString(NameKey, ""));
        if (hideOwnName && nameLabel != null)
            nameLabel.gameObject.SetActive(false);
    }

    [ServerRpc]
    void SubmitName(string wanted)
    {
        wanted = (wanted ?? "").Trim();
        if (wanted.Length > MaxNameLength)
            wanted = wanted.Substring(0, MaxNameLength);
        PlayerName.Value = wanted;
    }

    void LateUpdate()
    {
        if (Slot.Value != shownSlot)
        {
            shownSlot = Slot.Value;
            ApplyLook();
        }

        if (nameLabel == null || !nameLabel.gameObject.activeSelf)
            return;
        nameLabel.text = DisplayName;
        // Straight up from the frog even while it clings to a wall, turned to face the camera.
        nameLabel.transform.position = transform.position + Vector3.up * labelHeight;
        Camera view = Camera.main;
        if (view != null)
            nameLabel.transform.rotation = Quaternion.LookRotation(nameLabel.transform.position - view.transform.position);
    }

    void ApplyLook()
    {
        if (body == null || looks == null || looks.Length == 0 || Slot.Value < 0)
            return;
        Look look = looks[Slot.Value % looks.Length];
        block ??= new MaterialPropertyBlock();
        body.GetPropertyBlock(block);
        if (look.skin != null)
            block.SetTexture("_BaseMap", look.skin);
        block.SetColor("_BaseColor", look.tint.a > 0f ? look.tint : Color.white);
        body.SetPropertyBlock(block);
    }
}