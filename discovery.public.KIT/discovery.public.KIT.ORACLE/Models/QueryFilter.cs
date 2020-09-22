using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using discovery.KIT.ORACLE.Models;

namespace discovery.KIT.ORACLE.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonFilteringUnit
    {
        /// <summary>
        /// GUID Used in UI and storage
        /// </summary>
        public string GUID { get; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    public class QueryFilter : CommonFilteringUnit
    {
        /// <summary>
        /// 
        /// </summary>
        public string Column = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public SQLComparator Filter;
        /// <summary>
        /// 
        /// </summary>
        public string Value = string.Empty;

    }

    /// <summary>
    /// 
    /// </summary>
    public class QueryOrderBy : CommonFilteringUnit
    {
        /// <summary>
        /// 
        /// </summary>
        public string Column = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public SQLComparator Direction;
    }
}
