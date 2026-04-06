using System.Collections.Generic;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Breadcrumb trail follow system — recruited party NPCs follow the player
    /// in single-file (queue) formation by walking along the player's recorded path.
    ///
    /// Port of the TS <c>setup-party-follow.ts</c> pattern:
    /// Instead of each NPC targeting a fixed offset from the player, the system
    /// records a breadcrumb trail of the player's positions and assigns each
    /// follower a point further back on that trail. This creates a natural
    /// "follow-the-leader" chain where members walk the same path the player took.
    ///
    /// Collision between followers and the player is handled automatically by
    /// Unity's <see cref="CharacterController"/> — no manual overlap resolution needed.
    ///
    /// The controller subscribes to <see cref="DemoGameState.OnPartyMemberAdded"/>
    /// to detect newly recruited members at runtime. It also checks for members
    /// already in the party at <see cref="Start"/> (e.g. set from the Inspector).
    ///
    /// During dialogue scenes, the trail is paused and followers stop moving.
    /// When dialogue ends, the trail is cleared and recording restarts fresh
    /// to avoid stale path data.
    ///
    /// Setup in Unity Editor:
    /// 1. Attach this script to the **player** GameObject (the leader)
    /// 2. Assign <see cref="_gameState"/> (DemoGameState), <see cref="_characterRegistry"/>
    ///    (DialogueCharacterRegistry), and optionally <see cref="_demoSceneTrigger"/>
    ///    (DemoSceneTrigger — for dialogue pause/resume)
    /// 3. Recruitable NPCs must have a <see cref="CharacterMovementController"/>
    ///    and a <see cref="DialogueCharacterMarker"/> with their LSDE character ID
    /// </summary>
    public class PartyFollowController : MonoBehaviour
    {
        [Header("Trail Settings")]
        [SerializeField]
        [Tooltip(
            "Minimum distance (world units) the player must move before a new "
                + "breadcrumb point is recorded. Lower values give smoother trails "
                + "but use more memory. Default 0.5 is a good balance."
        )]
        private float _breadcrumbSpacing = 0.5f;

        [SerializeField]
        [Tooltip(
            "Maximum number of breadcrumb points stored. Oldest points are discarded "
                + "when this limit is reached. 600 is enough for 3 followers at generous spacing."
        )]
        private int _maximumTrailPoints = 600;

        [Header("Follower Settings")]
        [SerializeField]
        [Tooltip(
            "Distance (world units) between each follower on the trail. "
                + "Follower 1 walks at 1x this distance behind the player, "
                + "follower 2 at 2x, etc. Default 2.0 units."
        )]
        private float _followDistance = 2.0f;

        [SerializeField]
        [Tooltip(
            "Minimum distance from the current trail target before a new movement "
                + "command is sent. Prevents jittering from tiny position updates. "
                + "Default 0.3 units."
        )]
        private float _movementUpdateThreshold = 0.3f;

        [Header("References")]
        [SerializeField]
        [Tooltip("Reference to the game state for party membership checks and events.")]
        private DemoGameState _gameState;

        [SerializeField]
        [Tooltip(
            "Reference to the character registry for looking up NPC GameObjects "
                + "by their LSDE character ID."
        )]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip(
            "Optional reference to the scene trigger. When assigned, followers pause "
                + "during dialogue scenes and resume when dialogue ends."
        )]
        private DemoSceneTrigger _demoSceneTrigger;

        /// <summary>
        /// A single point on the breadcrumb trail.
        /// Stored oldest-first: index 0 is the oldest point, last index is the newest.
        /// </summary>
        private struct BreadcrumbPoint
        {
            /// <summary>World position of this breadcrumb.</summary>
            public Vector3 Position;

            /// <summary>
            /// Cumulative arc-length distance from the very first recorded point
            /// to this point. Increases monotonically along the trail.
            /// </summary>
            public float CumulativeDistance;
        }

        /// <summary>
        /// Tracks the state of a single follower NPC.
        /// </summary>
        private struct FollowerState
        {
            /// <summary>The LSDE character ID (e.g. <c>lsdeCharacter.l1</c>).</summary>
            public string CharacterId;

            /// <summary>The movement controller used to send movement targets.</summary>
            public CharacterMovementController MovementController;

            /// <summary>
            /// The last target position sent to the movement controller.
            /// Used to avoid resending when the follower hasn't moved enough.
            /// </summary>
            public Vector3 LastAssignedTarget;

            /// <summary>
            /// When true, this follower is being moved by an external system
            /// (e.g. <see cref="DemoActionExecutor.ExecuteMoveCharacterAt"/>).
            /// The follow controller will not assign trail targets until this is cleared.
            /// </summary>
            public bool IsExternallyControlled;
        }

        private readonly List<BreadcrumbPoint> _trailPoints = new();
        private readonly List<FollowerState> _activeFollowers = new();
        private bool _isTrailPaused;
        private bool _wasDialogueActiveLastFrame;

        private void Awake()
        {
            if (_gameState != null)
            {
                _gameState.OnPartyMemberAdded += HandlePartyMemberAdded;
            }
            else
            {
                Debug.LogWarning(
                    "[LSDE Demo] PartyFollowController has no DemoGameState assigned. "
                        + "Followers will not be registered automatically."
                );
            }
        }

        private void Start()
        {
            // Seed the trail with the player's current position
            _trailPoints.Add(
                new BreadcrumbPoint { Position = transform.position, CumulativeDistance = 0f }
            );

            // Register followers for characters already in the party at startup
            // (e.g. set from the Inspector list in DemoGameState).
            // We skip the player character (l4) since the player is the leader, not a follower.
            if (_gameState != null && _characterRegistry != null)
            {
                // Check each known party dictionary member
                string[] potentialPartyMembers =
                {
                    lsdeDictionaryparty.l1,
                    lsdeDictionaryparty.l2,
                    lsdeDictionaryparty.l3,
                };

                foreach (string memberId in potentialPartyMembers)
                {
                    if (_gameState.IsInParty(memberId))
                    {
                        RegisterFollower(memberId);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_gameState != null)
            {
                _gameState.OnPartyMemberAdded -= HandlePartyMemberAdded;
            }
        }

        /// <summary>
        /// Temporarily suspend follow-controller targeting for a specific character.
        /// Call this before an external system (e.g. <see cref="DemoActionExecutor"/>)
        /// takes control of the character's movement. While suspended, the follow controller
        /// will not assign trail targets to this character.
        /// </summary>
        /// <param name="characterId">The LSDE character ID to suspend (e.g. <c>lsdeCharacter.l1</c>).</param>
        public void SuspendFollower(string characterId)
        {
            for (int index = 0; index < _activeFollowers.Count; index++)
            {
                if (_activeFollowers[index].CharacterId == characterId)
                {
                    var follower = _activeFollowers[index];
                    follower.IsExternallyControlled = true;
                    _activeFollowers[index] = follower;

                    Debug.Log(
                        $"[LSDE Demo] Follower '{characterId}' suspended — "
                            + "external system has control."
                    );
                    return;
                }
            }
        }

        /// <summary>
        /// Resume follow-controller targeting for a character that was suspended
        /// via <see cref="SuspendFollower"/>. The character will rejoin the trail
        /// on the next frame.
        /// </summary>
        /// <param name="characterId">The LSDE character ID to resume.</param>
        public void ResumeFollower(string characterId)
        {
            for (int index = 0; index < _activeFollowers.Count; index++)
            {
                if (_activeFollowers[index].CharacterId == characterId)
                {
                    var follower = _activeFollowers[index];
                    follower.IsExternallyControlled = false;
                    // Reset last assigned target so the follower picks up the trail
                    // from wherever it is now
                    follower.LastAssignedTarget = follower.MovementController.transform.position;
                    _activeFollowers[index] = follower;

                    Debug.Log(
                        $"[LSDE Demo] Follower '{characterId}' resumed — "
                            + "follow controller has control."
                    );
                    return;
                }
            }
        }

        private void Update()
        {
            // --- Dialogue pause/resume ---
            bool isDialogueActive =
                _demoSceneTrigger != null && _demoSceneTrigger.IsDialogueSceneActive;

            if (isDialogueActive && !_wasDialogueActiveLastFrame)
            {
                // Dialogue just started → pause trail and stop all followers
                PauseTrail();
            }
            else if (!isDialogueActive && _wasDialogueActiveLastFrame)
            {
                // Dialogue just ended → resume trail recording
                ResumeTrail();
            }

            _wasDialogueActiveLastFrame = isDialogueActive;

            if (_isTrailPaused)
            {
                return;
            }

            // --- Record breadcrumb ---
            RecordBreadcrumb();

            // --- Assign trail targets to followers ---
            AssignFollowerTargets();
        }

        /// <summary>
        /// Called when a new character is added to the party at runtime.
        /// Finds the character's <see cref="CharacterMovementController"/> and
        /// adds it to the active followers list.
        /// </summary>
        private void HandlePartyMemberAdded(string memberId)
        {
            // Don't add the player character as a follower
            if (memberId == lsdeCharacter.l4)
            {
                return;
            }

            RegisterFollower(memberId);
        }

        /// <summary>
        /// Look up the character in the registry and register it as an active follower.
        /// </summary>
        private void RegisterFollower(string characterId)
        {
            // Check if already registered (avoid duplicates)
            foreach (var existingFollower in _activeFollowers)
            {
                if (existingFollower.CharacterId == characterId)
                {
                    return;
                }
            }

            if (_characterRegistry == null)
            {
                Debug.LogWarning(
                    "[LSDE Demo] PartyFollowController has no CharacterRegistry. "
                        + $"Cannot register follower '{characterId}'."
                );
                return;
            }

            var characterMarker = _characterRegistry.FindMarkerByCharacterId(characterId);

            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"[LSDE Demo] Character '{characterId}' not found in registry. "
                        + "Cannot register as follower."
                );
                return;
            }

            var movementController = characterMarker.GetComponent<CharacterMovementController>();

            if (movementController == null)
            {
                Debug.LogWarning(
                    $"[LSDE Demo] Character '{characterId}' on '{characterMarker.gameObject.name}' "
                        + "has no CharacterMovementController. Cannot follow."
                );
                return;
            }

            _activeFollowers.Add(
                new FollowerState
                {
                    CharacterId = characterId,
                    MovementController = movementController,
                    LastAssignedTarget = movementController.transform.position,
                }
            );

            Debug.Log(
                $"[LSDE Demo] {characterId} is now following the player "
                    + $"(follower #{_activeFollowers.Count} in queue)."
            );
        }

        /// <summary>
        /// Record the player's current position as a new breadcrumb point
        /// if the player has moved far enough from the last recorded point.
        /// </summary>
        private void RecordBreadcrumb()
        {
            Vector3 currentPlayerPosition = transform.position;

            if (_trailPoints.Count == 0)
            {
                _trailPoints.Add(
                    new BreadcrumbPoint
                    {
                        Position = currentPlayerPosition,
                        CumulativeDistance = 0f,
                    }
                );
                return;
            }

            // The newest point is at the end of the list
            BreadcrumbPoint newestPoint = _trailPoints[_trailPoints.Count - 1];
            float distanceFromNewest = Vector3.Distance(
                currentPlayerPosition,
                newestPoint.Position
            );

            if (distanceFromNewest >= _breadcrumbSpacing)
            {
                float newCumulativeDistance = newestPoint.CumulativeDistance + distanceFromNewest;

                _trailPoints.Add(
                    new BreadcrumbPoint
                    {
                        Position = currentPlayerPosition,
                        CumulativeDistance = newCumulativeDistance,
                    }
                );

                // Trim the oldest points if the trail is too long
                if (_trailPoints.Count > _maximumTrailPoints)
                {
                    _trailPoints.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// For each active follower, sample the trail at the appropriate distance
        /// behind the leader and update the follower's movement target.
        /// </summary>
        private void AssignFollowerTargets()
        {
            for (int followerIndex = 0; followerIndex < _activeFollowers.Count; followerIndex++)
            {
                var follower = _activeFollowers[followerIndex];

                // Skip followers that are being controlled by an external system
                // (e.g. DemoActionExecutor's moveCharacterAt action)
                if (follower.IsExternallyControlled)
                {
                    continue;
                }

                // Follower 1 is at 1x followDistance, follower 2 at 2x, etc.
                float targetTrailDistance = (followerIndex + 1) * _followDistance;

                Vector3? sampledPosition = SampleTrailAtDistance(targetTrailDistance);

                if (!sampledPosition.HasValue)
                {
                    // Trail not long enough yet — follower stays in place
                    continue;
                }

                Vector3 targetPosition = sampledPosition.Value;

                float distanceFromLastTarget = Vector3.Distance(
                    targetPosition,
                    follower.LastAssignedTarget
                );

                if (distanceFromLastTarget > _movementUpdateThreshold)
                {
                    follower.MovementController.SetMovementTarget(targetPosition);
                    follower.LastAssignedTarget = targetPosition;

                    // Write back the modified struct (structs are value types in C#)
                    _activeFollowers[followerIndex] = follower;
                }
            }
        }

        /// <summary>
        /// Sample the trail at a given distance behind the leader (the newest point).
        /// Interpolates between the two closest breadcrumb points for smooth positioning.
        ///
        /// Port of <c>sampleTrailAtDistance()</c> from the TS reference.
        /// </summary>
        /// <param name="distanceBehindLeader">
        /// How far behind the leader (in world units) to sample.
        /// </param>
        /// <returns>
        /// The interpolated world position, or null if the trail is not long enough.
        /// </returns>
        private Vector3? SampleTrailAtDistance(float distanceBehindLeader)
        {
            if (_trailPoints.Count < 2)
            {
                return null;
            }

            // The total trail length is the cumulative distance of the newest point
            float totalTrailLength = _trailPoints[_trailPoints.Count - 1].CumulativeDistance;

            // The target is "distanceBehindLeader" back from the head of the trail
            float targetCumulativeDistance = totalTrailLength - distanceBehindLeader;

            // Trail not long enough — return the oldest point so followers
            // gather at the start of the trail rather than staying frozen
            if (targetCumulativeDistance < _trailPoints[0].CumulativeDistance)
            {
                return _trailPoints[0].Position;
            }

            // Find the two bracketing points and interpolate
            for (int pointIndex = 1; pointIndex < _trailPoints.Count; pointIndex++)
            {
                BreadcrumbPoint currentPoint = _trailPoints[pointIndex];

                if (currentPoint.CumulativeDistance >= targetCumulativeDistance)
                {
                    BreadcrumbPoint previousPoint = _trailPoints[pointIndex - 1];

                    float segmentLength =
                        currentPoint.CumulativeDistance - previousPoint.CumulativeDistance;

                    if (segmentLength < 0.001f)
                    {
                        return currentPoint.Position;
                    }

                    float interpolationRatio =
                        (targetCumulativeDistance - previousPoint.CumulativeDistance)
                        / segmentLength;

                    return Vector3.Lerp(
                        previousPoint.Position,
                        currentPoint.Position,
                        interpolationRatio
                    );
                }
            }

            // Fallback — return the newest point
            return _trailPoints[_trailPoints.Count - 1].Position;
        }

        /// <summary>
        /// Pause the trail recording and stop all follower movement.
        /// Called when a dialogue scene starts.
        /// </summary>
        private void PauseTrail()
        {
            _isTrailPaused = true;

            // Only stop followers that are NOT externally controlled.
            // Externally controlled followers (e.g. being moved by an action) must
            // keep their current movement — StopMovement() would interrupt the action.
            foreach (var follower in _activeFollowers)
            {
                if (!follower.IsExternallyControlled)
                {
                    follower.MovementController.StopMovement();
                }
            }
        }

        /// <summary>
        /// Resume trail recording after a dialogue scene ends.
        /// Clears the trail and seeds it with the current player position
        /// to prevent followers from walking back to stale positions.
        /// </summary>
        private void ResumeTrail()
        {
            _isTrailPaused = false;

            // Clear stale trail data — the player may have moved during dialogue
            _trailPoints.Clear();
            _trailPoints.Add(
                new BreadcrumbPoint { Position = transform.position, CumulativeDistance = 0f }
            );
        }

        /// <summary>
        /// Reset the entire follow system to its initial state.
        /// Called by <see cref="WebGlSceneController"/> when switching between demo scenes.
        /// Stops all follower movement, clears the follower list and trail,
        /// and resets the pause state so the system is ready for a fresh demo.
        /// </summary>
        public void ResetFollowers()
        {
            // Stop movement on all active followers before clearing
            foreach (var follower in _activeFollowers)
            {
                if (follower.MovementController != null)
                {
                    follower.MovementController.StopMovement();
                }
            }

            _activeFollowers.Clear();

            // Reset trail
            _trailPoints.Clear();
            _trailPoints.Add(
                new BreadcrumbPoint { Position = transform.position, CumulativeDistance = 0f }
            );

            _isTrailPaused = false;
            _wasDialogueActiveLastFrame = false;

            Debug.Log("[LSDE Demo] PartyFollowController reset — all followers cleared.");
        }
    }
}
