using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorbSoftDev.SOW
{
    internal abstract class TemporaryEchelonTable<T> : Dictionary<long, T> where T : IEchelon
    {
        protected T root;
        protected static int NUMBER_OF_SIDES = 2;


        public TemporaryEchelonTable(T root)
        {
            this.root = root;
            //prepopulate the table with existing roots

            PopulateWith(root);

        }

        abstract protected void PopulateWith(T echelon);


        abstract public T ConjureEchelon(long echelonId);

    }

    internal class TemporaryScenarioEchelonTable : TemporaryEchelonTable<ScenarioEchelon>
    {


        public TemporaryScenarioEchelonTable(ScenarioEchelon root) : base(root) { }

        protected override void PopulateWith(ScenarioEchelon echelon)
        {

            // create default sides, if they don't exist
            // this must happen first to prevent changes to root while iterating
            for (int i = root.children.Count; i < NUMBER_OF_SIDES; i++)
            {
                ConjureEchelon(EchelonHelper.ComposeEchelonId(i + 1, 0, 0, 0, 0, 0));
            }

            this[echelon.id] = echelon;
            foreach (ScenarioEchelon child in echelon)
            {
                PopulateWith(child as ScenarioEchelon);
            }


        }

        public override ScenarioEchelon ConjureEchelon(long echelonId)
        {
            ScenarioEchelon echelon;

            if (TryGetValue(echelonId, out echelon))
            {
                if (echelonId != echelon.id)
                {
                    Log.Error(this, "Scenario requested Echelon ID " + echelonId + " but got " + echelon.id);
                }
                return echelon;
            }

            echelon = new ScenarioEchelon();
            // Log.Info(this,"[EchelonDictionary] Created "+ERank+" "+echelonId);

            if (EchelonHelper.RankOf(echelonId) == ERank.Side)
            {
                echelon.parent = root;
            }
            else
            {
                // find or create parents if needed
                echelon.parent = ConjureEchelon(EchelonHelper.ParentEchelonId(echelonId));
            }

            this[echelonId] = echelon;
            return echelon;
        }


    }

    internal class TemporaryOOBEchelonTable : TemporaryEchelonTable<OOBEchelon>
    {

        public TemporaryOOBEchelonTable(OOBEchelon root) : base(root) { }

        protected override void PopulateWith(OOBEchelon echelon)
        {
            this[echelon.id] = echelon;
            foreach (OOBEchelon child in echelon)
            {
                PopulateWith(child as OOBEchelon);
            }
        }

        public override OOBEchelon ConjureEchelon(long echelonId)
        {
            OOBEchelon echelon;

            if (TryGetValue(echelonId, out echelon))
            {
                // return if it exists
                return echelon;
            }

            echelon = new OOBEchelon();
            // Log.Info(this,"[EchelonDictionary] Created "+ERank+" "+echelonId);

            if (EchelonHelper.RankOf(echelonId) == ERank.Side)
            {
                echelon.parent = root;

            }
            else
            {
                // find or create parents if needed
                echelon.parent = ConjureEchelon(EchelonHelper.ParentEchelonId(echelonId));
            }

            this[echelonId] = echelon;
            return echelon;
        }
    }
}
