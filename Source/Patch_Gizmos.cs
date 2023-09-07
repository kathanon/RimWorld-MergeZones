using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace MergeZones;
[HarmonyPatch]
public static class Patch_Gizmos {
    private static Zone createdFor = null;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Zone), nameof(Zone.GetGizmos))]
    public static IEnumerable<Gizmo> GetGizmos(IEnumerable<Gizmo> __result, Zone __instance) {
        foreach (Gizmo gizmo in __result) {
            yield return gizmo;
        }

        var undo = Undo.For(__instance);
        if (undo != null) {
            yield return new Undo_Gizmo(undo);
        }

        if (ReferenceEquals(createdFor, __instance)) createdFor = null;

        var type = __instance.GetType();
        var sel = Find.Selector.SelectedObjects;
        var zones = sel
            .OfType<Zone>()
            .Where(x => x.GetType() == type && !ReferenceEquals(x, createdFor))
            .ToList();
        if (zones.Count < sel.Count || zones.Count < 2) yield break;

        var connected = new HashSet<Zone>() ;
        bool found;
        List<Zone> next = new(), last = new() { 
            __instance 
        };
        do {
            connected.AddRange(last);
            zones.RemoveAll(connected.Contains);
            next.AddRange(zones
                .Where(a => last.Any(b => Connected(a, b))));
            found = next.Count > 0;
            last.Clear();
            (last, next) = (next, last);
        } while (found);
        
        if (zones.Count > 0) yield break;

        connected.AddRange(last);
        createdFor = __instance;
        yield return new Merge_Gizmo(connected);
    }

    private static bool Connected(Zone a, Zone b) {
        var intersection = BoundingRect(a).ClipInsideRect(BoundingRect(b));
        var aset = a.Cells.Where(intersection.Contains).ToHashSet();
        var bset = b.Cells.Where(intersection.Contains).ToHashSet();
        bool asmall = aset.Count < bset.Count;
        var small = asmall ? aset : bset;
        var large = asmall ? bset : aset;
        foreach (var c in small) {
            for (int i = 0; i < 4; i++) {
                if (large.Contains(c + GenAdj.CardinalDirections[i])) {
                    return true;
                }
            }
        }
        return false;
    }

    private static CellRect BoundingRect(Zone zone) {
        var cells = zone.Cells;
        if (cells.Count == 0) return default;
        var result = CellRect.SingleCell(cells[0]);
        for (int i = 1; i < cells.Count; i++) {
            result.Encapsulate(cells[i]);
        }
        return result.ExpandedBy(1);
    }
}

public static class CellRectExtension {
    public static void Encapsulate(this ref CellRect rect, IntVec3 cell) {
        if (rect.minX > cell.x) {
            rect.minX = cell.x;
        } else if (rect.maxX < cell.x) {
            rect.maxX = cell.x;
        }

        if (rect.minZ > cell.z) {
            rect.minZ = cell.z;
        } else if (rect.maxZ < cell.z) {
            rect.maxZ = cell.z;
        }
    }
}

