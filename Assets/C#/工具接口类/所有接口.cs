using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPower
{
    bool IsOn();
    void AddPower(float set);
}
public interface ION
{
    bool OnSet { get; set; }
    float NumSet { get; set; }
}

public interface IStart
{
    void SetStart(float t);
}
public interface IFlip
{
    public bool flip { get; set; }
}