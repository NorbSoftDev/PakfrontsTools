using System;
using System.Collections.Generic;
using System.Windows;
// using System.Windows.Media;
using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

namespace NorbSoftDev.SOW.Utils
{
    public static class UnitTools
    {
        public static void ApplyFormation(this Map map, ScenarioEchelon echelon)
        {

             // ScaleTransform yardsToWorldTransform = new ScaleTransform(map.unitPerYard, map.unitPerYard);
            // GeneralTransform yardsToWorldTransform = new GeneralTransform();

            Formation formation = echelon.unit.formation;
            if (formation == null) return;

            Formation.Location[] formationLocations;
            // NB this will not apply the subformations, that is done below
            formation.BridageComputeChildPositionsYds(echelon, out formationLocations);

            // Leaves
            for (int childIndex = 0; childIndex < echelon.children.Count; childIndex++)
            {
                ScenarioEchelon child = (ScenarioEchelon)echelon.children[childIndex];

                int pIndex = childIndex + 2;
                if (pIndex >= formationLocations.Length)
                {
                   string messageBoxText = "Unable to move \""+child.unit.id + "\" into position in formation \""+formation.id+"\" as there is no slot "+pIndex+" for it in formation of size "+formationLocations.Length;
                   Log.Error(child, messageBoxText);
                    continue;
                }

                Point p = formationLocations[childIndex + 2].position;
                Console.WriteLine("Map:"+map+" p:"+p);
                //p = yardsToWorldTransform.Transform(p);
                p.X *= map.unitPerYard;
                p.Y *= map.unitPerYard;
                p = echelon.unit.transform.GetWorldMatrix().Transform(p);

                Vector dir = formationLocations[childIndex + 2].direction;
                dir = echelon.unit.transform.GetRotationMatrix().Transform(dir);

                Formation childFormation = formationLocations[childIndex + 2].formation;

                if (child.unit.formation != null && childFormation.level != child.unit.formation.level)
                {
                    Log.Warn(child.unit, String.Format("May not change formation level {0} from {1} ({2}) to {3} ({4})",
                        child.unit.id, child.unit.formation.level, child.unit.formation.id, childFormation.level, childFormation.id));
                }
                else
                {
                    //Console.WriteLine("Changin Child" + child.id + " from " + child.unit.formation.id + " to " + childFormation);
                    child.unit.formation = childFormation;
                }

                if (child.unit.transform == null) child.unit.transform = new WorldTransform();

                child.unit.transform.Set(
                    //(float)dir.Y, (float) dir.X,
                     (float)dir.Y, (float)dir.X,
                     (float)p.Y, (float)p.X
                     );

                if (child.unit.formation != null && child.unit.formation.level > 0)
                {
                    ApplyFormation(map, child);
                }
            }
        }



    }
}
