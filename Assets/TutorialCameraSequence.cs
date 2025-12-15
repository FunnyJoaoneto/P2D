using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;   // for CinemachineBrain

public class TutorialCameraSequence : MonoBehaviour
{
    [Header("Camera References")]
    public Camera cam;

    [Header("Camera Walls")]
    public GameObject cameraWallsRoot;

    [Header("Timing")]
    public float startDelay = 0.5f;

    [System.Serializable]
    public class Step
    {
        public Transform target;
        public float size = 20f;
        public float moveTime = 1.5f;
        public float holdTime = 1f;

        [Header("Tutorial Text")]
        [TextArea] public string message;

        [Header("Ping Targets")]
        public Transform[] pingTargets;
        public float pingDuration = 1f;
    }

    [Header("Sequence")]
    public Step[] steps;

    [Header("UI")]
    public TMPro.TextMeshProUGUI tutorialText;
    public GameObject pingPrefab;
    public Canvas uiCanvas;

    // internal
    private bool wallsInitiallyActive = true;
    private Transform player1Transform;
    private Transform player2Transform;

    // anchors above the heads
    private Transform player1PingAnchor;
    private Transform player2PingAnchor;

    private Vector3 initialCamPos;
    private float initialCamSize;

    // Skip tutorial
    private bool skipRequested = false;
    public GameObject skipButton;

    // Cinemachine brain on the main camera
    private CinemachineBrain brain;

    private IEnumerator Start()
    {
        if (cam == null)
            cam = Camera.main;

        brain = cam != null ? cam.GetComponent<CinemachineBrain>() : null;

        // Lock movement immediately so players don't move during setup
        PlayerGlobalLock.movementLocked = true;

        // Give Cinemachine one frame to position the camera at its gameplay start
        yield return null;

        // Optional delay BEFORE we take control, with Cinemachine still active
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        // Capture the position/size that Cinemachine set
        initialCamPos  = cam.transform.position;
        initialCamSize = cam.orthographicSize;

        // Now disable Cinemachine so this script fully drives the camera
        if (brain != null)
            brain.enabled = false;

        //------------------------
        // FIND PLAYERS
        //------------------------
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length < 2)
        {
            Debug.LogError("TutorialCameraSequence: Expected 2 players but found " + players.Length);
            yield break;
        }

        player1Transform = players[0].transform;
        player2Transform = players[1].transform;

        // find PingAnchor child on each player
        player1PingAnchor = player1Transform.Find("PingAnchor");
        player2PingAnchor = player2Transform.Find("PingAnchor");

        if (player1PingAnchor == null || player2PingAnchor == null)
        {
            Debug.LogError("TutorialCameraSequence: PingAnchor child not found on one of the players.");
            yield break;
        }

        //------------------------
        // DISABLE CAMERA WALLS
        //------------------------
        if (cameraWallsRoot != null)
        {
            wallsInitiallyActive = cameraWallsRoot.activeSelf;
            cameraWallsRoot.SetActive(false);
        }

        if (skipButton != null)
            skipButton.SetActive(true);

        tutorialText.gameObject.SetActive(true);

        //------------------------
        // INTRO STEP (NO CAMERA MOVE)
        //------------------------
        yield return ShowStepMessage(
            "Here are both of you: The LightGuy and the NightGirl.",
            new Transform[] { player1PingAnchor, player2PingAnchor },
            2f
        );

        if (skipRequested)
        {
            yield return SkipToGameplay();
            yield break;
        }

        yield return new WaitForSeconds(2f);

        //------------------------
        // RUN CAMERA SEQUENCE
        //------------------------
        foreach (var step in steps)
        {
            if (skipRequested)
                break;

            // Show message + pings
            yield return ShowStepMessage(step.message, step.pingTargets, step.pingDuration);

            if (skipRequested)
                break;

            // Move camera to step target
            yield return MoveToStep(step);
        }

        if (skipRequested)
        {
            yield return SkipToGameplay();
            yield break;
        }

        //------------------------
        // RESTORE CAMERA + WALLS
        //------------------------
        // Smooth transition back to the initial camera view
        yield return SmoothBackToInitial(1.5f);

        // Give control back to Cinemachine
        if (brain != null)
            brain.enabled = true;

        if (cameraWallsRoot != null)
            cameraWallsRoot.SetActive(wallsInitiallyActive);

        // Hide tutorial text
        tutorialText.text = "";
        tutorialText.gameObject.SetActive(false);

        //------------------------
        // UNLOCK MOVEMENT
        //------------------------
        PlayerGlobalLock.movementLocked = false;
    }

    private IEnumerator SkipToGameplay()
    {
        // Smooth move to initial camera state (still with Cinemachine disabled)
        yield return SmoothBackToInitial(1.5f);

        // Give control back to Cinemachine
        if (brain != null)
            brain.enabled = true;

        // Restore camera walls
        if (cameraWallsRoot != null)
            cameraWallsRoot.SetActive(wallsInitiallyActive);

        // Hide UI
        tutorialText.text = "";
        tutorialText.gameObject.SetActive(false);

        // Unlock movement
        PlayerGlobalLock.movementLocked = false;
    }

    //-----------------------------------------
    // CAMERA MOVEMENT
    //-----------------------------------------
    private IEnumerator MoveToStep(Step step)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        Vector3 targetPos = new Vector3(
            step.target.position.x,
            step.target.position.y,
            cam.transform.position.z
        );

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(step.moveTime, 0.01f);

            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, step.size, t);

            yield return null;
        }

        if (step.holdTime > 0f)
            yield return new WaitForSeconds(step.holdTime);
    }

    //-----------------------------------------
    // SHOW MESSAGE + PINGS
    //-----------------------------------------
    private IEnumerator ShowStepMessage(string msg, Transform[] pings, float duration)
    {
        tutorialText.text = msg;

        if (pings != null)
        {
            foreach (var p in pings)
            {
                if (p != null)
                    CreatePing(p, duration);
            }
        }

        yield return null;
    }

    private IEnumerator SmoothBackToInitial(float duration)
    {
        if (skipButton != null)
            skipButton.SetActive(false);

        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, initialCamPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, initialCamSize, t);
            yield return null;
        }
    }

    //-----------------------------------------
    // CREATE PING UI ELEMENT
    //-----------------------------------------
    private void CreatePing(Transform target, float duration)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        GameObject ping = Instantiate(pingPrefab, uiCanvas.transform);
        ping.transform.position = screenPos;

        Destroy(ping, duration);
    }

    //-----------------------------------------
    // SKIP TUTORIAL
    //-----------------------------------------
    public void RequestSkip()
    {
        skipRequested = true;
        if (skipButton != null)
            skipButton.SetActive(false);
    }
}
