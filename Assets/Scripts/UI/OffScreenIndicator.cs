using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OffScreenIndicator : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image arrowImage;
    [SerializeField] private TextMeshProUGUI distanceText;

    [Header("Settings")]
    [SerializeField] private float screenEdgePadding = 60f; 
    [SerializeField] private float rotationOffset = -90f;

    public Transform targetPlayer;
    public int playerID;

    private Camera mainCamera;
    private RectTransform selfRectTransform;
    private Canvas mainCanvas;
    private Color lastSetColor = Color.clear; 

    void Start()
    {
        mainCamera = Camera.main;
        selfRectTransform = GetComponent<RectTransform>();
        mainCanvas = GetComponentInParent<Canvas>();

        if (arrowImage == null) arrowImage = GetComponentInChildren<Image>();
        if (distanceText == null) distanceText = GetComponentInChildren<TextMeshProUGUI>();
        
        UpdateColor();
        
        SetVisibility(false);
    }

    void UpdateColor()
    {
        if (arrowImage == null || distanceText == null) return;
        
        Color targetColor;
        if (GameData.Instance != null)
        {
            targetColor = playerID == 1 
                ? GameData.Instance.player1Color 
                : GameData.Instance.player2Color;
        }
        else
        {
            targetColor = Color.white;
        }

        if (lastSetColor != targetColor)
        {
            arrowImage.color = targetColor;
            distanceText.color = targetColor;
            lastSetColor = targetColor;
        }
    }
    
    void SetVisibility(bool isVisible)
    {
        arrowImage.enabled = isVisible;
        distanceText.enabled = isVisible;
    }

    void LateUpdate()
    {
        if (targetPlayer == null || mainCamera == null)
        {
            SetVisibility(false);
            return;
        }

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetPlayer.position);
        bool isVisible = viewportPos.z > 0 && 
                         viewportPos.x > 0.0f && viewportPos.x < 1.0f && 
                         viewportPos.y > 0.0f && viewportPos.y < 1.0f;

        if (isVisible)
        {
            SetVisibility(false); 
            return;
        }
        
        SetVisibility(true); 
        
        UpdateColor();

        if (viewportPos.z < 0)
        {
            viewportPos = (Vector2)(viewportPos - new Vector3(0.5f, 0.5f)) * -1.0f + new Vector2(0.5f, 0.5f);
            viewportPos.z = 0; 
        }
        
        // --- 3. XOAY MŨI TÊN ---
        Vector3 direction = (viewportPos - new Vector3(0.5f, 0.5f, 0));
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        selfRectTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // --- 4. ĐỊNH VỊ MŨI TÊN Ở MÉP ---
        Vector3 screenEdgeViewport = new Vector3(
            Mathf.Clamp(viewportPos.x, 0.0f, 1.0f),
            Mathf.Clamp(viewportPos.y, 0.0f, 1.0f),
            viewportPos.z
        );

        Vector3 screenPos = mainCamera.ViewportToScreenPoint(screenEdgeViewport);
        float clampedX = Mathf.Clamp(screenPos.x, screenEdgePadding, Screen.width - screenEdgePadding);
        float clampedY = Mathf.Clamp(screenPos.y, screenEdgePadding, Screen.height - screenEdgePadding);
        selfRectTransform.position = new Vector2(clampedX, clampedY); 

        // --- 5. TÍNH KHOẢNG CÁCH ---
        float playerDistance = mainCamera.WorldToScreenPoint(targetPlayer.position).z;
        Vector3 edgeWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(screenEdgeViewport.x, screenEdgeViewport.y, playerDistance));
        float distance = Vector2.Distance((Vector2)targetPlayer.position, (Vector2)edgeWorldPos);
        distanceText.text = $"{distance:F0}m";
    }
}