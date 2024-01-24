using UnityEngine;

namespace IzumiTools
{
    /// <summary>
    /// Controls frequently be instantiated / destroyed objects to only switches its activeSelf.<br/>
    /// Used for bullets, UIs, etc.
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    [System.Serializable]
    public class ReuseNest<T> where T : Component
    {
        public Transform container;
        public T prefab;
        public System.Action<T> actionForNewGeneration;
        public System.Action<T> actionForReactivated;
        public T FirstChild => container.GetChild(0)?.GetComponent<T>();
        public T LastChild => container.GetChild(container.childCount - 1)?.GetComponent<T>();
        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach(Transform t in container)
                {
                    if(t.gameObject.activeSelf)
                        ++count;
                }
                return count;
            }
        }
        public int LastActiveSiblingIndex
        {
            get
            {
                for(int i = container.childCount - 1; i >= 0; --i)
                {
                    if (container.GetChild(i).gameObject.activeSelf)
                        return i;
                }
                return 0;
            }
        }
        public T EnableOne()
        {
            return Reuse() ?? Generate();
        }
        public T Reuse()
        {
            T returnObject;
            foreach (Transform childTf in container)
            {
                if (childTf.gameObject.activeSelf)
                    continue;
                if ((returnObject = childTf.GetComponent<T>()) != null)
                {
                    returnObject.gameObject.SetActive(true);
                    actionForReactivated?.Invoke(returnObject);
                    return returnObject;
                }
            }
            return null;
        }
        public T Generate()
        {
            T returnObject = Object.Instantiate(prefab);
            returnObject.transform.SetParent(container, false);
            returnObject.gameObject.SetActive(true);
            return returnObject;
        }
        public T PickEnabledFirst()
        {
            foreach (Transform childTf in container)
            {
                if (childTf.gameObject.activeSelf)
                    return childTf.GetComponent<T>();
            }
            return null;
        }
        public void DisableFirst()
        {
            T picked = PickEnabledFirst();
            if (picked != null)
                picked.gameObject.SetActive(false);
        }
        public void DisableAll()
        {
            container.SetActiveAllChidren(false);
        }
        public void DestroyAll()
        {
            container.DestroyAllChildren();
        }
    }

}