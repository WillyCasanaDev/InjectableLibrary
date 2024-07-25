using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genesis.Producto.Util.Library.Authorization.Azure;

namespace Genesis.Producto.Util.Library.Interfaces;
public interface IAzureAuthorizationServiceFactory
{
    AzureAuthorizationService Create(string clientName);
}
