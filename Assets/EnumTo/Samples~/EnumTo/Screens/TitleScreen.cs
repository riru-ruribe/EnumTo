using UnityEngine;

namespace EnumTo.Samples
{
    sealed class TitleScreen : IScreen
    {
        public string Name { get; } = "Title";
        public void Next()
        {
            Debug.LogWarning(Name);
        }
    }
}
