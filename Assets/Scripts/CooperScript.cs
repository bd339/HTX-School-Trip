using UnityEngine;
using UnityEngine.UI;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Audio;

[SerializeAll]
class CooperScript : MonoBehaviour {

    [DoNotSerialize]
    private static CooperScript engine;
    [DoNotSerialize]
    public static CooperScript Engine {
        get {
            if (engine == null) {
                engine = HierarchyManager.FindObjectOfType<CooperScript> ();
            }

            return engine;
        }
    }

    [DoNotSerialize]
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
    private Hotspot secondaryScriptProvider;

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

    // Use this for initialization
    void Start () {
        enabled = false;
        StartCoroutine (Init ());
    }

    private IEnumerator Init () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        var newScript = AddScript (mainScriptFile.text);

        if (!Player.Data.wasDeserialized) {
            primaryStateId = newScript.initialStateId;
            primaryCommandIndex = 0;
            UpdateHotspots ();
        } else {
            // reload the secondary script in case the game was saved during e.g. dialogue
            if (secondaryStates != null) {
                var reloadedScript = AddScript (secondaryScriptProvider.onClickScript);
                secondaryStates = reloadedScript.states;
            }

            // put sprites back on the loaded characters
            foreach (Transform sprite in HierarchyManager.Find ("Characters").transform) {
                string spriteName;
                if (Player.Data.spriteMap.TryGetValue (sprite.gameObject.name, out spriteName)) {
                    sprite.GetComponent<Image> ().sprite = Resources.Load<Sprite> (spriteName);
                }
            }
        }

        // loading a save game after script changes is very likely to break this
        primaryStates = newScript.states;

        enabled = true;
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

