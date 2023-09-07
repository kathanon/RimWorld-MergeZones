using HarmonyLib;
using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MergeZones;
public class Undo {
    private static readonly ConditionalWeakTable<Zone, Slot> table = new();
    private static readonly ConditionalWeakTable<Zone, Zone> mergedInto = new();

    private readonly Zone zone;
    private readonly List<RemovedZone> removed;
    private readonly List<ResetEntry> resetList;

    public static Undo For(Zone zone) 
        => table.GetOrCreateValue(zone).Undo;

    public static Zone MergedInto(Zone zone)
        => mergedInto.TryGetValue(zone, out var target) ? target : null;

    public static void Create(Zone keep, IEnumerable<Zone> remove) 
        => table.GetOrCreateValue(keep).Undo = new(keep, remove);

    public static void UpdateBillZones(Bill_Production bill) {
        if (bill == BillUtility.Clipboard) return;

        var store = bill.GetStoreZone();
        if (mergedInto.TryGetValue(store, out var target) && target is Zone_Stockpile targetStore) {
            For(targetStore)?.resetList.Add(new BillResetEntry(bill, targetStore, store, true));
            bill.SetStoreMode(bill.GetStoreMode(), targetStore);
        }

        if (mergedInto.TryGetValue(bill.includeFromZone, out var target2) && target2 is Zone_Stockpile targetFrom) {
            For(targetFrom)?.resetList.Add(new BillResetEntry(bill, targetFrom, bill.includeFromZone, false));
            bill.includeFromZone = targetFrom;
        }
    }

    private Undo(Zone keep, IEnumerable<Zone> remove) {
        zone = keep;
        removed = remove.Select(RemovedZone.Create).ToList();
        resetList = new();
        foreach (var item in remove) {
            mergedInto.Add(item, keep);
        }
    }

    public void Apply() {
        foreach (var z in removed) {
            z.Recreate(zone);
        }
        foreach (var item in resetList) {
            item.Reset();
        }
        table.GetOrCreateValue(zone).Undo = null;
    }

    private class Slot {
        private Undo undo;
        private float created;

        public Undo Undo {
            get {
                if (Time.realtimeSinceStartup - created > 900f) {
                    undo = null;
                }
                return undo;
            }

            set {
                created = Time.realtimeSinceStartup;
                undo = value;
            }
        }
    }

    private abstract class ResetEntry {
        public abstract void Reset();
    }

    private class BillResetEntry : ResetEntry {
        private readonly Bill_Production bill;
        private readonly Zone_Stockpile to;
        private readonly Zone_Stockpile from;
        private readonly bool isStore;

        public BillResetEntry(Bill_Production bill, Zone_Stockpile from, Zone_Stockpile to, bool isStore) {
            this.bill = bill;
            this.from = from;
            this.to = to;
            this.isStore = isStore;
        }

        public override void Reset() {
            if (isStore) {
                if (bill.GetStoreZone() == from) {
                    bill.SetStoreMode(bill.GetStoreMode(), to);
                }
            } else {
                if (bill.includeFromZone == from) {
                    bill.includeFromZone = to;
                }
            }
        }
    }

    private abstract class RemovedZone {
        private readonly string label;
        private readonly Color color;
        private readonly List<IntVec3> cells;

        protected RemovedZone(Zone zone) {
            label = zone.label;
            color = zone.color;
            cells = zone.Cells.ToList();
        }

        public void Recreate(Zone keep) {
            cells.Do(keep.RemoveCell);
            var manager = keep.zoneManager;
            var zone = Recreate(manager);
            if (!manager.AllZones.Any(x => x.label == label)) {
                zone.label = label;
            }
            zone.color = color;
            manager.RegisterZone(zone);
            cells.Do(zone.AddCell);
            Find.Selector.Select(zone);
        }

        protected abstract Zone Recreate(ZoneManager manager);

        public static RemovedZone Create(Zone zone) {
            if (zone is Zone_Stockpile stock) {
                return new RemovedStockpile(stock);
            } else if (zone is Zone_Growing grow) {
                return new RemovedGrowing(grow);
            } else {
                return null;
            }
        }

        private class RemovedStockpile : RemovedZone {
            private readonly StorageSettings settings;
            private readonly StorageSettingsPreset preset;

            private static readonly string dumpingBaseLabel 
                = StorageSettingsPreset.DumpingStockpile.PresetName();

            public RemovedStockpile(Zone_Stockpile zone) : base(zone) {
                settings = zone.settings;
                preset = (zone.BaseLabel == dumpingBaseLabel) 
                    ? StorageSettingsPreset.DumpingStockpile 
                    : StorageSettingsPreset.DefaultStockpile;
            }

            protected override Zone Recreate(ZoneManager manager) 
                => new Zone_Stockpile(preset, manager) { settings = settings };
        }

        private class RemovedGrowing : RemovedZone {
            private readonly ThingDef plant;

            public RemovedGrowing(Zone_Growing zone) : base(zone) {
                plant = zone.PlantDefToGrow;
            }

            protected override Zone Recreate(ZoneManager manager) {
                var zone = new Zone_Growing(manager);
                zone.SetPlantDefToGrow(plant);
                return zone;
            }
        }
    }
}
