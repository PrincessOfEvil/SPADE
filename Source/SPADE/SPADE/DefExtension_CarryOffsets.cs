using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace SPADE
    {
    public class DefExtension_CarriesThingsOffset : DefModExtension
        {
        public ThingOffsets offsets;
        public DefExtension_CarriesThingsOffset() { }
        }
    public class DefExtension_CarriesWeaponStraight : DefModExtension
        {
        public ThingOffsets offsets;
        public DefExtension_CarriesWeaponStraight() {}
        }
    public class DefExtension_CarriesWeaponOpenly : DefModExtension
        {
        public DefExtension_CarriesWeaponOpenly() { }
        }

    public class ThingOffsets
        {
        public Vector3 north = Vector3.zero;
        public Vector3 south = Vector3.zero;
        public Vector3 west = Vector3.zero;
        public Vector3 east = Vector3.zero;

        public Vector3 GetFor(Rot4 rot) 
            {
            if (rot == Rot4.North) return north;
            if (rot == Rot4.South) return south;
            if (rot == Rot4.West) return west;
            if (rot == Rot4.East) return east;
            return Vector3.zero;
            }
        }
    }
