using UnityEngine;
using ProjectMayhem.Player; // Namespace của BasePlayer

public class PlayerIndicatorManager : MonoBehaviour
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private Transform canvasTransform; 

    [Header("Player References")]
    [SerializeField] private BasePlayer player1;
    [SerializeField] private BasePlayer player2;

    void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("Chưa gán Player vào IndicatorManager!");
            return;
        }
        if (indicatorPrefab == null || canvasTransform == null)
        {
            Debug.LogError("Chưa gán Prefab hoặc Canvas vào IndicatorManager!");
            return;
        }

        SpawnIndicator(player1, 1);

        SpawnIndicator(player2, 2);
    }

    void SpawnIndicator(BasePlayer player, int id)
    {
        GameObject indicatorGO = Instantiate(indicatorPrefab, canvasTransform);
        
        OffScreenIndicator indicatorScript = indicatorGO.GetComponent<OffScreenIndicator>();
        if (indicatorScript != null)
        {
            indicatorScript.targetPlayer = player.transform;
            indicatorScript.playerID = id;
            
            indicatorGO.name = $"Player_{id}_Indicator";
        }
        else
        {
            Debug.LogError("Prefab không có script OffScreenIndicator!");
        }
    }
}