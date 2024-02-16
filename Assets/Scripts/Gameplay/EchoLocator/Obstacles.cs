using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public enum ObstacleType
    {
        Empty,
        Wall,
        Cupboard,
        Ennemy,
    }

    [SerializeField]
    public ObstacleType type = ObstacleType.Empty;

}
