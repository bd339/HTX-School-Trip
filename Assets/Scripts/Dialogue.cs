using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

[SerializeAll]
class Dialogue : MonoBehaviour {

    [DoNotSerialize]
    private static Dialogue dialogue;
    [DoNotSerialize]
    public static Dialogue ChatboxDialogue {
        get {
            if (dialogue == null) {
                dialogue = HierarchyManager.FindObjectOfType<Dialogue> ();
            }

            return dialogue;
        }
    }

    public float dialogueDelay = 0.05f;

    public string DialogueColor {
        set {
            Color newColor;
            if (ColorUtility.TryParseHtmlString (value, out newColor)) {
                chatboxContent.color = newColor;
            }
        }
    }

    public string Nameplate {
        set {
            HierarchyManager.Find ("Nameplate", transform).GetComponent<Text> ().text = value;
        }
    }

    private int dialogueIndex;
    private int dialogueLength;
    private string dialogueLine;
    [DoNotSerialize]
    private float lastDialogueUpdate;
    public string DialogueLine {
        set {
            chatboxContent.text = "";

            dialogueIndex = 0;
            dialogueLength = value.Length - value.Count (c => c == '}');
            dialogueLine = value;
            lastDialogueUpdate = Time.time;
        }
    }

    public string Question {
        set {
            chatboxContent.text = value;
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
                go.SetParent (chatboxContent.gameObject);
                go.GetComponent<RectTransform> ().anchoredPosition -= new Vector2 (0, 30 * origAnswers.Count);
                answers.Add (go.GetComponent<Text> ());

                origAnswers.Add (item.Key);
                answerStateIds.Add (item.Value);
            }
        }
    }

    [DoNotSerialize]
    private Text chatboxContent;

    // Use this for initialization
    void Start () {
        chatboxContent = HierarchyManager.Find ("Content", transform).GetComponent<Text> ();
        StartCoroutine (DisableDialogueUI ());
    }

    private IEnumerator DisableDialogueUI () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        if (!Player.Data.wasDeserialized) {
            HierarchyManager.Find ("Chatbox").GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, -300);
            gameObject.SetActive (false);
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (Player.Data.gamePaused || LevelSerializer.IsDeserializing) {
            return;
        }

        Cursor.lockState = CursorLockMode.None;

        if (dialogueLine != null) {
            if (chatboxContent.text.Length == dialogueLength) {
                if (Input.GetKeyDown (KeyCode.Space) ||
                    Input.GetKeyDown (KeyCode.Return)||
                    Input.GetKeyDown (KeyCode.RightArrow)||
                    Input.GetMouseButtonDown (0)) {

                    HierarchyManager.Find ("Next Indicator").SetActive (false);

                    dialogueLine = null;
                    CooperScript.Engine.stalled = false;
                } else {
                    var indicator = HierarchyManager.Find ("Next Indicator").GetComponent<Image> ();
                    if (!indicator.gameObject.activeInHierarchy) {
                        indicator.gameObject.SetActive (true);
                        indicator.rectTransform.anchoredPosition = new Vector2 (-15, -5);
                    }
                }
            } else if (dialogueLine [dialogueIndex] == '}') {
                if (Input.GetKeyDown (KeyCode.Space) ||
                    Input.GetKeyDown (KeyCode.Return)||
                    Input.GetKeyDown (KeyCode.RightArrow)||
                    Input.GetMouseButtonDown (0)) {

                    HierarchyManager.Find ("Next Indicator").SetActive (false);

                    dialogueIndex++;
                } else {
                    var indicator = HierarchyManager.Find ("Next Indicator").GetComponent<Image> ();
                    if (!indicator.gameObject.activeInHierarchy) {
                        indicator.gameObject.SetActive (true);
                        float numLines = chatboxContent.preferredWidth / 1120f;
                        float x = (numLines - Mathf.Floor (numLines)) * 1120f + 0.15f * 100 + Mathf.Floor (numLines) * 10f;
                        float y = -Mathf.Floor (numLines) * 20f - 10f;
                        indicator.rectTransform.anchoredPosition = new Vector2 (x, y);
                    }
                }
            } else {
                if (Input.GetKeyDown (KeyCode.Space) ||
                    Input.GetKeyDown (KeyCode.Return)||
                    Input.GetKeyDown (KeyCode.RightArrow)||
                    Input.GetMouseButtonDown (0)) {

                    var rem = dialogueLine.Substring (dialogueIndex);
                    int newIndex = rem.IndexOf ('}');

                    if (newIndex == -1) {
                        chatboxContent.text += rem;
                    } else {
                        chatboxContent.text += rem.Substring (0, newIndex);
                        dialogueIndex += rem.Substring (0, newIndex).Length;
                    }
                } else if (Time.time - lastDialogueUpdate >= dialogueDelay) {
                    lastDialogueUpdate = Time.time;
                    chatboxContent.text += dialogueLine [dialogueIndex];
                    dialogueIndex++;
                }
            }
        }

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

                chatboxContent.text = "";

                foreach (Text answer in answers) {
                    Destroy (answer);
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

    private IEnumerator HideDialogueUI () {
        var rect = HierarchyManager.Find ("Chatbox").GetComponent<RectTransform> ();
        var startTime = Time.time;

        while (rect.anchoredPosition.y > -300) {
            if (Player.Data.gamePaused) {
                rect.anchoredPosition = new Vector2 (0, -300);
            } else {
                rect.anchoredPosition = new Vector2 (0, Mathf.SmoothStep (20, -300, (Time.time - startTime) / 0.125f));
                yield return null;
            }
        }

        gameObject.SetActive (false);
    }

    private IEnumerator ShowDialogueUI () {
        chatboxContent.text = "";
        var rect = HierarchyManager.Find ("Chatbox").GetComponent<RectTransform> ();
        var startTime = Time.time;

        while (rect.anchoredPosition.y < 20) {
            if (Player.Data.gamePaused) {
                rect.anchoredPosition = new Vector2 (0, 20);
            } else {
                rect.anchoredPosition = new Vector2 (0, Mathf.SmoothStep (-300, 20, (Time.time - startTime) / 0.125f));
                yield return null;
            }
        }
    }

    public void EnterDialogueMode () {
        StopAllCoroutines ();
        gameObject.SetActive (true);
        StartCoroutine (ShowDialogueUI ());
    }

    public void LeaveDialogueMode () {
        if (gameObject.activeInHierarchy) {
            StopAllCoroutines ();
            StartCoroutine (HideDialogueUI ());
        }
    }
}
