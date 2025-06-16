using UnityEngine;
using Unity.Cinemachine;  // Cinemachine 3.x

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("Main cameras")]
    public CinemachineCamera gameboardCamera;
    public string boardObjectName = "Gameboard";
    public float boardOrbitSpeed = 10f;
    public float heroOrbitSpeed = 2f;

    private CinemachineCamera _p1Camera;
    private CinemachineCamera _p2Camera;
    private CinemachineCamera _villainCamera;
    private CinemachineCamera _current;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        var board = GameObject.Find(boardObjectName);
        if (board != null && gameboardCamera != null)
        {
            var orbit = SetupOrbit(gameboardCamera, board.transform, boardOrbitSpeed);
            orbit.enabled = true;
        }

        FocusBoard();
    }

private OrbitAroundTarget SetupOrbit(CinemachineCamera cam, Transform target, float speed)
{
    var orbit = cam.GetComponent<OrbitAroundTarget>();
    if (orbit == null)
        orbit = cam.gameObject.AddComponent<OrbitAroundTarget>();

    orbit.target = target;
    orbit.speed = speed;

    // Różne wartości w zależności od typu celu
    if (target.CompareTag("Player") || target.CompareTag("Villain"))
    {
        orbit.distance = 5f;
        orbit.height = 3f;
    }
    else
    {
        orbit.distance = 10f;
        orbit.height = 10f;
    }

    orbit.enabled = false;
    return orbit;
}


    public void ConfigureTurnCameras(HeroController hero1, HeroController hero2, VillainController villain)
    {
        if (hero1 != null)
        {
            _p1Camera = hero1.GetComponentInChildren<CinemachineCamera>(true);
            if (_p1Camera != null)
                SetupOrbit(_p1Camera, hero1.transform, heroOrbitSpeed);
        }

        if (hero2 != null)
        {
            _p2Camera = hero2.GetComponentInChildren<CinemachineCamera>(true);
            if (_p2Camera != null)
                SetupOrbit(_p2Camera, hero2.transform, heroOrbitSpeed);
        }

        if (villain != null)
        {
            _villainCamera = villain.GetComponentInChildren<CinemachineCamera>(true);
            if (_villainCamera != null)
                SetupOrbit(_villainCamera, villain.transform, heroOrbitSpeed);
        }
    }

    private void SetActive(CinemachineCamera cam)
    {
        if (cam == null) return;

        if (_current != null)
        {
            _current.Priority = 0;
            var oldOrbit = _current.GetComponent<OrbitAroundTarget>();
            if (oldOrbit != null) oldOrbit.enabled = false;
        }

        cam.Priority = 10;
        var orbit = cam.GetComponent<OrbitAroundTarget>();
        if (orbit != null) orbit.enabled = true;
        _current = cam;
    }

    public void FocusBoard() => SetActive(gameboardCamera);
    public void FocusHero(int index) => SetActive(index == 1 ? _p1Camera : _p2Camera);
    public void FocusVillain() => SetActive(_villainCamera);
}
