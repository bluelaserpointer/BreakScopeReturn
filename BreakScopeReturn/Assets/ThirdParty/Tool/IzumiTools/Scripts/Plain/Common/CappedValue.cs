using UnityEngine;

namespace IzumiTools
{
    /// <summary>
    /// Toplimited positive float value for representing things such as health point, magazine, and <see cref="Cooldown"/>, etc.
    /// </summary>
    [System.Serializable]
    public class CappedValue
    {
        [SerializeField]
        [Min(0f)]
        private float _max;

        public CappedValue(float capacity)
        {
            Capacity = capacity;
        }
        public CappedValue()
        {
            _max = 1;
        }

        private float _value;
        public float Value
        {
            get => _value;
            set => _value = Mathf.Clamp(value, 0, _max);
        }
        public float Capacity
        {
            get => _max;
            set
            {
                _max = Mathf.Max(0, value);
                if (_value > _max)
                    _value = _max;
            }
        }
        /// <summary>
        /// <see cref="Capacity"/> - <see cref="Value"/>
        /// </summary>
        public float Space
        {
            get => _max - _value;
            set => Value = _max - value;
        }
        /// <summary>
        /// <see cref="Value"/> / <see cref="Capacity"/>
        /// </summary>
        public float Ratio
        {
            get => _value / _max;
            set => _value = _max * Mathf.Clamp01(value);
        }
        /// <summary>
        /// <see cref="Value"/> == <see cref="Capacity"/>
        /// </summary>
        public bool Full => _value == _max;
        /// <summary>
        /// <see cref="Value"/> == 0
        /// </summary>
        public bool Empty => _value == 0;
        /// <summary>
        /// <see cref="Value"/> = <see cref="Capacity"/>
        /// </summary>
        public void Fill()
        {
            _value = _max;
        }
        /// <summary>
        /// <see cref="Value"/> = 0
        /// </summary>
        public void Clear()
        {
            Value = 0;
        }
        public float AddAndGetOverflow(float value)
        {
            float overflow;
            _value = ExtendedMath.ClampAndGetOverflow(_value + value, 0, _max, out overflow);
            return overflow;
        }
        public float AddAndGetDelta(float value)
        {
            float oldValue = Value;
            Value += value;
            return Value - oldValue;
        }
        /// <summary>
        /// Transfer value to the other as lot as possible, while not exceeding the amount of the source remaining, target space, and specified toplimit;
        /// </summary>
        /// <param name="target"></param>
        /// <param name="maxValue"></param>
        /// <returns>Actual transfered amount</returns>
        public float TransferTo(CappedValue target, float maxValue = int.MaxValue)
        {
            float amount = Mathf.Min(maxValue, Value, target.Space);
            Value -= amount;
            target.Value += amount;
            return amount;
        }
    }
}