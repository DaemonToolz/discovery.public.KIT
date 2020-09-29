using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace discovery.KIT.ORACLE.Models
{
    /// <summary>
    /// 
    /// </summary>
    public static class SQLComparatorsEnum
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator GreaterThan = new SQLComparator(">");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator GreaterThanOrEqual = new SQLComparator(">=");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator LowerThan = new SQLComparator("<");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator LowerThanOrEqual = new SQLComparator("<=");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator Equal = new SQLComparator("=");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator SQLDifferent = new SQLComparator("!=");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator OracleDifferent = new SQLComparator("<>");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator LIKE = new SQLComparator("LIKE");
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SQLOrderingEnum
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator Ascendant = new SQLComparator("ASC");
        /// <summary>
        /// 
        /// </summary>
        public static readonly SQLComparator Descendant = new SQLComparator("DESC");
    }


    /// <summary>
    /// 
    /// </summary>
    public class SQLComparator {
        /// <summary>
        /// 
        /// </summary>
        public string Sign;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sign"></param>
        public SQLComparator(string sign)
        {
            Sign = sign;
        }

        /// <summary>
        /// 
        /// </summary>
        public SQLComparator()
        {

        }
    }


}
