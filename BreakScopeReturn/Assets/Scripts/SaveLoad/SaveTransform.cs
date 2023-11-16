using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveTransform : SaveTarget
{
    struct Pose
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    public override void Deserialize(string json)
    {
        Pose pose = JsonUtility.FromJson<Pose>(json);
        transform.SetPositionAndRotation(pose.position, pose.rotation);
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(new Pose()
        {
            position = transform.position,
            rotation = transform.rotation
        });
    }
}
