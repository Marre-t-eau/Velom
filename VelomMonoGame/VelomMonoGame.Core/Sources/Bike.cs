using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelomMonoGame.Core.Sources.Tools;

namespace VelomMonoGame.Core.Sources;

internal class Bike : Object3D
{
    public Bike() : base(ModelBank.GetModel("bike"))
    {
    }

    public float Distance { get; set; }
}
