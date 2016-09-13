using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Howzit.Domains.Contracts
{
    public interface IHttpContextBaseRepository
    {
        HttpContextBase HttpContext { get; }
    }
}
