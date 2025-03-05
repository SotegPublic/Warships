using System;
using UnityEngine;

[Serializable]
public class RankData
{
    [SerializeField][HideInInspector] public float MaxRatingForRank;
    [SerializeField][HideInInspector] public string Rank;
}
