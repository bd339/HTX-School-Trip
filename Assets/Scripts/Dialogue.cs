using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

class Dialogue : MonoBehaviour {

    private static Dialogue dialogue;
    public static Dialogue ChatboxDialogue {
        get {
            if (dialogue == null) {
                dialogue = HierarchyManager.FindObjectOfType<Dialogue> ();
            }

            return dialogue;
        }
    }

    public Text Content {
        get {
            return HierarchyManager.Find ("Content", transform).GetComponent<Text> ();
        }
    }

    public float dialogueDelay = 0.05f;

    public string DialogueColor {
        set {
            Color newColor;
            if (ColorUtility.TryParseHtmlString (value, out newColor)) {
                Content.color = newColor;
            }
        }
    }

    public string Nameplate {
        set {
            HierarchyManager.Find ("Nameplate", transform).GetComponent<Text> ().text = value;
        }
    }

    public string DialogueLine {
        set {
            Content.text = "";
            StartCoroutine (PrintDialogue (value, dialogueDelay));
        }
    }

    private int answerIndex;
    private List<Text> answers;
    private List<int> answerStateIds;
    private List<string> origAnswers;
    public Dictionary<string, int> Answers {
        set {
            var prefab = Resources.Load<GameObject> ("Answer");

            answerIndex = 0;
            answers = new List<Text> ();
            answerStateIds = new List<int> ();
            origAnswers = new List<string> ();

            foreach (var item in value.AsEnumerable ()) {
                var go = Instantiate (prefab);
                go.transform.SetParent (Content.transform, false);
                go.GetComponent<RectTransform> ().anchoredPosition -= new Vector2 (0, 30 * origAnswers.Count);
                answers.Add (go.GetComponent<Text> ());

                origAnswers.Add (item.Key);
                answerStateIds.Add (item.Value);
            }
        }
    }

    // Use this for initialization
    void Start () {
    }
    
    // Update is called once per frame
    void Update () {
        if (CooperScript.Engine.gamePaused) {
            return;
        }

        Cursor.lockState = CursorLockMode.None;

        if (origAnswers != null) {
            if (Input.GetKeyDown (KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0) {
                if (answerIndex > 0) {
                    answerIndex--;
                }
            } else if (Input.GetKeyDown (KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0) {
                if (answerIndex < answers.Count - 1) {
                    answerIndex++;
                }
            } else if (Input.GetKeyDown (KeyCode.Return) ||
                Input.GetKeyDown (KeyCode.Space) ||
                Input.GetKeyDown (KeyCode.RightArrow) ||
                Input.GetMouseButtonDown (0)) {

                Content.text = "";

                foreach (Text answer in answers) {
                    Destroy (answer.gameObject);
                }

                CooperScript.Engine.CommandIndex = 0;
                CooperScript.Engine.StateId = answerStateIds [answerIndex];
                CooperScript.Engine.stalled = false;

                origAnswers = null;
                return;
            }

            for (int i = 0; i < answers.Count; i++) {
                answers [i].color = Color.white;
                answers [i].text = origAnswers [i];
                answers [i].fontStyle = FontStyle.Normal;
                answers [i].GetComponent<Outline> ().effectColor = Color.black;
            }

            answers [answerIndex].fontStyle = FontStyle.Bold;
            answers [answerIndex].text = '>' + origAnswers [answerIndex];
            answers [answerIndex].color = new Color (226f / 255f, 18f / 255f, 219f / 255f, 255f);
            answers [answerIndex].GetComponent<Outline> ().effectColor = Color.white;
        }
    }

    private IEnumerator PrintDialogue (string line, float delay = 0.05f) {
        int idx = 0;
        int length = line.Length - line.Count (c => c == '}');
        float lastUpdate = Time.time;

        var indicator = HierarchyManager.Find ("Next Indicator").GetComponent<Image> ();

        while (Content.text.Length != length) {
            if (line [idx] == '}') {
                indicator.gameObject.SetActive (true);

                float numLines = Content.preferredWidth / 1120f;
                float x = (numLines - Mathf.Floor (numLines)) * 1120f + 0.15f * 100 + Mathf.Floor (numLines) * 10f;
                float y = -Mathf.Floor (numLines) * 20f - 10f;
                indicator.rectTransform.anchoredPosition = new Vector2 (x, y);

                yield return new WaitWhile (() => !ContinueDialogue ());

                indicator.gameObject.SetActive (false);
                idx++;

                yield return null;
            } else if (ContinueDialogue ()) {
                var rem = line.Substring (idx);
                int newIdx = rem.IndexOf ('}');

                if (newIdx == -1) {
                    Content.text += rem;
                } else {
                    Content.text += rem.Substring (0, newIdx);
                    idx += rem.Substring (0, newIdx).Length;
                }

                yield return null;
            } else {
                if (Time.time - lastUpdate < delay) {
                    yield return null;
                    continue;
                }

                lastUpdate = Time.time;
                Content.text += line [idx];
                idx++;
            }
        }

        indicator.gameObject.SetActive (true);
        indicator.rectTransform.anchoredPosition = new Vector2 (-15, -5);

        yield return new WaitWhile (() => !ContinueDialogue ());

        indicator.gameObject.SetActive (false);
        CooperScript.Engine.stalled = false;
    }

    private bool ContinueDialogue () {
        return Input.GetKeyDown (KeyCode.Space) ||
               Input.GetKeyDown (KeyCode.Return) ||
               Input.GetKeyDown (KeyCode.RightArrow) ||
               Input.GetMouseButtonDown (0);
    }

    private IEnumerator HideDialogueUI () {
        var rect = HierarchyManager.Find ("Chatbox").GetComponent<RectTransform> ();

        var startTime = Time.time;

        while (rect.anchoredPosition.y > -300) {
            if (CooperScript.Engine.gamePaused) {
                yield return null;
            } else {
                rect.anchoredPosition = new Vector2 (0, Mathf.SmoothStep (20, -300, (Time.time - startTime) / 0.125f));
                yield return null;
            }
        }

        gameObject.SetActive (false);
    }

    private IEnumerator ShowDialogueUI () {
        var rect = HierarchyManager.Find ("Chatbox").GetComponent<RectTransform> ();

        var startTime = Time.time;

        while (rect.anchoredPosition.y < 20) {
            if (CooperScript.Engine.gamePaused) {
                yield return null;
            } else {
                rect.anchoredPosition = new Vector2 (0, Mathf.SmoothStep (-300, 20, (Time.time - startTime) / 0.125f));
                yield return null;
            }
        }
    }

    public void EnterDialogueMode () {
        StopCoroutine ("LeaveDialogueMode");
        gameObject.SetActive (true);
        StartCoroutine (ShowDialogueUI ());
    }

    public void LeaveDialogueMode () {
        if (gameObject.activeInHierarchy) {
            StopCoroutine ("EnterDialogueMode");
            StartCoroutine (HideDialogueUI ());
        }
    }
}
