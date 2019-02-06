using System;

namespace Bb.ComponentModel.Attributes
{

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RegisterMethodAttribute : Attribute
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="displayName">key for matching ruless</param>
        public RegisterMethodAttribute(string displayName)
        {
            DisplayName = displayName;        
        }

        public string DisplayName { get; }
    }

}