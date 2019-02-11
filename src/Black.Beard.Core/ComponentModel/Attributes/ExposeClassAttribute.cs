using System;

namespace Bb.ComponentModel.Attributes
{

    /// <summary>
    /// specify this class contains method to expose
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ExposeClassAttribute : Attribute
    {


        public ExposeClassAttribute()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">key for matching ruless</param>
        public ExposeClassAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName { get; set; }

        public string Context { get; set; }

    }

}