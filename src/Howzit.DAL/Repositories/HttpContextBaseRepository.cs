using Howzit.Domains.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Howzit.DAL.Repositories
{
    public class HttpContextBaseRepository : IHttpContextBaseRepository
    {
        private HttpContextBase httpContextBase;

        public HttpContextBaseRepository(HttpContextBase httpContextBase)
        {
            this.httpContextBase = httpContextBase;
        }

        #region IHttpContextBaseRepository Members

        public HttpContextBase HttpContext
        {
            get { return httpContextBase;  }
        }

        #endregion
    }
}
