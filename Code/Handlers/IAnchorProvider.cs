using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers {
    public interface IAnchorProvider {
        List<string> GetAnchors();
    }
}
