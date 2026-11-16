using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RobotSphereAudio : MonoBehaviour
{
    public enum MovementMode { None, Walking, Rolling }

    [Header("Audio Sources")]
    [Tooltip("Dedicated source for looping walk servos or continuous rolling")]
    public AudioSource movementSource;
    [Tooltip("Dedicated source for one-shot opening and closing")]
    public AudioSource actionSource;

    [Header("Movement Loops")]
    public AudioClip walkLoopClip;
    public AudioClip rollLoopClip;
    [Range(0.8f, 1.3f)] public float walkPitch = 1.0f;
    [Range(0.8f, 1.5f)] public float rollPitch = 1.15f;

    [Header("Transformation SFX")]
    public AudioClip openClip;
    public AudioClip closeClip;

    private Animator animator;
    private MovementMode currentMode = MovementMode.None;
    private int lastStateHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (animator == null) return;

        // If transitioning to a new state, read the upcoming state immediately so audio doesn't lag
        AnimatorStateInfo stateInfo = animator.IsInTransition(0)
            ? animator.GetNextAnimatorStateInfo(0)
            : animator.GetCurrentAnimatorStateInfo(0);

        int currentHash = stateInfo.shortNameHash;
        bool stateJustChanged = currentHash != lastStateHash;

        if (stateInfo.IsName("anim_Walk_Loop"))
        {
            SetMovementLoop(MovementMode.Walking, walkLoopClip, walkPitch);
        }
        else if (stateInfo.IsName("closed_Roll_Loop"))
        {
            SetMovementLoop(MovementMode.Rolling, rollLoopClip, rollPitch);
        }
        else if (stateInfo.IsName("anim_open") || stateInfo.IsName("anim_closed_StopRoll"))
        {
            StopMovementLoop();
            if (stateJustChanged) PlayActionSound(openClip);
        }
        else if (stateInfo.IsName("anim_close") || stateInfo.IsName("anim_open_GoToRoll"))
        {
            StopMovementLoop();
            if (stateJustChanged) PlayActionSound(closeClip);
        }
        else // anim_Idle_Loop_S
        {
            StopMovementLoop();
        }

        lastStateHash = currentHash;
    }

    private void SetMovementLoop(MovementMode targetMode, AudioClip clip, float pitch)
    {
        if (clip == null || movementSource == null) return;

        if (currentMode != targetMode || !movementSource.isPlaying)
        {
            currentMode = targetMode;
            movementSource.clip = clip;
            movementSource.pitch = pitch;
            movementSource.loop = true;
            movementSource.Play();
        }
    }

    private void StopMovementLoop()
    {
        if (movementSource != null && movementSource.isPlaying)
        {
            movementSource.Stop();
        }
        currentMode = MovementMode.None;
    }

    private void PlayActionSound(AudioClip clip)
    {
        if (clip == null || actionSource == null) return;
        actionSource.pitch = Random.Range(0.96f, 1.04f);
        actionSource.PlayOneShot(clip);
    }
}