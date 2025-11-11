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

    void Start()
    {
        mainCamera = Camera.main;
        selfRectTransform = GetComponent<RectTransform>();
        mainCanvas = GetComponentInParent<Canvas>();

        if (arrowImage == null) arrowImage = GetComponentInChildren<Image>();
        if (distanceText == null) distanceText = GetComponentInChildren<TextMeshProUGUI>();

        SetIndicatorColor();
        
        SetVisibility(false);
    }

    void SetIndicatorColor()
    {
        if (arrowImage == null || GameData.Instance == null) return;

        if (playerID == 1)
        {
            arrowImage.color = GameData.Instance.player1Color;
        }
        else if (playerID == 2)
        {
            arrowImage.color = GameData.Instance.player2Color;
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

        // --- 1. KIỂM TRA TẦM NHÌN (Sử dụng Viewport) ---
        // Viewport: (0,0) là góc dưới trái, (1,1) là góc trên phải
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(targetPlayer.position);

        bool isVisible = viewportPos.z > 0 && 
                         viewportPos.x > 0.0f && viewportPos.x < 1.0f && 
                         viewportPos.y > 0.0f && viewportPos.y < 1.0f;

        if (isVisible)
        {
            SetVisibility(false); // Player ở trong màn hình, ẩn mũi tên
            return;
        }
        
        SetVisibility(true); // Player ở ngoài, hiện mũi tên

        // --- 2. XỬ LÝ KHI Ở ĐẰNG SAU CAMERA ---
        if (viewportPos.z < 0)
        {
            // Lật vị trí lại so với tâm (0.5, 0.5)
            viewportPos = (Vector2)(viewportPos - new Vector3(0.5f, 0.5f)) * -1.0f + new Vector2(0.5f, 0.5f);
            viewportPos.z = 0; // Đặt z = 0 để nó nằm "trên" mặt phẳng camera
        }
        
        // --- 3. XOAY MŨI TÊN ---
        // Lấy hướng từ tâm màn hình (0.5, 0.5) tới vị trí của player (viewportPos)
        // Dùng viewportPos (chưa clamp) để có hướng chính xác
        Vector3 direction = (viewportPos - new Vector3(0.5f, 0.5f, 0));
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        selfRectTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        // --- 4. ĐỊNH VỊ MŨI TÊN Ở MÉP ---
        // Bây giờ, clamp vị trí viewport vào mép (từ 0 đến 1)
        Vector3 screenEdgeViewport = new Vector3(
            Mathf.Clamp(viewportPos.x, 0.0f, 1.0f),
            Mathf.Clamp(viewportPos.y, 0.0f, 1.0f),
            viewportPos.z
        );

        // Chuyển vị trí viewport đã clamp (0-1) sang vị trí Screen (pixel)
        Vector3 screenPos = mainCamera.ViewportToScreenPoint(screenEdgeViewport);
        
        // Clamp thêm 1 lần nữa ở tọa độ pixel để thêm padding
        float clampedX = Mathf.Clamp(screenPos.x, screenEdgePadding, Screen.width - screenEdgePadding);
        float clampedY = Mathf.Clamp(screenPos.y, screenEdgePadding, Screen.height - screenEdgePadding);
        
        selfRectTransform.position = new Vector2(clampedX, clampedY); // Hoạt động đúng với Canvas Overlay

        // --- 5. TÍNH KHOẢNG CÁCH ---
        // Lấy z-distance (khoảng cách) của player
        float playerDistance = mainCamera.WorldToScreenPoint(targetPlayer.position).z;
        
        // Chuyển vị trí rìa viewport (screenEdgeViewport) về World
        // Sử dụng z-distance của player để có vị trí world chính xác ở rìa
        Vector3 edgeWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(screenEdgeViewport.x, screenEdgeViewport.y, playerDistance));
        
        // Tính khoảng cách 2D
        float distance = Vector2.Distance((Vector2)targetPlayer.position, (Vector2)edgeWorldPos);
        
        distanceText.text = $"{distance:F0}m";
    }
}