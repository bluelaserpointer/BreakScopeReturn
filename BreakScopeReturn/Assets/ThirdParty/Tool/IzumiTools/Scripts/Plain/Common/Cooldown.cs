using UnityEngine;

namespace IzumiTools
{
    /// <summary>
    /// Manual progressable cooldown manager.
    /// </summary>
    [System.Serializable]
    public class Cooldown : CappedValue
    {
        public Cooldown(float maxValue)
        {
            Capacity = maxValue;
        }
        public Cooldown()
        {
            Capacity = 1;
        }
        public bool IsReady {
            get => Value == Capacity;
            set
            {
                if (value)
                    Fill();
                else
                    Clear();
            }
        }
        public bool Eat()
        {
            if (IsReady)
            {
                Clear();
                return true;
            }
            return false;
        }
        public void Add(float value)
        {
            Value += value;
        }
        public void AddDeltaTime()
        {
            Add(Time.deltaTime);
        }
        public void AddFixedDeltaTime()
        {
            Add(Time.fixedDeltaTime);
        }
        public bool AddAndEat(float value)
        {
            Add(value);
            return Eat();
        }
        public bool AddDeltaTimeAndEat()
        {
            AddDeltaTime();
            return Eat();
        }
        public bool AddFixedDeltaTimeAndEat()
        {
            AddFixedDeltaTime();
            return Eat();
        }
    }
}