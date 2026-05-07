using System;
using System.Collections.Generic;
using Configs;

public class ScratchToolInventoryModel
{
    private readonly List<ScratchToolConfig> _ownedTools = new List<ScratchToolConfig>();

    public IReadOnlyList<ScratchToolConfig> OwnedTools => _ownedTools;

    public event Action<ScratchToolConfig> OnToolAdded;

    public ScratchToolInventoryModel(IEnumerable<ScratchToolConfig> starterTools = null)
    {
        if (starterTools == null)
        {
            return;
        }

        foreach (ScratchToolConfig tool in starterTools)
        {
            AddTool(tool);
        }
    }

    public bool AddTool(ScratchToolConfig tool)
    {
        if (tool == null || tool.Id <= 0 || HasTool(tool.Id))
        {
            return false;
        }

        _ownedTools.Add(tool);
        OnToolAdded?.Invoke(tool);
        return true;
    }

    public bool HasTool(int toolId)
    {
        for (int i = 0; i < _ownedTools.Count; i++)
        {
            if (_ownedTools[i] != null && _ownedTools[i].Id == toolId)
            {
                return true;
            }
        }

        return false;
    }
}
