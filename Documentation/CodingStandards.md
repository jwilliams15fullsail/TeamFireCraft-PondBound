
# Coding Standards & Best Practices

## Purpose

These standards establish a consistent approach to writing and organizing code for the project.

These standards apply to all code within the project including code authored by the development team directly as well as using generative AI to create or modify code.

---

# 1. General Principles

- Readability - Another developer should be able to understand any code within the project without extensive explanation.
- Consistency - Similar problems should be solved in similar ways throughout the project.
- Maintainability - Code should be structured so that changes and additions can be made without unnecessary modification to unrelated systems.
- Simplicity - Prefer the simplest solution that adequately solves the problem.
- Reusability - Systems that are expected to be used in multiple places should be designed for reuse.
- Separation of Responsibilities - A class or system should have a clear purpose.
- Minimal Coupling - Systems should avoid unnecessary dependencies on specific objects, scenes, or other systems.
- Performance Awareness - Avoid unnecessarily expensive operations, particularly inside frequently executed methods.
- Team Compatibility - Code must work cleanly with the team's version-control and development workflow.

---

# 2. Project Organization

Project files should be organized into logical, consistently named folders. The following is the recommended structure:

Example:

Assets/
- Art/
	-  Characters/
	-  Environment/
	-  UI/
	-  VFX/
-  Audio/
	-  Music/
	-  SFX/
	-  Voice/
-  Materials/
-  Prefabs/
	-  Characters/
	-  Enemies/
	-  Environment/
	-  Items/
	-  UI/
-  Scenes/
	-  Levels&Environments/
	-  Menus/
	-  Sandboxes&Dev/
-  Scripts/
	-  Core&Systems/
	-  Enemies/
	-  NPCs/
	-  Player/
	-  UI/
-  ScriptableObjects/

---

# 3. Naming Conventions

Comments and developer-created names should use U.S. English spelling and grammar.
Use clear, descriptive names.

### Classes and Methods

Use **PascalCase**:

```csharp
public class PlayerController
{
    public void TakeDamage()
    {
    }
}
```

### Variables

Use **camelCase**:

```csharp
private int currentHealth;
private float movementSpeed;
```

### Boolean Variables

Use names that clearly indicate a true/false condition:

```csharp
bool isDead;
bool canAttack;
bool hasKey;
```

Avoid vague names such as:

```csharp
bool value;
bool thing;
bool state;
```

### Script Files

The filename of a C# script should match the name of its primary class.

For example:

PlayerController.cs
```csharp
 public class PlayerController
```

---

# 4. Code Organization

Each class should have a clear purpose.

For example:

```text
PlayerController  -> Player movement
EnemyController   -> Enemy behavior
OptionMenuUI   	  -> Options menu UI event handling 
```

Avoid creating one large class that handles unrelated systems.

---

# 5. Inspector Variables

Use `[SerializeField]` for variables that need to be edited in the Unity Inspector but do not need to be publicly modified by other scripts.

```csharp
[SerializeField] private int maxHealth = 100;
```

Avoid making variables `public` simply so they appear in the Inspector.

Use `[Header("Header Name")]` to group related variables and organize the Unity Inspector.

```csharp
[Header("Health")]
[SerializeField] private int maxHealth = 100;
[SerializeField] private float regenerationDelay = 2f;
```

---

# 6. Methods

Methods should have a clear and focused purpose.

Good:

```csharp
TakeDamage();
UpdateHealthBar();
SpawnEnemy();
ResetLevel();
```

Avoid vague method names:

```csharp
DoThing();
Process();
HandleStuff();
```

Avoid unnecessarily large methods when functionality can reasonably be separated.

---

# 7. Comments

Write self-documenting code
Bad:
```csharp
t = d * (cb - cdb);
```
Good:
```csharp
totalDamage = damage * (currentBuffs - currentDebuffs);
```

Write useful comments. Use comments to explain **why** something is being done
Bad:
```csharp
// starting spawnDelay coroutine.
yield return new WaitForSeconds(spawnDelay);
```
Good:
```csharp
// Delay spawning until the door animation finishes.
yield return new WaitForSeconds(spawnDelay);
```

Avoid comments that simply repeat what the code already says:
Bad:
```csharp
// Add one to score.
score++;
```

Use `TODO` comments for known unfinished work:
Good:
```csharp
// TODO: Replace temporary explosion effect with final VFX.
```

---

# 8. Unity Practices

Use Unity's lifecycle methods appropriately:

* `Awake()` - initialization that should occur when the object is loaded, including obtaining references.
* `Start()` - initialization that should occur before the first frame update, after `Awake()`.
* `Update()` - frame-based gameplay and input.
* `FixedUpdate()` - physics-related operations.

Avoid performing unnecessary searches or expensive operations every frame.

For example, avoid repeatedly using the following within an update function:
```csharp
GameObject.Find("Player");
```

Instead, obtain the reference once and reuse it.

---

# 9. Keep Changes Focused

When working on a task, avoid modifying unrelated code or systems.

Changes should be limited to what is necessary to implement or support the intended functionality.

---

# 10. Reuse Existing Systems

Before creating a new system, check whether the project already has one that performs the required function. Extend an existing system when appropriate rather than creating a competing system.

---

# 11. Keep Code Simple

Prefer a simple solution that clearly solves the problem over a complicated solution that provides functionality the project does not need. 

Unless there is a specific reason the project needs them, avoid adding:

* Unnecessary frameworks
* Packages
* Design patterns
* Complex architecture
* Optimization systems

---

# 12. Generative AI

Generative AI may be used as a learning and research tool, but may not be used to create or substantially develop the project's code, systems, or other required deliverables.

AI may be used to:

- Research Unity systems, programming concepts, and development methods.
- Ask questions about how a system or programming concept works.
- Explore different approaches to solving a programming problem.
- Act as a "rubber duck" to discuss, analyze, or troubleshoot code that the student has written.
- Help identify possible causes of an error or unexpected behavior.
- Explain unfamiliar code, terminology, or documentation.

AI should not be used to:

- Generate code or systems for the project for the student to incorporate.
- Rewrite or substantially modify project code.
- Generate complete scripts, features, mechanics, or other required project functionality.
- Generate project documentation or other deliverables that are expected to demonstrate the student's own work.
- Replace the student's own research, problem solving, or understanding of the systems being developed.

### Student Responsibility

AI may only be used to support learning, not replace the development process. Students are responsible for writing, implementing, testing, and understanding their own project.

Students should be able to explain the code and systems they create, including:

- How the system works.
- What data structures or important variables are being used and why.
- Why the chosen approach is appropriate for the problem.
- How the system interacts with other parts of the project.

AI assistance should help a student learn how to solve a problem, not provide the solution that the student submits.