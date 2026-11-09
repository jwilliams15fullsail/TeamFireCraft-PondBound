using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using UnityEngine;

// The level's objectives. The main ones are done in order, one at a time. Optional ones run
// alongside them: an optional objective shows up as soon as every main objective above it in
// the list is done, and stays (with its own marker) until it's done. Optional ones never hold
// up the main objectives. ObjectiveMarkers shows all the active ones.
// A switch, plate or anything else reports with ObjectiveSequence.Report(this) on the host: that
// completes the objective whose Completed By is that object (or one of its parents) or, if
// no objective names it, the current main objective when that has no Completed By set.
// Up to 32 objectives.
public class ObjectiveSequence : NetworkBehaviour
{
    [Serializable]
    public class Objective
    {
        [Tooltip("What the player is told, e.g. \"Press the switch on the server racks\".")]
        [TextArea(1, 3)] public string text;
        [Tooltip("Where the marker points. Empty: no marker for this one, just the text.")]
        public Transform target;
        [Tooltip("Runs alongside the main objectives: it appears once every main objective above it is done, and never holds them up.")]
        public bool optional;
        [Tooltip("The switch, plate or other object that completes this one (or a parent of it). Empty: whatever completes the current main objective.")]
        public GameObject completedBy;
    }

    const int MaxObjectives = 32;

    [SerializeField] Objective[] steps = new Objective[0];
    [SerializeField, HideInInspector] Transform[] objectives; // the old list, copied into Steps once

    readonly SyncVar<int> done = new(); // bit i set = objective i completed

    int Count => steps != null ? Mathf.Min(steps.Length, MaxObjectives) : 0;
    bool IsDone(int i) => (done.Value & (1 << i)) != 0;

    // The main objective the players should do now (-1 = every main objective is done).
    int Current
    {
        get
        {
            for (int i = 0; i < Count; i++)
            {
                if (!steps[i].optional && !IsDone(i))
                    return i;
            }
            return -1;
        }
    }

    // Shown right now: the current main objective, or an optional one that every main objective
    // above it is done for.
    bool IsActive(int i)
    {
        if (IsDone(i))
            return false;
        int current = Current;
        return steps[i].optional ? current < 0 || i < current : i == current;
    }

    // Fills the list with the objectives to show: the main one first, then the optional ones.
    public void GetActive(List<Objective> into)
    {
        int current = Current;
        if (current >= 0)
            into.Add(steps[current]);
        for (int i = 0; i < Count; i++)
        {
            if (steps[i].optional && IsActive(i))
                into.Add(steps[i]);
        }
    }

    public Transform CurrentTarget
    {
        get
        {
            int i = Current;
            return i >= 0 ? steps[i].target : null;
        }
    }

    // Every active objective as plain text, one per line, optional ones marked.
    public string CurrentText
    {
        get
        {
            var lines = new List<string>();
            var active = new List<Objective>();
            GetActive(active);
            foreach (Objective objective in active)
                lines.Add(objective.optional ? "Optional: " + objective.text : objective.text);
            return string.Join("\n", lines);
        }
    }

    // All the main objectives are done (optional ones left over don't count).
    public bool IsComplete => Current < 0;

    // Host: every objective sequence that is running, so switches and plates can find theirs.
    static readonly List<ObjectiveSequence> running = new();

    public override void OnStartServer()
    {
        base.OnStartServer();
        running.Add(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        running.Remove(this);
    }

    // Host only: a switch, plate or anything else was used. Every objective sequence in its scene
    // checks whether that completes an objective (see Complete), so nothing needs linking up.
    public static void Report(Component source)
    {
        foreach (ObjectiveSequence sequence in running)
        {
            if (sequence.gameObject.scene == source.gameObject.scene)
                sequence.Complete(source);
        }
    }

    // An objective done early (say the capsules opened before they're asked for) counts, and
    // simply never shows up.
    [Server]
    public void Complete(Component source)
    {
        GameObject who = source != null ? source.gameObject : null;
        bool named = false;
        for (int i = 0; i < Count; i++)
        {
            if (who == null || steps[i].completedBy == null || !who.transform.IsChildOf(steps[i].completedBy.transform))
                continue;
            named = true;
            if (!IsDone(i))
            {
                done.Value |= 1 << i;
                return;
            }
        }
        if (named)
            return; // its objective is done already (a second press of the same switch)

        int current = Current;
        if (current >= 0 && steps[current].completedBy == null)
            done.Value |= 1 << current;
    }

    // Older calls: completes the current main objective.
    [Server]
    public void CompleteCurrentObjective()
    {
        int current = Current;
        if (current >= 0)
            done.Value |= 1 << current;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        // Copy the old list of targets into Steps, once, so nothing set up before is lost.
        if ((steps == null || steps.Length == 0) && objectives != null && objectives.Length > 0)
        {
            steps = new Objective[objectives.Length];
            for (int i = 0; i < objectives.Length; i++)
                steps[i] = new Objective { target = objectives[i], text = objectives[i] != null ? objectives[i].name : "" };
            objectives = null;
        }
    }
#endif
}