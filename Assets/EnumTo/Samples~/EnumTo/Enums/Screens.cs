namespace EnumTo.Samples
{
    [EnumToExtendable(typeof(int), typeof(IScreen))]
    public enum Screens
    {
        [EnumToValue(typeof(BootScreen))]
        Boot,
        [EnumToValue(typeof(TitleScreen))]
        Title,
        [EnumToValue(typeof(GameScreen))]
        Game,
    }
}
