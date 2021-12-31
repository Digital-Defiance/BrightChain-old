namespace BrightChain.API.Helpers
{
    using StarDateCalc;

    public class StarDateConverter
    {
        public static readonly DateTime BirthdayGregorian = new DateTime(year: 2021, month: 6, day: 16, hour: 3, minute: 33, second: 33);

        private static readonly double birthdayStardate = new StarDate(StarDate.LanguageCode.English).TNG(StarDateConverter.BirthdayGregorian);

        public static readonly double Offset = 99055.68D - StarDateConverter.birthdayStardate;

        public static readonly double Birthday = Stardate(StarDateConverter.BirthdayGregorian);

        public static double Stardate(DateTime date) => new StarDate(StarDate.LanguageCode.English).TNG(date) + Offset;

        public static double Now => Stardate(DateTime.Now);
    }
}
