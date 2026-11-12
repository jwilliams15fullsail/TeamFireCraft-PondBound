using FishNet.Object;
using UnityEngine;

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
    private string lastBroadcastedClip = "";

    private EnemyAI enemyAI;
    private WaypointMover waypointMover;
    private ProceduralLegIK legIK;
    private Coroutine resumeIKRoutine;
    private bool isIKPaused = false;

    private void Awake()
    {
        animation = GetComponent<Animation>();
        enemyAI = GetComponent<EnemyAI>();
        waypointMover = GetComponent<WaypointMover>();
        legIK = GetComponent<ProceduralLegIK>();
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
    private void HandleIdle()
    {
        ResumeIKIfPaused();
        RequestPlayClip(idleClip, 0.2f, 1.0f, forceReplay: false);
    }

    private void HandleWalk()
    {
        ResumeIKIfPaused();
        RequestPlayClip(walkClip, 0.2f, walkSpeed, forceReplay: false);
    }

    private void HandleRun()
    {
        ResumeIKIfPaused();
        RequestPlayClip(runClip, 0.2f, runSpeed, forceReplay: false);
    }

    private void HandleTaunt()
    {
        PauseIKForAction(tauntClip);
        RequestPlayClip(tauntClip, 0.15f, 1.0f, forceReplay: true);
    }

    private void HandleAttack()
    {
        if (attackClips.Length == 0) return;

        string chosenAttack = attackClips[Random.Range(0, attackClips.Length)];
        PauseIKForAction(chosenAttack);
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
        PauseIKForAction(clipName);
        ResumeIKIfPaused();

        animation.CrossFade(clipName, fadeTime);

        if (animation[clipName] != null)
        {
            animation[clipName].speed = speed;
        }
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

    private void PauseIKForAction(string clipName)
    {
        if (legIK == null) return;

        isIKPaused = true;
        if (resumeIKRoutine != null) StopCoroutine(resumeIKRoutine);
        legIK.SetIKActive(false);

        // Automatically re-enable IK once the animation clip finishes playing
        float clipLength = (animation != null && animation[clipName] != null) ? animation[clipName].length : 1.2f;
        resumeIKRoutine = StartCoroutine(AutoResumeIKAfterDelay(clipLength));
    }

    private void ResumeIKIfPaused()
    {
        // Only run once if IK was actually paused!
        if (legIK == null || !isIKPaused) return;

        isIKPaused = false;
        if (resumeIKRoutine != null)
        {
            StopCoroutine(resumeIKRoutine);
            resumeIKRoutine = null;
        }

        legIK.ResetFeetToGround();
        legIK.SetIKActive(true);
    }

    private System.Collections.IEnumerator AutoResumeIKAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResumeIKIfPaused();
    }
}