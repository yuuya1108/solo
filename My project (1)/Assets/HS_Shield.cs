
using UnityEngine;

public class AutoGuardItem : MonoBehaviour
{
    [Header("ƒAƒCƒeƒ€İ’è")]
    public bool hasItem = true;

    [Header("”½Œ‚İ’è")]
    public float stunDuration = 1.5f;

    public GameObject shieldObject;

    private AudioSource audioSource;

    public AudioClip Shieldsound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        shieldObject.SetActive(hasItem);
    }

    // UŒ‚‚ğ–h‚°‚½‚©‚Ç‚¤‚©‚ğ•Ô‚·
    public bool TryBlock(Controller attacker)
    {
        if (!hasItem)
        {
            return false;
        }

        // ƒAƒCƒeƒ€‚ğÁ”ï
        hasItem = false;

        shieldObject.SetActive(false);

        // UŒ‚Ò‚ğd’¼‚³‚¹‚é
        if (attacker != null)
        {
            attacker.ApplyItemStun(stunDuration);
        }

        if (Shieldsound != null && audioSource != null)
        {
            audioSource.PlayOneShot(Shieldsound);
        }

        return true;
    }
}
