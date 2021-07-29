using Celeste.Mod.EeveeHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers {
    public interface IEntityHandler {
        Entity Entity { get; }
        EntityContainer Container { get; set; }

        void OnAttach(EntityContainer container);

        void OnDetach(EntityContainer container);

        bool IsInside(EntityContainer container);

        Rectangle GetBounds();

        void Destroy();
    }
}
