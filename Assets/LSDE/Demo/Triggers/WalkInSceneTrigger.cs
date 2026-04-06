using LSDE.Runtime;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Walk-in scene trigger — automatically launches an LSDE dialogue scene
    /// when the player enters the trigger zone. No click required.
    ///
    /// Unlike <see cref="DialogueProximityTrigger"/> (which requires a click),
    /// this trigger fires immediately on proximity. It is one-shot: once triggered,
    /// it will not fire again until explicitly re-armed.
    ///
    /// Designed for placing on standalone GameObjects in the scene (e.g. a rally point,
    /// glowing zone, or boss entrance) rather than on NPC characters.
    ///
    /// Requirements:
    /// - The player must have a Collider + Rigidbody (isKinematic) and the tag "Player"
    /// - A SphereCollider (isTrigger) is added automatically at Awake
    ///
    /// Setup in Unity Editor:
    /// 1. Create an empty GameObject at the desired trigger location
    /// 2. Attach this script to it
    /// 3. Assign the <see cref="_demoSceneTrigger"/> reference (DemoSceneTrigger)
    /// 4. Select the LSDE scene to launch from the dropdown
    /// 5. Optionally add a visual child (mesh, particle system) to indicate the zone
    /// </summary>
    public class WalkInSceneTrigger : MonoBehaviour
    {
        [Header("Trigger Zone")]
        [SerializeField]
        [Tooltip(
            "Radius of the trigger zone. The player must walk into this radius "
                + "for the scene to launch. A SphereCollider (isTrigger) is created "
                + "automatically with this radius."
        )]
        private float _triggerRadius = 2f;

        [Header("Dialogue")]
        [SerializeField]
        [Tooltip(
            "Reference to the DemoSceneTrigger that manages engine initialization "
                + "and scene launching."
        )]
        private DemoSceneTrigger _demoSceneTrigger;

        [SerializeField]
        [LsdeSceneSelector]
        [Tooltip("The LSDE scene to launch when the player walks into the zone.")]
        private string _sceneUuidToLaunch;

        private bool _hasTriggered;

        private void Awake()
        {
            var proximitySphere = gameObject.AddComponent<SphereCollider>();
            proximitySphere.isTrigger = true;
            proximitySphere.radius = _triggerRadius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (_hasTriggered)
            {
                return;
            }

            if (_demoSceneTrigger == null)
            {
                Debug.LogWarning(
                    $"[LSDE Demo] WalkInSceneTrigger on '{gameObject.name}' "
                        + "has no DemoSceneTrigger assigned."
                );
                return;
            }

            if (_demoSceneTrigger.IsDialogueSceneActive)
            {
                return;
            }

            if (string.IsNullOrEmpty(_sceneUuidToLaunch))
            {
                Debug.LogWarning(
                    $"[LSDE Demo] WalkInSceneTrigger on '{gameObject.name}' "
                        + "has no scene UUID configured."
                );
                return;
            }

            _hasTriggered = true;
            _demoSceneTrigger.LaunchDialogueScene(_sceneUuidToLaunch);

            Debug.Log($"[LSDE Demo] Walk-in trigger '{gameObject.name}' launched scene.");
        }
    }
}
