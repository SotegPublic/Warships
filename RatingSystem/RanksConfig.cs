using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(RanksConfig), menuName = nameof(RanksConfig), order = 0)]
[Serializable]
public class RanksConfig : ScriptableObject
{
    [SerializeField][HideInInspector] public int MaxRanksCount;
    [SerializeField][HideInInspector] public List<RankData> RanksList = new List<RankData>();

    public void AddLevel()
    {
        RanksList.Add(new RankData());
    }

    public void RemoveLastLevel()
    {
        RanksList.RemoveAt(RanksList.Count - 1);
    }

    public void ClearList()
    {
        RanksList.Clear();
    }

    public string GetRankName(float rating)
    {
        for(int i = 0; i < RanksList.Count; i++)
        {
            if (RanksList[i].MaxRatingForRank >= rating)
            {
                return RanksList[i].Rank;
            }
        }

        return RanksList[RanksList.Count - 1].Rank;
    }
}
