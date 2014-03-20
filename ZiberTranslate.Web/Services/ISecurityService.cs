using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZiberTranslate.Web.Services
{
    public interface ISecurityService
    {
        User Login(string emailAddress, string password);
    }
}