                    newState.nameplate = sr.ReadLine ().Replace ("$PlayerName", LevelSerializer.PlayerName);

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
        secondaryScriptProvider = provider;
    }

    // Update is called once per frame
    void Update () {
        if (Player.Data.gamePaused || stalled || LevelSerializer.IsDeserializing) {
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

            if (cmdName.Equals ("wait")) {
                i.MoveNext ();
                stalled = true;
                StartCoroutine ("StallForSeconds", float.Parse (i.Current));
            } else if (cmdName.Equals ("mode")) {
                i.MoveNext ();

                if (i.Current.Equals ("investigation")) {
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // leave court mode
                    InvestigationControls.Controls.EnterInvestigationMode ();
                    ScrollIndicator.Indicators.EnterInvestigationMode ();
                } else if (i.Current.Equals ("dialogue")) {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    ScrollIndicator.Indicators.LeaveInvestigationMode ();
                    // leave court mode
                    Dialogue.ChatboxDialogue.EnterDialogueMode ();
                } else if (i.Current.Equals ("court")) {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    ScrollIndicator.Indicators.LeaveInvestigationMode ();
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // enter court mode
                } else {
                    InvestigationControls.Controls.LeaveInvestigationMode ();
                    ScrollIndicator.Indicators.LeaveInvestigationMode ();
                    Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                    // leave court mode
                }
            } else if (cmdName.Equals ("state")) {
                i.MoveNext ();
                CommandIndex = -1;
                StateId = int.Parse (i.Current);
            } else if (cmdName.Equals ("global state")) {
                i.MoveNext ();

                if (secondaryStates == null) {
                    primaryCommandIndex = -1;
                } else {
                    primaryCommandIndex = 0;
                }

                primaryStateId = int.Parse (i.Current);
                UpdateHotspots ();
            } else if (cmdName.Equals ("fade in")) {
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
                        var prefab = Resources.Load<Image> (spriteName);
                        if (prefab == null) {
                            Debug.Log ("There's no way to fade in " + spriteName);
                            return;
                        }
                        var go = Instantiate (prefab);
                        go.name = spriteName;
                        go.gameObject.SetParent (HierarchyManager.Find ("Characters"));

                        Player.Data.spriteMap.Remove (go.name);
                        Player.Data.spriteMap.Add (go.name, prefab.sprite.name);

                        if (i.MoveNext ()) {
                            StartCoroutine (FadeInSprite (go, float.Parse (i.Current)));
                        } else {
                            StartCoroutine (FadeInSprite (go));
                        }
                    }
                }
            } else if (cmdName.Equals ("fade out")) {
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
            } else if (cmdName.Equals ("text color")) {
                i.MoveNext ();
                Dialogue.ChatboxDialogue.DialogueColor = i.Current;
            } else if (cmdName.Equals ("text delay")) {
                i.MoveNext ();
                Dialogue.ChatboxDialogue.dialogueDelay = float.Parse (i.Current);
            } else if (cmdName.Equals ("location")) {
                i.MoveNext ();
                Player.Data.location = i.Current;
            } else if (cmdName.Equals ("panorama")) {
                i.MoveNext ();
                if (!HierarchyManager.Find ("Panorama").activeInHierarchy) {
                    HierarchyManager.Find ("Flat").SetActive (false);
                    HierarchyManager.Find ("Panorama").SetActive (true);
                }

                InvestigationControls.Controls.BackgroundMesh = HierarchyManager.Find ("Panorama").GetComponent<MeshRenderer> ();
                InvestigationControls.Controls.BackgroundTex = Resources.Load<Texture2D> (i.Current);
            } else if (cmdName == "flat") {
                i.MoveNext ();
                if (!HierarchyManager.Find ("Flat").activeInHierarchy) {
                    HierarchyManager.Find ("Panorama").SetActive (false);
                    HierarchyManager.Find ("Flat").SetActive (true);
                }

                InvestigationControls.Controls.BackgroundMesh = HierarchyManager.Find ("Flat").GetComponent<MeshRenderer> ();
                InvestigationControls.Controls.BackgroundTex = Resources.Load<Texture2D> (i.Current);
            } else if (cmdName.StartsWith ("sprite")) {
                i.MoveNext ();
                var oldName = i.Current;

                Image uiSprite;
                var spriteObj = HierarchyManager.Find (i.Current, HierarchyManager.Find ("Characters").transform);
                if (spriteObj == null) {
                    var prefab = Resources.Load<Image> (oldName);
                    if (prefab == null) {
                        Debug.Log ("Cannot create " + oldName);
                        return;
                    }
                    var go = Instantiate (prefab);
                    go.name = oldName;
                    go.gameObject.SetParent (HierarchyManager.Find ("Characters"));
                    i.MoveNext ();
                    uiSprite = Resources.Load<Image> (i.Current);
                } else {
                    uiSprite = spriteObj.GetComponent<Image> ();
                }

                if (cmdName.EndsWith ("x")) {
                    i.MoveNext ();
                    var x = float.Parse (i.Current);
                    uiSprite.rectTransform.anchoredPosition = new Vector2 (x, uiSprite.rectTransform.anchoredPosition.y);
                } else if (cmdName.EndsWith ("i")) {
                    i.MoveNext ();
                    uiSprite.sprite = Resources.Load<Sprite> (i.Current);
                }
            } else if (cmdName.Equals ("set flag")) {
                i.MoveNext ();

                if (!Player.Data.flags.Contains(i.Current)) {
                    Player.Data.flags.Add (i.Current);

                    foreach (var item in HierarchyManager.FindObjectsOfType<InventoryItem> ()) {
                        if (item.flag == i.Current) {
                            item.GetComponent<Image> ().enabled = true;
                        }
                    }
                }
            } else if (cmdName.Equals ("has flag")) {
                i.MoveNext ();
                if (Player.Data.flags.Contains (i.Current)) {
                    i.MoveNext ();
                    CommandIndex = -1;
                    StateId = int.Parse (i.Current);
                }
            } else if (cmdName.Equals ("unset flag")) {
                i.MoveNext ();

                foreach (var item in HierarchyManager.FindObjectsOfType<InventoryItem> ()) {
                    if (item.flag == i.Current) {
                        item.GetComponent<Image> ().enabled = false;
                    }
                }

                Player.Data.flags.Remove (i.Current);
            } else if (cmdName.StartsWith ("music")) {
                if (cmdName.EndsWith ("start")) {
                    i.MoveNext ();
                    var music = Resources.Load<AudioClip> (i.Current);
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
                    var audio = Resources.Load<AudioClip> (i.Current);
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().clip = audio;
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Play ();
                } else if (cmdName.EndsWith ("stop")) {
                    HierarchyManager.Find ("Sound Effects").GetComponent<AudioSource> ().Stop ();
                }
            }
        } else if (command.StartsWith ("%")) {
            stalled = true;
            // play choice audio
            Dialogue.ChatboxDialogue.Question = command.Substring (1);

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
            if (cmdName.Equals ("return")) {
                secondaryStates = null;
                UpdateHotspots ();

                Dialogue.ChatboxDialogue.LeaveDialogueMode ();
                // leave court mode
                InvestigationControls.Controls.EnterInvestigationMode ();
                ScrollIndicator.Indicators.EnterInvestigationMode ();

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

    private IEnumerator FadeInSprite (Image sprite, float duration = 0.25f) {
        sprite.enabled = true;

        if (duration == 0) {
            sprite.color = new Color (1f, 1f, 1f, 1f);
            yield break;
        }

        var initialAlpha = sprite.color.a;
        float startTime = Time.time;
        while (sprite != null && sprite.color.a < 1f) {
            if (Player.Data.gamePaused) {
                sprite.color = new Color (1f, 1f, 1f, 1f);
                break;
            }

            sprite.color = new Color (1f, 1f, 1f, Mathf.SmoothStep (initialAlpha, 1, (Time.time - startTime) / duration));
            yield return null;
        }
    }

    private IEnumerator FadeOutSprite (Image sprite, float duration = 0.25f) {
        if (duration == 0) {
            sprite.color = new Color (1f, 1f, 1f, 0f);
            sprite.enabled = false;
            yield break;
        }

        var initialAlpha = sprite.color.a;
        float startTime = Time.time;
        while (sprite != null && sprite.color.a > 0f) {
            if (Player.Data.gamePaused) {
                sprite.color = new Color (1f, 1f, 1f, 0f);
                break;
            }

            sprite.color = new Color (1f, 1f, 1f, Mathf.SmoothStep (initialAlpha, 0, (Time.time - startTime) / duration));
            yield return null;
        }

        if (sprite != null) {
            sprite.enabled = false;
        }
    }

    private IEnumerator StallForSeconds (float stallTime) {
        float startTime = Time.time;
        while (Time.time - startTime < stallTime) {
            if (Player.Data.gamePaused) {
                break;
            }

            yield return null;
        }

        stalled = false;
    }
}
