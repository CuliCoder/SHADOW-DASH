using System;
using System.Collections.Generic;
public class WeightRandom<T>
{
    private readonly List<(T value, double Cumulative)> _table = new();
    private double _total;
    private readonly Random _rnd = new Random();

    public WeightRandom(IEnumerable<(T value, double weight)> items)
    {
        UpdateWeights(items);
    }

    public void UpdateWeights(IEnumerable<(T value, double weight)> items)
    {
        _table.Clear();
        double sum = 0f;
        foreach (var (value, weight) in items)
        {
            sum += weight;
            _table.Add((value, sum));
        }
        _total = sum;
    }

    public T Pick()
    {
        double r = _rnd.NextDouble() * _total;
        int lo = 0;
        int hi = _table.Count - 1;
        while (lo < hi)
        {
            int mid = (hi + lo) / 2;
            if (_table[mid].Cumulative < r) lo = mid + 1;
            else hi = mid;
        }
        return _table[lo].value;
    }
}