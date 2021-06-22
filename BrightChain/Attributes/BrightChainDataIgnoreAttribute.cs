using System;
namespace BrightChain.Attributes
{
    /// <summary>
    /// Will be ignored during persisting to disk / virtual properties around the central data
    /// </summary>
    public class BrightChainDataIgnoreAttribute : Attribute
    {
        public BrightChainDataIgnoreAttribute()
        {
        }
    }
}
