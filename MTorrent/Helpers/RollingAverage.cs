using System;
using System.Diagnostics;

namespace Torrent.Helpers
{
    public class RollingAverage
    {
        private readonly float[] _values;
        private int _index;

        public float Current { get; private set; }

        public RollingAverage(int intervals)
        {
            if (intervals < 1)
                throw new ArgumentOutOfRangeException(nameof(intervals));

            _values = new float[intervals];
            _index = 0;
            Current = 0;
        }

        public void Update(float value)
        {
            Debug.Assert(!float.IsNaN(value) && !float.IsInfinity(value));

            if ((uint)++_index >= (uint)_values.Length)
                _index = 0;

            Current -= _values[_index];
            value = _values[_index] = value / _values.Length;
            Current += value;
        }
    }
}
