using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft
{
    public sealed class AnimationStateController : MonoBehaviour
    {
        private Animator animator;
        private int isDancingId;

        private IEnumerable<Collider> ragdollColliders;
        private IEnumerable<Rigidbody> ragdollBodies;

        private void Awake()
        {
            animator = GetComponent<Animator>();

            ragdollColliders = GetComponentsInChildren<Collider>()
                .Where(c => c.gameObject != gameObject)
                .ToList();

            ragdollBodies = GetComponentsInChildren<Rigidbody>().ToList();

            SetRagdoll(false);
        }

        private void SetRagdoll(bool enable)
        {
            foreach (var c in ragdollColliders)
            {
                c.isTrigger = !enable;
            }

            foreach (var b in ragdollBodies)
            {
                b.isKinematic = !enable;
            }

            animator.enabled = !enable;
        }

        private void Start()
        {
            isDancingId = Animator.StringToHash("isDancing");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                animator.SetBool(isDancingId, !animator.GetBool(isDancingId));
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                SetRagdoll(true);
            }
        }
    }
}