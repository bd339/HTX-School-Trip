using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

class CooperScript : MonoBehaviour {

    private static CooperScript engine;
    public static CooperScript Engine {
        get {
            if (engine == null) {
                engine = HierarchyManager.FindObjectOfType<CooperScript> ();
            }

            return engine;
        }
    }

    public bool gamePaused;

    public TextAsset mainScriptFile;

    public bool stalled;

    private struct State {
        public string nameplate;
        public List<string> commands;
    }

    private struct Script {
        public int initialStateId;
        public Dictionary<int, State> states;
    }

    private int primaryStateId;
    private int primaryCommandIndex;
    private Dictionary<int, State> primaryStates;

    private int secondaryStateId;
    private int secondaryCommandIndex;
    private Dictionary<int, State> secondaryStates;

    public int StateId {
        get {
            return secondaryStates == null ? primaryStateId : secondaryStateId;
        }

        set {
            if (secondaryStates == null) {
                primaryStateId = value;
                UpdateHotspots ();
            } else {
                secondaryStateId = value;
            }
        }
    }
    public int CommandIndex {
        get {
            return secondaryStates == null ? primaryCommandIndex : secondaryCommandIndex;
        }

        set {
            if (secondaryStates == null) {
                primaryCommandIndex = value;
            } else {
                secondaryCommandIndex = value;
            }
        }
    }
    private Dictionary<int, State> States {
        get {
            return secondaryStates == null ? primaryStates : secondaryStates;
        }
    }

    private List<Image> fadingSprites = new List<Image> ();

    // Use this for initialization
    void Start () {
        var newScript = AddScript (mainScriptFile.text);
        primaryStateId = newScript.initialStateId;
        primaryCommandIndex = 0;
        primaryStates = newScript.states;

        UpdateHotspots ();
    }

    private Script AddScript (string script) {
        var newScript = new Script ();
        var newStates = new Dictionary<int, State> ();

        using (var sr = new System.IO.StringReader (script)) {
            int i = 0;
            string line;
            while ((line = sr.ReadLine ()) != null) {
                if (line.StartsWith ("\\")) {
                    int stateId = int.Parse (line.Substring (1));
                    if (i == 0) {
                        newScript.initialStateId = stateId;
                        i++;
                    }

                    var newState = new State ();
                    var newCommands = new List<string> ();
                    
                    // dummy command to ensure that all states have at least 1 command
                    newCommands.Add ("'dummy'");

                    newState.nameplate = sr.ReadLine ().Replace ("$PlayerName", PlayerPrefs.GetString ("PlayerName"));

                    while (sr.Peek () != '\\' && sr.Peek () != -1) {
                        var command = sr.ReadLine ();
                        // skip empty lines and comments
                        if (command.Length > 0 && !command.StartsWith (";")) {
                            newCommands.Add (command);
                        }
                    }

                    newState.commands = newCommands;
                    newStates.Add (stateId, newState);
                }
            }

            newScript.states = newStates;
        }

        return newScript;
    }

    public void AddSecondaryScript (Hotspot provider) {
        var newScript = AddScript (provider.onClickScript);
        secondaryStateId = newScript.initialStateId;
        secondaryCommandIndex = 0;
        secondaryStates = newScript.states;
    }

