using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelToCells
{
    public byte Level;
    public byte CellsCount;

    public static byte CalculateInventoryAvaliableCells(sbyte level, List<LevelToCells> avaliableCellsOptions)
    {
        byte avaliableCells = 0;
        foreach (var option in avaliableCellsOptions)
        {
            if (level >= option.Level)
                avaliableCells = option.CellsCount;
            else
                break;
        }

        return avaliableCells;
    }
}