using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MergeZones {
    public static class Strings {
        public const string ID = "kathanon.MergeZones";
        public const string Name = "Merge Zones";

        public static readonly string MergeLabel = (ID + ".MergeLabel").Translate();
        public static readonly string UndoLabel  = (ID + ".UndoLabel" ).Translate();
        public static readonly string UndoTip    = (ID + ".UndoTip"   ).Translate();

        private const string MergeTipKey = ID + ".MergeTip";

        public static string MergeTip(string name) 
            => MergeTipKey.Translate(name);
    }
}
