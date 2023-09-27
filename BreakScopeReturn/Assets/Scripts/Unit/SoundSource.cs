using UnityEngine;

public struct SoundSource
{
    public SoundSource(Vector3 position, float volumeDistance)
    {
        this.position = position;
        this.volumeDistance = volumeDistance;
        suspicious = false;
        emergence = false;
    }
    public Vector3 position;
    public float volumeDistance;
    public bool suspicious;
    public bool emergence;
    public static void MakeSound(SoundSource soundSource)
    {
        foreach (GameObject unitObj in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unitObj.TryGetComponent(out Unit unit) && Vector3.Distance(soundSource.position, unit.ViewPosition) < soundSource.volumeDistance)
            {
                unit.Hear(soundSource);
            }
        }
    }
}
