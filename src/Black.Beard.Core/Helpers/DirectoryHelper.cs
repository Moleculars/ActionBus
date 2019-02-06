using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Bb.Core.Helpers
{

    public static class DirectoryHelper
    {

        /// <summary>
        /// Delete all content before delete specified folder 
        /// </summary>
        /// <param name="dir"></param>
        /// <returns>true if all files and directories are deleted</returns>
        public static bool DeletePath(this DirectoryInfo dir)
        {

            bool result = true;

            foreach (var item in dir.GetDirectories())
                try
                {
                    result &= DeletePath(item);
                }
                catch (Exception ex1)
                {
                    result = false;
                }

            foreach (var item in dir.GetFiles())
                try
                {
                    item.Delete();
                }
                catch (Exception ex2)
                {
                    result = false;
                }

            if (result)
                try
                {
                    dir.Delete();
                }
                catch (Exception ex3)
                {
                    result = false;
                }

            return result;

        }


    }


}
