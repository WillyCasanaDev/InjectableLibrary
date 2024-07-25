using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Genesis.Producto.Util.Library.Options;
public class ExternalHttpRequestClientOptions
{
    public bool EnableSSLByPass { get; set; } = false;
    public int TimeoutInSeconds { get; set; } = 200;
}
