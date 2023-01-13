using Simplisity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DNNrocketAPI.Components;

namespace RocketContentAPI.Components
{
    public class ModuleContentLimpet : ModuleBase
    {
        public ModuleContentLimpet(int portalId, string moduleRef, int moduleid = -1, int tabid = -1) : base(portalId, moduleRef, moduleid, tabid)
        {
        }
    }
}