    // Update is called once per frame
    void Update () {
        if (gamePaused || stalled) {
            return;
        }

        State currentState;
        if (!States.TryGetValue (StateId, out currentState)) {
            return;
        }
        var command = currentState.commands [CommandIndex];
        string cmdName = null;

        if (command.StartsWith ("'")) {
            IEnumerator<string> i = Regex.Split (command, "' '").Select (c => c.Replace ("'", "")).GetEnumerator ();
            i.MoveNext ();
            cmdName = i.Current;

            if (cmdName == "wait") {
                i.MoveNext ();

                stalled = true;
                StartCoroutine ("StallForSeconds", float.Parse (i.Current));
            } else if (cmdName == "mode") {
                i.MoveNext ();

                if (i.Current == "investigation") {
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // leave court mode
                    InvestigationControls.Controls.EnterInvestigationMode ();
                } else if (i.Current == "dialogue") {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    // leave court mode
                    Dialogue.ChatboxDialogue.EnterDialogueMode ();
                } else if (i.Current == "court") {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // enter court mode
                } else {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // leave court mode
                }
            } else if (cmdName == "state") {
                i.MoveNext ();

                CommandIndex = -1;
                StateId = int.Parse (i.Current);
            } else if (cmdName == "global state") {
                i.MoveNext ();

                if (secondaryStates == null) {
                    primaryCommandIndex = -1;
                } else {
                    primaryCommandIndex = 0;
                }

                primaryStateId = int.Parse (i.Current);
                UpdateHotspots ();
            } else if (cmdName == "fade in") {
                i.MoveNext ();
                var spriteName = i.Current;
                var uiSprite = HierarchyManager.Find (spriteName, HierarchyManager.Find ("Characters").transform);

                if (uiSprite != null) {
                    if (i.MoveNext ()) {
                        StartCoroutine (FadeInSprite (uiSprite.GetComponent<Image> (), float.Parse (i.Current)));
                    } else {
                        StartCoroutine (FadeInSprite (uiSprite.GetComponent<Image> ()));
                    }
                } else {
                    uiSprite = HierarchyManager.Find (spriteName);

                    if (uiSprite != null) {
                        if (i.MoveNext ()) {
                            StartCoroutine (FadeInSprite (uiSprite.GetComponent<Image> (), float.Parse (i.Current)));
                        } else {
                            StartCoroutine (FadeInSprite (uiSprite.GetComponent<Image> ()));
                        }
                    } else {
                        var prefab = ResourceManager.Load<Image> (spriteName);
                        if (prefab == null) {
                            Debug.Log ("There's no way to fade in " + spriteName);
                            return;
                        }

                        var go = Instantiate (prefab);
                        go.name = spriteName;
                        go.transform.SetParent (HierarchyManager.Find ("Characters").transform, false);

                        if (i.MoveNext ()) {
                            StartCoroutine (FadeInSprite (go, float.Parse (i.Current)));
                        } else {
                            StartCoroutine (FadeInSprite (go));
                        }
                    }
                }
            } else if (cmdName == "fade out") {
                i.MoveNext ();
                var spriteName = i.Current;
                var uiSprite = HierarchyManager.Find (spriteName, HierarchyManager.Find("Characters").transform);

                if (uiSprite != null) {
                    if (i.MoveNext ()) {
                        StartCoroutine (FadeOutSprite (uiSprite.GetComponent<Image> (), float.Parse (i.Current)));
                    } else {
                        StartCoroutine (FadeOutSprite (uiSprite.GetComponent<Image> ()));
                    }
                } else {
                    uiSprite = HierarchyManager.Find (spriteName);
                    if (i.MoveNext ()) {
                        StartCoroutine (FadeOutSprite (uiSprite.GetComponent<Image> (), float.Parse (i.Current)));
                    } else {
                        StartCoroutine (FadeOutSprite (uiSprite.GetComponent<Image> ()));
                    }
                }
            } else if (cmdName == "text color") {
                i.MoveNext ();
                Dialogue.ChatboxDialogue.DialogueColor = i.Current;
            } else if (cmdName == "text delay") {
                i.MoveNext ();
                Dialogue.ChatboxDialogue.dialogueDelay = float.Parse (i.Current);
            } else if (cmdName == "panorama") {
                i.MoveNext ();

                if (!HierarchyManager.Find ("Panorama").activeInHierarchy) {
                    HierarchyManager.Find ("Flat").SetActive (false);
                    HierarchyManager.Find ("Panorama").SetActive (true);
                }

                InvestigationControls.Controls.BackgroundMesh = HierarchyManager.Find ("Panorama").GetComponent<MeshRenderer> ();
                InvestigationControls.Controls.BackgroundTex = ResourceManager.Load<Texture2D> (i.Current);
            } else if (cmdName == "flat") {
                i.MoveNext ();

                if (!HierarchyManager.Find ("Flat").activeInHierarchy) {
                    HierarchyManager.Find ("Panorama").SetActive (false);
                    HierarchyManager.Find ("Flat").SetActive (true);
                }

                InvestigationControls.Controls.BackgroundMesh = HierarchyManager.Find ("Flat").GetComponent<MeshRenderer> ();
                InvestigationControls.Controls.BackgroundTex = ResourceManager.Load<Texture2D> (i.Current);
            } else if (cmdName.StartsWith ("sprite")) {
                i.MoveNext ();
                var oldName = i.Current;

                Image uiSprite;
                var spriteObj = HierarchyManager.Find (i.Current, HierarchyManager.Find ("Characters").transform);

                if (spriteObj == null) {
                    spriteObj = HierarchyManager.Find (i.Current);

                    if (spriteObj == null) {
                        var prefab = ResourceManager.Load<Image> (oldName);
                        if (prefab == null) {
                            Debug.Log ("Cannot create " + oldName);
                            return;
                        }

                        var go = Instantiate (prefab);
                        go.transform.SetParent (HierarchyManager.Find ("Characters").transform, false);

                        i.MoveNext ();
                        uiSprite = ResourceManager.Load<Image> (i.Current);
                    } else {
                        uiSprite = spriteObj.GetComponent<Image> ();
                    }
                } else {
                    uiSprite = spriteObj.GetComponent<Image> ();
                }

                if (cmdName.EndsWith ("x")) {
                    i.MoveNext ();

                    var x = float.Parse (i.Current);
                    uiSprite.rectTransform.anchoredPosition = new Vector2 (x, uiSprite.rectTransform.anchoredPosition.y);
                } else if (cmdName.EndsWith ("i")) {
                    i.MoveNext ();
                    uiSprite.sprite = ResourceManager.Load<Sprite> (i.Current);
                }
            } else if (cmdName == "set flag") {
                i.MoveNext ();
                PlayerPrefs.SetInt (i.Current, 1);

                foreach (var item in HierarchyManager.FindObjectsOfType<InventoryItem> ()) {
                    if (item.flag == i.Current) {
                        item.GetComponent<Image> ().enabled = true;
                    }
                }
            } else if (cmdName == "has flag") {
                i.MoveNext ();

                if (PlayerPrefs.HasKey (i.Current)) {
                    i.MoveNext ();
                    CommandIndex = -1;
                    StateId = int.Parse (i.Current);
                }
            } else if (cmdName == "unset flag") {
                i.MoveNext ();

                foreach (var item in HierarchyManager.FindObjectsOfType<InventoryItem> ()) {
                    if (item.flag == i.Current) {
                        item.GetComponent<Image> ().enabled = false;
                    }
                }

                PlayerPrefs.DeleteKey (i.Current);
            } else if (cmdName.StartsWith ("music")) {
                if (cmdName.EndsWith ("start")) {
                    i.MoveNext ();

                    var music = ResourceManager.Load<AudioClip> (i.Current);
                    HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().clip = music;
                    HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().Play ();
                } else if (cmdName.EndsWith ("stop")) {
                    HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().Stop ();
                } else if (cmdName.EndsWith ("snap")) {
                    i.MoveNext ();
                    AudioMixerSnapshot snapshot = HierarchyManager.Find ("BG Music").GetComponent<AudioSource> ().outputAudioMixerGroup.audioMixer.FindSnapshot (i.Current);

                    i.MoveNext ();
                    snapshot.TransitionTo (float.Parse (i.Current));
                }
            } else if (cmdName.StartsWith ("audio")) {
                if (cmdName.EndsWith ("start")) {
                    i.MoveNext ();

                    var audio = ResourceManager.Load<AudioClip> (i.Current);
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().clip = audio;
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Play ();
                } else if (cmdName.EndsWith ("stop")) {
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Stop ();
                }
            } else if (cmdName == "flip") {
                i.MoveNext ();
                HierarchyManager.Find (i.Current).GetComponent<Image> ().transform.Rotate (0, 180, 0);
            }
        } else if (command.StartsWith ("%")) {
            stalled = true;
            // play choice audio
            Dialogue.ChatboxDialogue.Content.text = command.Substring (1);

            var answers = new Dictionary<string, int> ();

            while (CommandIndex < currentState.commands.Count - 1) {
                CommandIndex++;
                var line = currentState.commands [CommandIndex];

                if (line.StartsWith ("%")) {
                    line = line.Substring (1);
                    var answer = Regex.Split (line, " \\d+$") [0];
                    int stateId = int.Parse (Regex.Match (line, "\\d+$").Value);
                    answers.Add (answer, stateId);
                } else {
                    CommandIndex--;
                    break;
                }
            }

            Dialogue.ChatboxDialogue.Answers = answers;
        } else {
            stalled = true;
            Dialogue.ChatboxDialogue.Nameplate = currentState.nameplate;
            Dialogue.ChatboxDialogue.DialogueLine = command;
        }

        CommandIndex++;

        if (CommandIndex >= currentState.commands.Count) {
            CommandIndex = 0;
            StateId++;
        }

        if (cmdName != null) {
            if (cmdName == "return") {
                secondaryStates = null;
                UpdateHotspots ();

                Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                // leave court mode
                InvestigationControls.Controls.EnterInvestigationMode ();

                foreach (Transform sprite in HierarchyManager.Find ("Characters").transform) {
                    Destroy (sprite.gameObject);
                }
            }
        }
    }

