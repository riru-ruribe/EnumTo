namespace EnumTo.Samples
{
    [EnumToExtendable]
    public enum Fruits
    {
        Apple,
        [EnumToValue(2)] Peach,
        [EnumToValue(4)] Cherry,
    }
}
