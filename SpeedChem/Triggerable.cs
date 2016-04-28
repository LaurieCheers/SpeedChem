using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    interface Triggerable
    {
        void Trigger();
        void Update(List<WorldObject> objects);
    }
}
