using UnityEngine;
using System.Collections.Generic;

public enum MachinePurpose
{
    TRIANGLE,
    CIRCLE,
    SQUARE,
    RED,
    GREEN,
    BLUE
}

public interface IMachine
{
    bool IsOn { get; set; }
    ScriptableObject MachineData { get; set; }
    MachinePurpose Purpose { get; set; }
    void Interact(Resource resource);
    void LogResource(Resource resource);
}
