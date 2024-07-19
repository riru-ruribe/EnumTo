using UnityEngine;

namespace EnumTo.Samples
{
    [EnumToExtendable(typeof(int), typeof(Color))]
    public enum Colors
    {
        [EnumToValue(1, 1, 1, 1)] White,
        [EnumToValue(0, 0, 0, 1)] Black,
        [EnumToValue(1, 0, 0, 1)] Red,
    }
}
