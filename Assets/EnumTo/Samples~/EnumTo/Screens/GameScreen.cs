using UnityEngine;

namespace EnumTo.Samples
{
    sealed class GameScreen : IScreen
    {
        public string Name { get; } = "Game";
        public void Next()
        {
            Debug.LogError(Name);
        }
    }
}
