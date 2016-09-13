using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Howzit.Domains.Models
{
    public class ApplicationUserClaim : IdentityUserClaim<int> { }
}