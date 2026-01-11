using UnityEngine;

[RequireComponent(typeof(AreaEffector2D), typeof(Collider2D))]
public class FanZone : MonoBehaviour
{
    private AreaEffector2D effector;
    private bool girlInside = false;
    private PlayerController girlRef;
    public AudioSource fanSource;
    public AudioClip fanClip;
    
    void Awake()
    {
        effector = GetComponent<AreaEffector2D>();
        effector.enabled = false; // fan starts off
    }

    void OnEnable()
    {
        PlayerController.OnGlideStateChanged += OnPlayerGlideChange;
    }

    void OnDisable()
    {
        PlayerController.OnGlideStateChanged -= OnPlayerGlideChange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ✅ Use tag to detect player
        if (!other.CompareTag("Player"))
            return;

        var player = other.GetComponent<PlayerController>();
        if (player != null && !player.lightPlayer) // only girl
        {
            girlInside = true;
            girlRef = player;
            UpdateEffectorState();
            Debug.Log("👧 Girl entered fan zone");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var player = other.GetComponent<PlayerController>();
        if (player != null && !player.lightPlayer)
        {
            girlInside = false;
            girlRef = null;
            fanSource.PlayOneShot(fanClip);
            UpdateEffectorState();
            Debug.Log("👧 Girl left fan zone");
        }
    }

    private void OnPlayerGlideChange(PlayerController player, bool isGliding)
    {
        // respond only to girl's glide state
        if (player == girlRef && !player.lightPlayer)
        {
            UpdateEffectorState();
        }
    }

    private void UpdateEffectorState()
    {
       
        bool shouldEnable = girlInside && girlRef != null && girlRef.isGliding;
        effector.enabled = shouldEnable;

        if (shouldEnable)
            Debug.Log("🌬 Fan ON (girl gliding inside zone)");
        else
            Debug.Log("❌ Fan OFF");
    }
}
