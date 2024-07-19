namespace EnumTo.Samples
{
    [EnumToExtendable]
    public enum Animals
    {
        [EnumToName("犬")] Dog,
        [EnumToName("猫")] Cat,
    }
}
