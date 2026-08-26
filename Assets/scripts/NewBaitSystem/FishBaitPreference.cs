using UnityEngine;
using System;

[Serializable] // Чтобы видеть в инспекторе
public class FishBaitPreference
{
    public BaitData bait;       // Какая наживка
    [Range(0, 100)] public int chance; // Шанс поймать эту рыбу на эту наживку (в %)
}