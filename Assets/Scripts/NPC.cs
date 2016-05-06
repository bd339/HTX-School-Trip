using UnityEngine;

class NPC : Hotspot {

    new protected void OnMouseDown () {
        Camera.main.GetComponent<InvestigationControls> ().Target = transform.position;

        StartCoroutine (RunScript ());
    }

    private System.Collections.IEnumerator RunScript () {
        yield return new WaitWhile (() => Camera.main.GetComponent<InvestigationControls> ().HasTarget);

        base.OnMouseDown ();
    }
}
