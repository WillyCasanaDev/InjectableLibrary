using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genesis.Producto.Util.Library.Options;
public class RetryPolicyOptions
{
    public int RetryCount { get; set; }
    public int SleepDurationProvider { get; set; }
}
