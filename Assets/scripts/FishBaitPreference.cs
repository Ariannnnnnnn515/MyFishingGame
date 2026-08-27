using System;
using UnityEngine;

namespace Fishing.Core.Data // <-- ÒÎ ÆÅ ÏÐÎÑÒÐÀÍÑÒÂÎ!
{
    [Serializable]
    public class FishBaitPreference
    {
        public BaitData bait;
        [Range(0, 100)] public int chance;
    }
}