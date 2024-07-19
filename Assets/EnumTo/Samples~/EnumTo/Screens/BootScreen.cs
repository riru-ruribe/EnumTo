using UnityEngine;

namespace EnumTo.Samples
{
    sealed class BootScreen : IScreen
    {
        public string Name { get; } = "Boot";
        public void Next()
        {
            Debug.Log(Name);
        }
    }
}