    private void UpdateHotspots () {
        foreach (Hotspot h in FindObjectsOfType<Hotspot> ()) {
            if (primaryStateId >= h.minState && h.maxState >= primaryStateId) {
                h.GetComponent<BoxCollider> ().enabled = true;
                h.GetComponent<SpriteRenderer> ().enabled = true;
            } else {
                h.GetComponent<BoxCollider> ().enabled = false;
                h.GetComponent<SpriteRenderer> ().enabled = false;
            }
        }
    }

    private IEnumerator FadeInSprite (Image sprite, float duration = 0) {
        if (fadingSprites.Contains (sprite)) {
            StopCoroutine ("FadeOutSprite");
            fadingSprites.Remove (sprite);
        }

        fadingSprites.Add (sprite);

        sprite.enabled = true;

        if (duration == 0) {
            sprite.color = new Color (1f, 1f, 1f, 1f);

            fadingSprites.Remove (sprite);
            yield break;
        }

        var initialAlpha = sprite.color.a;
        float startTime = Time.time;
        while (sprite != null && sprite.color.a < 1f) {
            if (gamePaused) {
                yield return null;
            }

            sprite.color = new Color (1f, 1f, 1f, Mathf.SmoothStep (initialAlpha, 1, (Time.time - startTime) / duration));
            yield return null;
        }

        fadingSprites.Remove (sprite);
    }

    private IEnumerator FadeOutSprite (Image sprite, float duration = 0) {
        if (fadingSprites.Contains (sprite)) {
            StopCoroutine ("FadeInSprite");
            fadingSprites.Remove (sprite);
        }

        fadingSprites.Add (sprite);

        if (duration == 0) {
            sprite.color = new Color (1f, 1f, 1f, 0f);
            sprite.enabled = false;

            fadingSprites.Remove (sprite);
            yield break;
        }

        var initialAlpha = sprite.color.a;
        float startTime = Time.time;
        while (sprite != null && sprite.color.a > 0f) {
            if (gamePaused) {
                yield return null;
            }

            sprite.color = new Color (1f, 1f, 1f, Mathf.SmoothStep (initialAlpha, 0, (Time.time - startTime) / duration));
            yield return null;
        }

        if (sprite != null) {
            sprite.enabled = false;
        }

        fadingSprites.Remove (sprite);
    }

    private IEnumerator StallForSeconds (float stallTime) {
        float startTime = Time.time;
        while (Time.time - startTime < stallTime) {
            if (gamePaused) {
                yield return null;
            }

            yield return null;
        }

        stalled = false;
    }
}
