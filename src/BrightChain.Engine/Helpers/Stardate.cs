namespace BrightChain.Engine.Helpers
{
    using System;
    using BrightChain.Engine.Exceptions;

    public class Stardate : IComparable<Stardate>, IEquatable<Stardate>, IFormattable
    {
        public static readonly Stardate BrightChainBirthDay = new Stardate(new DateTime(year: 2021, month: 6, day: 16, hour: 3, minute: 33, second: 33));

        private readonly double starDate;

        private readonly DateTime utcDate;

        public Stardate(DateTime dateTime)
        {
            this.utcDate = dateTime;
            this.starDate = StardateAsDouble(dateTime);
        }

        /// <summary>
        /// Copied almost entirely from https://www.codeproject.com/tips/98761/stardates-in-c
        /// </summary>
        /// <param name="conversionDate"></param>
        /// <returns></returns>
        public static double StardateAsDouble(DateTime conversionDate)
        {
            DateTime starTrekEpoch = new DateTime(2323, 1, 1, 0, 0, 0);

            // you can replace the {conversionDate} with year values running all
            // the way back to January 1, 1946 at 0:00:00 and still maintain 
            // a positive stardate by virtue of the way the calculation is 
            // done, but this is contingent upon application of a 377 year 
            // offset which puts us into the current Trek year.  Follow the 
            // code for a little bit clearer understanding...
            if (conversionDate < new DateTime(1946, 1, 1, 0, 0, 0))
            {
                throw new BrightChainException("Unsupported");
            }

            // derive the total offset between present date and trek date
            // if we don't do the year offset, we will end up with a date
            // that is in the negative, which while technically correct
            // it's probably not what we want so we adjust the year value
            // of the current date to bring us into the proper Trek year
            conversionDate = conversionDate.AddYears(377);

            TimeSpan timeOffset = conversionDate - starTrekEpoch;

            // we divide into a highly granular value to get the most
            // accurate value possible for our calculation.  What we are
            // actually figuring out the average number of seconds in a
            // 4 year leap/non-leap cycle and dividing the total number of
            // milliseconds in our time offset to arrive at our raw stardate
            // value.
            //
            // we further round this value to 2 decimal places and miliply it
            // by 100 in rounded form, and then divide by 100 to get our two 
            // decimal places back. 2.7 stardate units account for 1 earth day
            // so we do the rounding and multiply / divide operations to get 
            // granularity back into the final date value.
            //
            // it makes sense when you look at it :-)  trust me.
            double yearValue = timeOffset.TotalMilliseconds / (60 * 60 * 24 * 365.2422);
            double stardate = Math.Floor(yearValue * 100);
            stardate = stardate / 100;

            return stardate;
        }

        public double Double => this.starDate;

        public DateTime DateTime => this.utcDate;

        public int CompareTo(Stardate other)
        {
            return (this.Double == other.Double) ? 0 : (this.Double > other.Double) ? -1 : 1;
        }

        public bool Equals(Stardate other)
        {
            return this.Double == other.Double;
        }

        public string ToString(string format, IFormatProvider formatProvider) =>
            this.Double.ToString(format, formatProvider);

        public string ToString() =>
            this.Double.ToString();
    }
}
