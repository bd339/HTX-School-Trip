using UnityEngine;

[SerializeAll]
class InvestigationControls : MonoBehaviour {

    [DoNotSerialize]
    private static InvestigationControls controls;
    [DoNotSerialize]
    public static InvestigationControls Controls {
        get {
            if (controls == null) {
                controls = HierarchyManager.FindObjectOfType<InvestigationControls> ();
            }

            return controls;
        }
    }

    public MeshRenderer backgroundMesh;

    private bool hasTarget;
    public bool HasTarget {
        get {
            return hasTarget;
        }
    }

    private Vector3 target;
    public Vector3 Target {
        set {
            target = value;
            hasTarget = true;
            StartCoroutine (LockOnTarget ());
        }
    }

    private float maxUpAngle;
    private float maxDownAngle;
    private float maxLeftAngle;
    private float maxRightAngle;

    // Use this for initialization
    void Start () {
        Vector3 v1 = new Vector3 (0, 0, backgroundMesh.bounds.extents.z);
        float halfHorizView = Camera.main.orthographicSize * Camera.main.aspect;
        Vector3 v2 = new Vector3 (halfHorizView, 0, Mathf.Sqrt (Mathf.Pow (v1.z, 2) - Mathf.Pow (halfHorizView, 2)));
        maxRightAngle = 90 - Vector3.Angle (v1, v2);
        maxLeftAngle = 360 - maxRightAngle;

        Vector3 v3 = new Vector3 (0, Camera.main.orthographicSize, v1.z);
        maxDownAngle = Mathf.Rad2Deg * Mathf.Atan (backgroundMesh.bounds.extents.y / v1.z) - Vector3.Angle (v1, v3);
        maxUpAngle = 360 - maxDownAngle;

        StartCoroutine (Disable ());
    }

    private System.Collections.IEnumerator Disable () {
        yield return new WaitWhile (() => LevelSerializer.IsDeserializing);

        if (!Player.Data.wasDeserialized) {
            enabled = false;
        }
    }

    // Update is called once per frame
    void Update () {
        if (Player.Data.gamePaused || LevelSerializer.IsDeserializing) {
            return;
        }

        Cursor.lockState = CursorLockMode.Confined;

        if (hasTarget) {
            return;
        }

        float rot = 25f * Time.deltaTime;

        float upScrollZone = Screen.height * 0.90f;

        if (Input.GetKey (KeyCode.UpArrow) || Input.mousePosition.y >= upScrollZone) {
            if (Input.GetKey (KeyCode.DownArrow) || Input.mousePosition.y <= Screen.height - upScrollZone) {
                ScrollIndicator.Indicators.Up = false;
                ScrollIndicator.Indicators.Down = false;
                return;
            }

            ScrollIndicator.Indicators.Up = true;
            transform.Rotate (Vector3.left, rot);
        } else if (Input.GetKey (KeyCode.DownArrow) || Input.mousePosition.y <= Screen.height - upScrollZone) {
            ScrollIndicator.Indicators.Down = true;
            transform.Rotate (Vector3.right, rot);
        } else {
            ScrollIndicator.Indicators.Up = false;
            ScrollIndicator.Indicators.Down = false;
        }

        float rightScrollZone = Screen.width * 0.95f;

        if (Input.GetKey (KeyCode.LeftArrow) || Input.mousePosition.x <= Screen.width - rightScrollZone) {
            if (Input.GetKey (KeyCode.RightArrow) || Input.mousePosition.x >= rightScrollZone) {
                ScrollIndicator.Indicators.Left = false;
                ScrollIndicator.Indicators.Right = false;
                return;
            }

            ScrollIndicator.Indicators.Left = true;
            transform.Rotate (Vector3.down, rot);
        } else if (Input.GetKey (KeyCode.RightArrow) || Input.mousePosition.x >= rightScrollZone) {
            ScrollIndicator.Indicators.Right = true;
            transform.Rotate (Vector3.up, rot);
        } else {
            ScrollIndicator.Indicators.Left = false;
            ScrollIndicator.Indicators.Right = false;
        }
    }

    void LateUpdate () {
        // must be done to have angles be consistently in the interval [0, 360]
        // otherwise Unity neglects wrapping negative angles very close to 0
        float rotY = transform.eulerAngles.y < 0 ? 360 + transform.eulerAngles.y : transform.eulerAngles.y;

        if (rotY > 180) {
            if (rotY < maxLeftAngle || Mathf.Abs (rotY - maxLeftAngle) < 0.2f) {
                ScrollIndicator.Indicators.Left = null;
            }
        } else {
            if (rotY > maxRightAngle || Mathf.Abs (rotY - maxRightAngle) < 0.2f) {
                ScrollIndicator.Indicators.Right = null;
            }
        }

        float rotX = transform.eulerAngles.x < 0 ? 360 + transform.eulerAngles.x : transform.eulerAngles.x;

        if (rotX > 180) {
            if (rotX < maxUpAngle || Mathf.Abs (rotX - maxUpAngle) < 0.2f) {
                ScrollIndicator.Indicators.Up = null;
            }
        } else {
            if (rotX > maxDownAngle || Mathf.Abs (rotX - maxDownAngle) < 0.2f) {
                ScrollIndicator.Indicators.Down = null;
            }
        }

        float camX;

        if (rotX > 180) {
            camX = Mathf.Clamp (rotX - 360, maxUpAngle - 360, maxDownAngle);
        } else {
            camX = Mathf.Clamp (rotX, maxUpAngle - 360, maxDownAngle);
        }

        float camY;

        if (rotY > 180) {
            camY = Mathf.Clamp (rotY - 360, maxLeftAngle - 360, maxRightAngle);
        } else {
            camY = Mathf.Clamp (rotY, maxLeftAngle - 360, maxRightAngle);
        }

        transform.eulerAngles = new Vector3 (camX, camY, 0);
    }

    private System.Collections.IEnumerator LockOnTarget () {
        Quaternion q = Quaternion.LookRotation (target - transform.position);
        Vector3 oldRot = transform.eulerAngles;

        while (transform.eulerAngles != q.eulerAngles) {
            if (Player.Data.gamePaused) {
                transform.eulerAngles = q.eulerAngles;
                continue;
            }

            transform.rotation = Quaternion.RotateTowards (transform.rotation, q, 100f * Time.deltaTime);
            LateUpdate (); // wtihout this LateUpdate is never called if the coroutine is running. Unity bug?

            if (oldRot == transform.eulerAngles) {
                break;
            }

            oldRot = transform.eulerAngles;
            yield return null;
        }

        hasTarget = false;
    }

    public void EnterInvestigationMode () {
        enabled = true;
    }

    public void LeaveInvestigationMode () {
        enabled = false;
    }
}
