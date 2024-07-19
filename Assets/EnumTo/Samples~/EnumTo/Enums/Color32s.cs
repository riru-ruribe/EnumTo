using UnityEngine;

namespace EnumTo.Samples
{
    [EnumToExtendable(typeof(int), typeof(Color32))]
    public enum Color32s
    {
        [EnumToValue(255, 255, 255, 255)] White,
        [EnumToValue(0, 0, 0, 255)] Black,
        [EnumToValue(255, 0, 0, 255)] Red,
    }
}
