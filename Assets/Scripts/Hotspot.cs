using UnityEngine;

[RequireComponent (typeof (BoxCollider), typeof (SpriteRenderer))]
[DoNotSerializePublic]
class Hotspot : MonoBehaviour {

    [TextArea (0, 100)]
    public string onClickScript;

    public Texture2D hoverCursor;

    public int minState = 0;
    public int maxState = int.MaxValue;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
    }

    protected void OnMouseDown () {
        // remove help
    }

    void OnMouseEnter () {
        Cursor.SetCursor (hoverCursor, Vector2.zero, CursorMode.Auto);
    }

    void OnMouseExit () {
        Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
    }

    public static void OnSecondaryScriptComplete () {
        Dialogue.ChatboxDialogue.LeaveDialogueMode ();
        // leave court mode
        InvestigationControls.Controls.EnterInvestigationMode ();
        ScrollIndicator.Indicators.EnterInvestigationMode ();
    }
}
