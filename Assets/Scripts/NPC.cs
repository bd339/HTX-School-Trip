using UnityEngine;

class NPC : Hotspot {

    new protected void OnMouseDown () {
        Camera.main.GetComponent<InvestigationControls> ().Target = transform.position;

        foreach (var h in FindObjectsOfType<Hotspot> ()) {
            h.GetComponent<BoxCollider> ().enabled = false;
        }

        CooperScript.Engine.stalled = true;
        CooperScript.Engine.AddSecondaryScript (onClickScript);

        InvestigationControls.Controls.LeaveInvestigationMode ();
        ScrollIndicator.Indicators.LeaveInvestigationMode ();
        Dialogue.ChatboxDialogue.LeaveDialogueMode ();
        // leave court mode

        StartCoroutine (RunScript ());
    }

    private System.Collections.IEnumerator RunScript () {
        yield return new WaitWhile (() => Camera.main.GetComponent<InvestigationControls> ().HasTarget);

        CooperScript.Engine.stalled = false;
        base.OnMouseDown ();
    }
}
