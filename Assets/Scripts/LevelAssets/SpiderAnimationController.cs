using FishNet.Object;
using UnityEngine;
using UnityEngineInternal;

public class SpiderAnimationController : NetworkBehaviour
{
    [Header("Legacy Clip Names")]
    [SerializeField] private string idleClip = "idle";
    [SerializeField] private string walkClip = "walk";
    [SerializeField] private string runClip = "run";
    [SerializeField] private string tauntClip = "taunt";
    [SerializeField] private string[] attackClips = { "attack1", "attack2" };

    [Header("Playback Speeds")]
    [SerializeField] private float walkSpeed = 1.0f;
    [SerializeField] private float runSpeed = 1.4f;

    private Animation animation;
    private string currentClip = "";
    private string lastBroadcastedClip = "";

    private EnemyAI enemyAI;
    private WaypointMover waypointMover;

    private void Awake()
    {
        animation = GetComponent<Animation>();
        enemyAI = GetComponent<EnemyAI>();
        waypointMover = GetComponent<WaypointMover>();
    }

    private void OnEnable()
    {
        if (enemyAI != null)
        {
            enemyAI.OnIdle += HandleIdle;
            enemyAI.OnWalk += HandleWalk;
            enemyAI.OnRun += HandleRun;
            enemyAI.OnTaunt += HandleTaunt;
            enemyAI.OnAttack += HandleAttack;
        }

        if (waypointMover != null)
        {
            waypointMover.OnIdle += HandleIdle;
            waypointMover.OnWalk += HandleWalk;
        }
    }

    private void OnDisable()
    {
        if (enemyAI != null)
        {
            enemyAI.OnIdle -= HandleIdle;
            enemyAI.OnWalk -= HandleWalk;
            enemyAI.OnRun -= HandleRun;
            enemyAI.OnTaunt -= HandleTaunt;
            enemyAI.OnAttack -= HandleAttack;
        }

        if (waypointMover != null)
        {
            waypointMover.OnIdle -= HandleIdle;
            waypointMover.OnWalk -= HandleWalk;
        }
    }

    // --- EVENT HANDLERS (RUN ON HOST/SERVER) ---
    private void HandleIdle() => RequestPlayClip(idleClip, 0.2f, 1.0f, forceReplay: false);
    private void HandleWalk() => RequestPlayClip(walkClip, 0.2f, walkSpeed, forceReplay: false);
    private void HandleRun() => RequestPlayClip(runClip, 0.2f, runSpeed, forceReplay: false);
    private void HandleTaunt() => RequestPlayClip(tauntClip, 0.15f, 1.0f, forceReplay: true);

    private void HandleAttack()
    {
        if (attackClips.Length == 0) return;

        // Pick the random attack on the Host so every client sees the exact same bite variation
        string chosenAttack = attackClips[Random.Range(0, attackClips.Length)];
        RequestPlayClip(chosenAttack, 0.1f, 1.0f, forceReplay: true);
    }

    // --- NETWORK BROADCAST LOGIC ---
    private void RequestPlayClip(string clipName, float fadeTime, float speed, bool forceReplay)
    {
        if (string.IsNullOrEmpty(clipName)) return;

        // Spam filter: Don't restart looping clips that are already playing
        if (!forceReplay && lastBroadcastedClip == clipName) return;

        // Check if NetworkManager is missing or if this specific spider isn't spawned on the network yet
        bool isNetworkActive = (NetworkManager != null && NetworkObject != null && NetworkObject.IsSpawned);

        if (!isNetworkActive)
        {
            lastBroadcastedClip = clipName;
            PlayClipLocally(clipName, fadeTime, speed);
            return;
        }

        // MULTIPLAYER CLIENT-ONLY: Do nothing, wait for the Host's RPC
        if (!IsServerInitialized) return;

        // MULTIPLAYER HOST/SERVER: Broadcast to all connected clients
        lastBroadcastedClip = clipName;
        ObserversPlayClip(clipName, fadeTime, speed);
    }

    [ObserversRpc(BufferLast = true)]
    private void ObserversPlayClip(string clipName, float fadeTime, float speed)
    {
        PlayClipLocally(clipName, fadeTime, speed);
    }

    // Shared helper method used by both Offline mode and Network RPCs
    private void PlayClipLocally(string clipName, float fadeTime, float speed)
    {
        if (animation == null) return;

        animation.CrossFade(clipName, fadeTime);

        if (animation[clipName] != null)
        {
            animation[clipName].speed = speed;
        }
    }

    //private void HandleIdle() => PlayIdle();
    //private void HandleWalk() => PlayWalk();
    //private void HandleRun() => PlayRun();
    //private void HandleTaunt() => PlayTaunt();
    //private void HandleAttack() => PlayRandomAttack();

    public void PlayIdle(float fadeTime = 0.2f)
    {
        PlayClip(idleClip, fadeTime, 1.0f);
    }

    public void PlayWalk(float fadeTime = 0.2f)
    {
        PlayClip(walkClip, fadeTime, walkSpeed);
    }

    public void PlayRun(float fadeTime = 0.2f)
    {
        PlayClip(runClip, fadeTime, runSpeed);
    }

    public void PlayTaunt(float fadeTime = 0.15f)
    {
        // Force play even if already called recently
        currentClip = tauntClip;
        if (animation != null) animation.CrossFade(tauntClip, fadeTime);
    }

    public void PlayRandomAttack(float fadeTime = 0.1f)
    {
        if (animation == null || attackClips.Length == 0) return;

        string chosenAttack = attackClips[Random.Range(0, attackClips.Length)];
        currentClip = chosenAttack;
        animation.CrossFade(chosenAttack, fadeTime);
    }

    public bool IsPlayingAttack()
    {
        if (animation == null) return false;
        foreach (string clip in attackClips)
        {
            if (animation.IsPlaying(clip)) return true;
        }
        return false;
    }

    // Internal helper to prevent spamming CrossFade on looping animations
    private void PlayClip(string clipName, float fadeTime, float speed)
    {
        if (animation == null || string.IsNullOrEmpty(clipName)) return;
        if (currentClip == clipName && animation.IsPlaying(clipName)) return;

        currentClip = clipName;
        animation.CrossFade(clipName, fadeTime);

        if (animation[clipName] != null)
        {
            animation[clipName].speed = speed;
        }
    }
}