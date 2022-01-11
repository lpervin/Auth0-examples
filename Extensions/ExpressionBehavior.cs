using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CFSWeb.Data.Extensions
{
    internal class ExpressionBehavior
    {

        public ExpressionType ExpressionType { get; set; }
        public Boolean IsBinary { get; set; }
        public Boolean UseMethod { get; set; }
        public String Method { get; set; }

        /// <summary>
        /// When true then the following expression will apply o => o.Property.Contains(searchCriteria)
        /// When false then the following expression will apply o => searChCriteria.AsQuerable().Contains(o.Property)
        /// </summary>
        public Boolean MethodResultCompareValue { get; set; }
    }
}
