using UnityEngine;
using UnityEngine.UI;

[SerializeAll]
class ScrollIndicator : MonoBehaviour {

    [DoNotSerialize]
    private static ScrollIndicator indicators;
    [DoNotSerialize]
    public static ScrollIndicator Indicators {
        get {
            if (indicators == null) {
                indicators = HierarchyManager.FindObjectOfType<ScrollIndicator> ();
            }

            return indicators;
        }
    }

    private bool upActive;
    public bool? Up {
        set {
            if (value.HasValue) {
                upActive = value.Value;
            } else if (upIndicator != null) {
                upIndicator.gameObject.SetActive (false);
            }
        }
    }

    private bool downActive;
    public bool? Down {
        set {
            if (value.HasValue) {
                downActive = value.Value;
            } else if (downIndicator != null) {
                downIndicator.gameObject.SetActive (false);
            }
        }
    }

    private bool leftActive;
    public bool? Left {
        set {
            if (value.HasValue) {
                leftActive = value.Value;
            } else if (leftIndicator != null) {
                leftIndicator.gameObject.SetActive (false);
            }
        }
    }

    private bool rightActive;
    public bool? Right {
        set {
            if (value.HasValue) {
                rightActive = value.Value;
            } else if (rightIndicator != null) {
                rightIndicator.gameObject.SetActive (false);
            }
        }
    }

    private Image upIndicator;
    private Image downIndicator;
    private Image leftIndicator;
    private Image rightIndicator;

    private Color active   = Color.white;
    private Color inactive = new Color (1f, 1f, 1f, 0.5f);

    // Use this for initialization
    void Start () {
        enabled = false;
        upIndicator    = HierarchyManager.Find ("Up", transform).GetComponent<Image> ();
        downIndicator  = HierarchyManager.Find ("Down", transform).GetComponent<Image> ();
        leftIndicator  = HierarchyManager.Find ("Left", transform).GetComponent<Image> ();
        rightIndicator = HierarchyManager.Find ("Right", transform).GetComponent<Image> ();
        StartCoroutine (Deactivate ());
    }

    private System.Collections.IEnumerator Deactivate () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        if (!Player.Data.wasDeserialized) {
            gameObject.SetActive (false);
        }

        enabled = true;
    }

    // Update is called once per frame
    void Update () {
        if (Player.Data.gamePaused || LevelSerializer.IsDeserializing) {
            return;
        }

        upIndicator.gameObject.   SetActive (true);
        downIndicator.gameObject. SetActive (true);
        leftIndicator.gameObject. SetActive (true);
        rightIndicator.gameObject.SetActive (true);

        if (upActive) {
            upIndicator.color = active;
        } else {
            upIndicator.color = inactive;
        }

        if (downActive) {
            downIndicator.color = active;
        } else {
            downIndicator.color = inactive;
        }

        if (leftActive) {
            leftIndicator.color = active;
        } else {
            leftIndicator.color = inactive;
        }

        if (rightActive) {
            rightIndicator.color = active;
        } else {
            rightIndicator.color = inactive;
        }
    }

    public void EnterInvestigationMode () {
        gameObject.SetActive (true);
    }

    public void LeaveInvestigationMode () {
        gameObject.SetActive (false);
    }
}
