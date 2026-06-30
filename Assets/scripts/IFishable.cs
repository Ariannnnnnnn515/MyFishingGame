using UnityEngine;

namespace Fishing.Core.Interfaces
{
    /// <summary>
    /// Интерфейс для объектов, которые могут быть пойманы (рыба, мусор, сокровища).
    /// </summary>
    public interface IFishable
    {
        /// <summary> Уникальный ID вида </summary>
        string SpeciesId { get; }

        /// <summary> Текущее состояние рыбы (спокойна, борется, устала) </summary>
        FishState State { get; set; }

        /// <summary> Сопротивление при вываживании (0-1) </summary>
        float CurrentResistance { get; }

        /// <summary> Вызывается при засечке </summary>
        void OnHooked();

        /// <summary> Обновление состояния при тяге (возвращает true, если устала) </summary>
        bool ApplyTension(float tensionPower);

        /// <summary> Вызывается при сходе рыбы </summary>
        void OnEscape();
    }

    public enum FishState { Idle, Hooked, Fighting, Tired, Landed }
}