using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MergeZones;
public class Undo_Gizmo : Command {
    private readonly Undo undo;

    public Undo_Gizmo(Undo undo) {
        this.undo = undo;
        defaultLabel = Strings.UndoLabel;
        defaultDesc = Strings.UndoTip;
        icon = Textures.Merge;
    }

    public override void ProcessInput(Event ev) {
        base.ProcessInput(ev);
        undo.Apply();
    }

    protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms) {
        var result = base.GizmoOnGUIInt(butRect, parms);
        GUI.color = ColorLibrary.Teal;
        var overlay = butRect.ContractedBy(4f).TopHalf().RightHalf();
        Widgets.DrawTextureFitted(overlay, Textures.Undo, 1f);
        GUI.color = Color.white;
        return result;
    }
}
