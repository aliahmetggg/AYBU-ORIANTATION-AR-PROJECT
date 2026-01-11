using UnityEngine;

/// <summary>
/// Dokunma ile avatar toplama mekanizmasını yönetir
/// </summary>
public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }

    [Header("Ayarlar")]
    [SerializeField] private LayerMask avatarLayerMask;
    [SerializeField] private float touchRayDistance = 100f;

    [Header("Efektler")]
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private AudioClip collectSound;

    private Camera mainCamera;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        HandleTouchInput();
    }

    /// <summary>
    /// Dokunma/tıklama girişini işle
    /// </summary>
    private void HandleTouchInput()
    {
        // Dokunma veya mouse tıklaması
        bool isTouching = false;
        Vector3 inputPosition = Vector3.zero;

        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            inputPosition = Input.mousePosition;
        }
        #else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            isTouching = true;
            inputPosition = Input.GetTouch(0).position;
        }
        #endif

        if (!isTouching) return;

        // UI üzerinde mi kontrol et
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Raycast ile avatar bul
        TryCollectAvatar(inputPosition);
    }

    /// <summary>
    /// Raycast ile avatar toplamaya çalış
    /// </summary>
    private void TryCollectAvatar(Vector3 screenPosition)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, touchRayDistance, avatarLayerMask))
        {
            CollectableAvatar collectable = hit.collider.GetComponent<CollectableAvatar>();
            if (collectable == null)
            {
                collectable = hit.collider.GetComponentInParent<CollectableAvatar>();
            }

            if (collectable != null)
            {
                PlayCollectEffects(hit.point);
                collectable.Collect();
            }
        }
        else
        {
            // Layer mask olmadan da dene
            if (Physics.Raycast(ray, out hit, touchRayDistance))
            {
                CollectableAvatar collectable = hit.collider.GetComponent<CollectableAvatar>();
                if (collectable == null)
                {
                    collectable = hit.collider.GetComponentInParent<CollectableAvatar>();
                }

                if (collectable != null)
                {
                    PlayCollectEffects(hit.point);
                    collectable.Collect();
                }
            }
        }
    }

    /// <summary>
    /// Toplama efektlerini oynat
    /// </summary>
    private void PlayCollectEffects(Vector3 position)
    {
        // Particle efekti
        if (collectEffectPrefab != null)
        {
            GameObject effect = Instantiate(collectEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // Ses efekti
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }

    /// <summary>
    /// Debug ray çiz
    /// </summary>
    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        Gizmos.color = Color.yellow;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawRay(ray.origin, ray.direction * touchRayDistance);
    }
}
