using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Minecraft
{
    [RequireComponent(typeof(Volume))]
    public sealed class PostProcessingController : MonoBehaviour
    {
        private DepthOfField dof;

        private float blurOffFadeTime = float.MinValue;
        private float timeAcc = 0.0f;

        private const float farFocusStart = 37.2f;
        private const float farFocusEnd = 163.9f;

        private void Start()
        {
            var volume = GetComponent<Volume>();
            volume.profile.TryGet(out dof);
        }

        private void Update()
        {
            if (timeAcc <= blurOffFadeTime)
            {
                dof.farFocusStart.value = Mathf.Lerp(0.0f, farFocusStart, timeAcc / blurOffFadeTime);
                dof.farFocusEnd.value = Mathf.Lerp(0.0f, farFocusEnd, timeAcc / blurOffFadeTime);

                timeAcc += Time.unscaledDeltaTime;

            }
        }

        public void TurnBlurOff(float fadeTime)
        {
            timeAcc = 0.0f;
            blurOffFadeTime = fadeTime;
        }

        public void TurnBlurOn()
        {
            timeAcc = 0.0f;
            blurOffFadeTime = float.MinValue;

            dof.farFocusStart.value = 0.0f;
            dof.farFocusEnd.value = 0.0f;
        }
    }
}