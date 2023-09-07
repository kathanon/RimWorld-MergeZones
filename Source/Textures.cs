using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MergeZones {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";

        public static readonly Texture2D Merge = ContentFinder<Texture2D>.Get(Prefix + "Merge");

        // Game textures
        public static readonly Texture2D Undo = ContentFinder<Texture2D>.Get("UI/Widgets/RotLeft");
    }
}
