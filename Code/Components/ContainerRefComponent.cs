using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;

namespace Celeste.Mod.EeveeHelper.Components {
    /// <summary>
    /// Container Reference Component. When an Entity is added to an EntityContainer, this can be used to retrieve the Containers from the Entity directly.
    /// </summary>
    public class ContainerRefComponent : Component {
        public List<IContainer> containers;

        public ContainerRefComponent(IContainer container) : base(false, false) {
            containers = new List<IContainer>();
            containers.Add(container);
        }


    }

    public static class ContainerRefComponentExtension {
        public static void AddOrAppendContainer(this Entity self, IContainer container) {
            var q = self.Get<ContainerRefComponent>();
            if (q == null)
                self.Add(new ContainerRefComponent(container));
            else
                q.containers.Add(container);
        }
    }
}
