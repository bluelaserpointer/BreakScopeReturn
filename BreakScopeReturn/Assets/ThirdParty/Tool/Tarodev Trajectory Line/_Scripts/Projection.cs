using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class Projection : MonoBehaviour {
    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 100;
    [SerializeField] Transform _obstaclesParent => GameManager.Instance.CurrentStage.transform;

    private Scene _simulationScene;
    private PhysicsScene _physicsScene;
    private readonly Dictionary<Transform, Transform> _spawnedObjects = new Dictionary<Transform, Transform>();

    private void Start() {
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene() {
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();
        foreach (Transform obj in _obstaclesParent) {
            var ghostObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            foreach (var script in ghostObj.GetComponentsInChildren<Component>())
            {
                Type type = script.GetType();
                if (type.IsAssignableFrom(typeof(Transform)))
                    continue;
                if (type == typeof(AudioListener))
                {
                    Destroy(script);
                }
                else if (typeof(Behaviour).IsAssignableFrom(type))
                {
                    if (!typeof(Collider).IsAssignableFrom(type)
                        && !typeof(Rigidbody).IsAssignableFrom(type)
                        && !typeof(Joint).IsAssignableFrom(type))
                        ((Behaviour)script).enabled = false;
                    else
                    {
                        //print("(A): " + type.Name);
                    }
                }
                else if (typeof(Renderer).IsAssignableFrom(type))
                {
                    ((Renderer)script).enabled = false;
                }
                else
                {
                    //print("(B): " + type.Name);
                }
            }
            SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
            if (!ghostObj.isStatic) _spawnedObjects.Add(obj, ghostObj.transform);
        }
    }

    private void Update() {
        foreach (var item in _spawnedObjects) {
            item.Value.SetPositionAndRotation(item.Key.position, item.Key.rotation);
        }
    }

    public void SimulateTrajectory(GameObject ghostProjectile, Action initProcess)
    {
        print("Phase A");
        ghostProjectile.transform.parent = null;
        SceneManager.MoveGameObjectToScene(ghostProjectile, _simulationScene);
        initProcess.Invoke();
        print("Phase B");
        _line.positionCount = _maxPhysicsFrameIterations;
        for (var i = 0; i < _maxPhysicsFrameIterations; i++) {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            if (!ghostProjectile.gameObject)
                return;
            _line.SetPosition(i, ghostProjectile.transform.position);
        }
        print("Phase C");
        Destroy(ghostProjectile.gameObject);
    }
}