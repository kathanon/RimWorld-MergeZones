using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MergeZones;
public class Merge_Gizmo : Command {
    private readonly List<Zone> list;
    private readonly Zone defKeep;

    public Merge_Gizmo(ICollection<Zone> zones) {
        list = zones.ToList();
        list.SortBy(x => x.label);
        defKeep = SelectDefault(list);
        defaultLabel = Strings.MergeLabel;
        defaultDesc = Strings.MergeTip(defKeep.label);
        icon = Textures.Merge;
    }

    public override void ProcessInput(Event ev) {
        base.ProcessInput(ev);
        DoMerge(defKeep);
    }

    public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions 
        => list.Select(Option).ToList();

    private FloatMenuOption Option(Zone x) 
        => new(x.label, () => DoMerge(x), BaseContent.WhiteTex, x.color.ToTransparent(.75f));

    private void DoMerge(Zone keep) {
        var remove = list.Where(x => !ReferenceEquals(x, keep));
        Undo.Create(keep, remove);
        foreach (var zone in remove) {
            Find.Selector.Deselect(zone);
            zone.Deregister();
            zone.Cells.Do(keep.AddCell);
        }
    }

    private static Zone SelectDefault(List<Zone> zones) {
        return zones.FirstOrFallback(x => !x.label.StartsWith(x.BaseLabel), zones[0]);
    }
}
