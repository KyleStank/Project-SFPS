using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UltimateFracturing;

// Assign this script to a Camera to activate some simple FPS behavior, and interaction with fractured objects.

public class UltimateFracturingFPS : MonoBehaviour
{
    public enum Mode
    {
        ShootObjects,       // Will shoot physical objects
        ExplodeRaycast      // Will raycast against the scene, finding fractured chunks and explode them
    };

    public Mode              ShootMode          = Mode.ExplodeRaycast;  // The current shoot mode
    public float             MouseSpeed         = 0.3f;                 // Mouse sensivity
    public Texture           HUDTexture;                                // The texture to draw at the center
    public float             HUDSize            = 0.03f;                // The size of the HUD to draw
    public Color             HUDColorNormal;                            // HUD color when no raycast chunk was found
    public Color             HUDColorRaycast;                           // HUD color when raycast chunk was found
    public Transform         Weapon;                                    // The transform of the weapon, which should be child of this component assigned to a camera
    public AudioClip         AudioWeaponShot;                           // The audio clip to play when a shot is fired
    public float             WeaponShotVolume   = 1.0f;                 // The volume of the weapon shot
    public float             ExplosionForce     = 1.0f;                 // The force to apply when FracturedChunk.Impact() is called.
    public float             ExplosionRadius    = 0.4f;                 // The radius to apply when FracturedChunk.Impact() is called.
    public float             RecoilDuration     = 0.2f;                 // The length of the recoil animation in seconds
    public float             RecoilIntensity    = 0.05f;                // The intensity of the recoil
    public GameObject        ObjectToShoot      = null;                 // In ShootObjects mode, the object to instance when shooting
    public float             InitialObjectSpeed = 1.0f;                 // In ShootObjects mode, the initial speed of the object
    public float             ObjectScale        = 1.0f;                 // In ShootObjects mode, the object's scale
    public float             ObjectMass         = 1.0f;                 // In ShootObjects mode, the object's mass
    public float             ObjectLife         = 10.0f;                // In ShootObjects mode, the object's life time (seconds until it deletes itself)

    private Vector3          m_v3MousePosition;
    private bool             m_bRaycastFound;
    private float            m_fRecoilTimer;
    private Vector3          m_v3InitialWeaponPos;
    private Quaternion       m_qInitialWeaponRot;

	void Start()
    {
	    m_v3MousePosition = Input.mousePosition;
        m_bRaycastFound   = false;
        m_fRecoilTimer    = 0.0f;

        if(Weapon)
        {
            m_v3InitialWeaponPos = Weapon.localPosition;
            m_qInitialWeaponRot  = Weapon.localRotation;
        }
	}

    void OnGUI()
    {
        Color colGUI = GUI.color;

        // Draw a simple hud for aiming

        if(ShootMode == Mode.ExplodeRaycast)
        {
            int nHalfPixelSize = Mathf.RoundToInt(Screen.width * HUDSize * 0.5f);

            Rect rectPosition = new Rect((Screen.width / 2) - nHalfPixelSize, (Screen.height / 2) - nHalfPixelSize, nHalfPixelSize * 2, nHalfPixelSize * 2);
            GUI.color = m_bRaycastFound ? HUDColorRaycast : HUDColorNormal;
            GUI.DrawTexture(rectPosition, HUDTexture, ScaleMode.StretchToFill, true);
            GUI.color = colGUI;
        }

        GUI.color = colGUI;
    }

	void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            ShootMode = ShootMode == Mode.ExplodeRaycast ? Mode.ShootObjects : Mode.ExplodeRaycast;
        }

        if(ObjectToShoot != null && ShootMode == Mode.ShootObjects)
        {
            // Shoot objects

            if(Weapon) Weapon.GetComponent<Renderer>().enabled = false;

            if(Input.GetKeyDown(KeyCode.Space))
            {
                GameObject newObject           = Instantiate(ObjectToShoot) as GameObject;
                newObject.transform.position   = this.transform.position;
                newObject.transform.localScale = new Vector3(ObjectScale, ObjectScale, ObjectScale);

                Rigidbody rb = newObject.GetComponent<Rigidbody>();
                rb.mass = ObjectMass;
                rb.solverIterationCount = 255;
                rb.AddForce(transform.forward * InitialObjectSpeed, ForceMode.VelocityChange);

                DieTimer dieTimer = newObject.AddComponent<DieTimer>() as DieTimer;
                dieTimer.SecondsToDie = ObjectLife;
            }
        }

        if(ShootMode == Mode.ExplodeRaycast)
        {
            // Raycast

            if(Weapon) Weapon.GetComponent<Renderer>().enabled = true;

            bool bShot = Input.GetKeyDown(KeyCode.Space);

            if(bShot)
            {
                m_fRecoilTimer = RecoilDuration;
                if(AudioWeaponShot) AudioSource.PlayClipAtPoint(AudioWeaponShot, transform.position, WeaponShotVolume);
            }

            m_bRaycastFound = false;

            RaycastHit hitInfo;

            FracturedChunk chunkRaycast = FracturedChunk.ChunkRaycast(transform.position, transform.forward, out hitInfo);

            if(chunkRaycast)
            {
                m_bRaycastFound = true;

                if(bShot)
                {
                    // Hit it!
                    chunkRaycast.Impact(hitInfo.point, ExplosionForce, ExplosionRadius, true);
                }
            }
        }

        // Update recoil

        if(m_fRecoilTimer > 0.0f)
        {
            if(Weapon)
            {
                // Some rudimentary recoil animation here
                Weapon.transform.localPosition = m_v3InitialWeaponPos + new Vector3(0.0f, 0.0f, (-m_fRecoilTimer / RecoilDuration) * RecoilIntensity);
                Weapon.transform.localRotation = m_qInitialWeaponRot * Quaternion.Euler(new Vector3((m_fRecoilTimer / RecoilDuration) * 360.0f * RecoilIntensity * 0.1f, 0.0f, 0.0f));
            }

            m_fRecoilTimer -= Time.deltaTime;
        }
        else
        {
            if(Weapon)
            {
                Weapon.transform.localPosition = m_v3InitialWeaponPos;
                Weapon.transform.localRotation = m_qInitialWeaponRot;
            }
        }

        // Mouse-aim

        if(Input.GetMouseButton(0) && Input.GetMouseButtonDown(0) == false)
        {
            this.transform.Rotate      (-(Input.mousePosition.y - m_v3MousePosition.y) * MouseSpeed, 0.0f, 0.0f);
            this.transform.RotateAround(this.transform.position, Vector3.up, (Input.mousePosition.x - m_v3MousePosition.x) * MouseSpeed);
        }

        m_v3MousePosition = Input.mousePosition;
	}
}
