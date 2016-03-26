using UnityEngine;

using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[SerializeAll]
class CooperScript : MonoBehaviour {

    [DoNotSerialize]
    private static CooperScript engine;
    [DoNotSerialize]
    public static CooperScript Engine {
        get {
            if (engine == null) {
                engine = FindObjectOfType<CooperScript> ();
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
        engine = FindObjectOfType<CooperScript> ();
        enabled = false;

        StartCoroutine (AddPrimaryScript ());
    }

    private System.Collections.IEnumerator AddPrimaryScript () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        var newScript = AddScript (mainScriptFile.text);

        if (!Player.Data.wasDeserialized) {
            primaryStateId = newScript.initialStateId;
            primaryCommandIndex = 0;
            UpdateHotspots ();
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

                    newState.nameplate = sr.ReadLine ().
                        Replace ("$PlayerName", LevelSerializer.PlayerName);

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

    public void AddSecondaryScript (string script) {
        var newScript = AddScript (script);
        secondaryStateId = newScript.initialStateId;
        secondaryCommandIndex = 0;
        secondaryStates = newScript.states;
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
                throw new System.NotImplementedException ();
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
                Hotspot.OnSecondaryScriptComplete (); // inline the static function?
            }
        }
    }

    private void UpdateHotspots () {
        if (secondaryStates != null) {
            return;
        }

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
}
