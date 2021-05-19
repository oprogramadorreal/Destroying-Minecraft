using System;
using UnityEngine;

namespace Minecraft
{
    public sealed class SunController : MonoBehaviour
    {
        private bool isNight = false;

        public event EventHandler<DayLightChangedEventArgs> DayLightChanged;

        private void Update()
        {
            var currentIsNight = Vector3.Dot(transform.TransformDirection(Vector3.forward), Vector3.down) < 0.0f;

            if (isNight != currentIsNight)
            {
                isNight = currentIsNight;
                OnDayLightChanged();
            }

            transform.Rotate(Time.deltaTime * GetSunSpeed(), 0.0f, 0.0f);
        }

        private float GetSunSpeed()
        {
            return isNight ? 4.0f : 2.0f;
        }

        public bool IsNight()
        {
            return isNight;
        }

        private void OnDayLightChanged()
        {
            var handler = DayLightChanged;
            handler?.Invoke(this, new DayLightChangedEventArgs { IsNight = isNight });
        }

        public sealed class DayLightChangedEventArgs : EventArgs
        {
            public bool IsNight { get; set; }
        }
    }
}
