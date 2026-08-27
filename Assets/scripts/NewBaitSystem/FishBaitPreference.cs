using System;
using UnityEngine;

// ”бираем пространство имен или оставл€ем то же, что и в FishData
// ≈сли в FishData используетс€ namespace Fishing.Core.Data, то и здесь должно быть так же
namespace Fishing.Core.Data
{
    [Serializable]
    public class FishBaitPreference
    {
        [Tooltip("Ќаживка, дл€ которой настраиваетс€ шанс")]
        public BaitData bait;

        [Tooltip("Ўанс поклевки (0-100%)")]
        [Range(0, 100)]
        public int chance = 50;
    }
}