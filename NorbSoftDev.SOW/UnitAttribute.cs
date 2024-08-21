using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorbSoftDev.SOW
{
    public class UnitAttribute
    {
        public int value { get; set; }
        public UnitAttribute(int value)
        {
            this.value = value;
        }


        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class UnitState : UnitAttribute
    {
        public UnitState(int value) : base(value) { }
    }
}
