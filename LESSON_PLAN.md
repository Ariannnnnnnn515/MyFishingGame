# Курс «Собираем игру-рыбалку в Unity»

Возраст: 13–15 лет  
Уровень: ученик уже знает основы Unity и C#  
Продолжительность: 5 занятий по 60 минут  
Проект: `MyFishingGame`, Unity `6000.0.77f1`

## Что получится в конце

Ты соберёшь существующие части проекта в один повторяемый игровой цикл:

```text
Главное меню
    ↓
Перемещение по острову
    ↓
Заброс удочки в воду
    ↓
Ожидание поклёвки
    ↓
Мини-игра натяжения лески
    ↓
Рыба поймана или сорвалась
    ↓
Новый заброс
```

Также в игре будут пауза, возврат в главное меню и понятные сообщения на экране.

Мы не создаём новые C#-скрипты, ScriptableObject-ассеты или префабы. Мы работаем только с уже существующими скриптами и объектами сцен. Новые `Canvas`, `Text` и `Slider` создаются внутри `Scene1`, поэтому отдельные файлы для них не нужны.

## Управление финальной версией

| Действие | Клавиша или кнопка |
|---|---|
| Ходьба | `W`, `A`, `S`, `D` |
| Осмотр | движение мыши |
| Прыжок | `Space` |
| Заброс | `Space` или правая кнопка мыши |
| Натянуть леску | удерживать левую кнопку мыши |
| Ослабить леску | отпустить левую кнопку мыши |
| Отменить рыбалку | `R` |
| Пауза | `Esc` |

> Во время проверки заброса удобнее использовать правую кнопку мыши: тогда `Space` остаётся только прыжком.

## Какие части проекта уже существуют

Главные рабочие файлы находятся в `Assets/scripts`.

| Файл | Задача |
|---|---|
| `Player.cs` | движение игрока, прыжок и обзор мышью |
| `PlayerFishingInput.cs` | команды игрока: заброс и отмена рыбалки |
| `FishingController.cs` | связывает все этапы рыбалки |
| `CastingSystem.cs` | анимирует заброс |
| `LineVisualizer.cs` | рисует леску через `LineRenderer` |
| `BiteSystem.cs` | ждёт поклёвку и выбирает рыбу |
| `ReelingSystem.cs` | управляет мини-игрой вываживания |
| `FishData.cs` | описывает один вид рыбы |
| `FishingSpotData.cs` | описывает место ловли и список рыб |
| `IFishable.cs` | задаёт общие возможности пойманной рыбы |
| `MainMenuUI.cs` | кнопки главного меню |
| `PauseUI.cs` | открытие и закрытие паузы |
| `Scenes.cs` | правильные имена игровых сцен |

Главные данные и сцены:

- `Assets/Карась.asset` — готовые данные рыбы;
- `Assets/NewFishingSpot.asset` — готовая точка ловли;
- `Assets/_Project/Scenes/MainMenu.unity` — главное меню;
- `Assets/_Project/Scenes/Scene1.unity` — игровая сцена.

---

# Занятие 1. Знакомство с проектом и подготовка

## Цель

Понять устройство проекта, проверить запуск сцен и подготовить код к дальнейшей работе.

## Результат занятия

- Unity открывает проект без красных ошибок компиляции.
- Кнопка главного меню загружает `Scene1`.
- Игрок может двигаться и осматриваться.
- Ты понимаешь, какой скрипт отвечает за каждый этап рыбалки.

## 0–5 минут. Открываем проект безопасно

1. Открой Unity Hub.
2. Выбери проект `MyFishingGame`.
3. Репозиторий ожидает Unity `6000.0.77f1`. Лучше использовать именно эту версию.
4. Если установлена только другая версия Unity 6, не соглашайся на массовое обновление ассетов без резервной копии проекта.
5. После открытия выбери `Window → General → Console`.
6. Нажми кнопку **Clear** в Console.

Красные сообщения — ошибки, из-за которых код может не запуститься. Жёлтые сообщения — предупреждения: их тоже важно читать, но они не всегда мешают игре.

## 5–15 минут. Проверяем сцены

1. Открой `File → Build Profiles` или `File → Build Settings` — название зависит от версии Unity.
2. В списке сцен должны находиться:
   - `Assets/_Project/Scenes/MainMenu.unity`;
   - `Assets/_Project/Scenes/Scene1.unity`.
3. У обеих сцен должна стоять галочка.
4. Дважды щёлкни `MainMenu.unity` в окне Project.
5. Нажми **Play**.
6. Нажми игровую кнопку начала игры.

Ожидаемый результат: загружается `Scene1`.

Если Unity пишет `Scene 'Scene1' couldn't be loaded`, ещё раз проверь, что `Scene1` добавлена в список сборки и включена.

## 15–25 минут. Строим карту системы

Рыбалка состоит из небольших систем. Главный контроллер не делает всю работу сам — он передаёт задачи другим компонентам.

```mermaid
flowchart LR
    Input[PlayerFishingInput] --> Controller[FishingController]
    Controller --> Cast[CastingSystem]
    Cast --> Line[LineVisualizer]
    Controller --> Bite[BiteSystem]
    Spot[FishingSpotData] --> Bite
    Fish[FishData] --> Spot
    Bite --> Controller
    Controller --> Reel[ReelingSystem]
    Reel --> Controller
    Controller --> Events[События для UI]
    Events --> Input
```

Проговори цепочку своими словами:

1. `PlayerFishingInput` узнаёт, что игрок нажал кнопку заброса.
2. `FishingController` запускает `CastingSystem`.
3. `CastingSystem` двигает конец лески, а `LineVisualizer` рисует её.
4. После заброса `BiteSystem` ждёт случайное время.
5. `BiteSystem` берёт возможную рыбу из `FishingSpotData`.
6. `ReelingSystem` запускает мини-игру.
7. `FishingController` сообщает интерфейсу об улове или срыве через события.

## 25–35 минут. Исправляем имя класса игрока

В Unity имя публичного класса `MonoBehaviour` должно совпадать с именем файла. Сейчас файл называется `Player.cs`, а класс внутри — `PlayerMovement`.

1. Открой `Assets/scripts/Player.cs`.
2. Найди строку:

```csharp
public class PlayerMovement : MonoBehaviour
```

3. Замени её на:

```csharp
public class Player : MonoBehaviour
```

4. Сохрани файл.
5. Вернись в Unity и дождись завершения компиляции.
6. Открой `Scene1` и выбери объект `Capsule`.
7. В Inspector компонент игрока больше не должен показывать `Missing Script`.

Имена приватных методов и полей менять не нужно.

## 35–45 минут. Разбираемся с кодировкой комментариев

Часть старых скриптов сохранена в Windows-1251. В VS Code русские комментарии могут выглядеть как нечитаемые символы или набор странных букв. Это не игровая механика, а способ хранения текста.

Для файлов `BiteSystem.cs`, `FishData.cs`, `FishingController.cs`, `FishingSpotData.cs`, `IFishable.cs`, `LineVisualizer.cs` и `ReelingSystem.cs`:

1. Открой один файл в VS Code.
2. Нажми название кодировки в правом нижнем углу.
3. Выбери **Reopen with Encoding**.
4. Выбери **Cyrillic (Windows 1251)**.
5. Убедись, что русский текст стал читаемым.
6. Снова нажми кодировку.
7. Выбери **Save with Encoding → UTF-8**.
8. Повтори для остальных перечисленных файлов.

Не выбирай случайную кодировку и не сохраняй файл, если текст всё ещё нечитаемый.

`CastingSystem.cs` уже имеет UTF-8, но часть его старых комментариев повреждена. На следующем занятии мы заменим этот короткий файл понятной версией.

В Visual Studio используй `File → Save As`, стрелку рядом с кнопкой **Save**, затем **Save with Encoding**. Сначала открой исходный текст как Windows-1251, а сохраняй как UTF-8.

## 45–55 минут. Проверяем игрока и Inspector

1. Открой `Scene1`.
2. Выбери объект `Capsule`.
3. Проверь компонент `Player`:
   - `Character Controller` ссылается на компонент того же объекта;
   - `Speed` равен `5`;
   - `Jump Force` равен `5`;
   - `Gravity Force` равен `9.81`;
   - `Main Camera` назначена;
   - отсутствие `Animator` пока допустимо, потому что код его не использует.
4. Нажми **Play**.
5. Проверь WASD, мышь и прыжок.
6. Останови Play Mode и проверь Console.

## 55–60 минут. Контрольная точка

Ответь без подсказки:

- Почему `FishingController` называют координатором?
- Чем компонент сцены отличается от `ScriptableObject`?
- Почему имя класса `MonoBehaviour` должно совпадать с именем файла?
- Где сначала искать причину, если игра не реагирует на кнопку?

Мини-задание: нарисуй на бумаге цепочку из пяти главных компонентов рыбалки и подпиши стрелки словами «запускает» или «сообщает».

---

# Занятие 2. Вода, цель и заброс

## Цель

Настроить попадание курсора в воду и увидеть анимированный заброс с леской.

## Результат занятия

- Raycast распознаёт только поверхность воды.
- ПКМ или `Space` запускает заброс.
- Леска появляется во время рыбалки и идёт от удочки к выбранной точке.

## 0–5 минут. Вспоминаем Raycast

Raycast — это невидимый луч. В нашей игре он выходит из камеры через положение курсора и ищет коллайдер на слое `Water`.

Для успешного попадания нужны сразу три условия:

1. объект воды виден камере;
2. у воды есть Collider;
3. объект находится на слое `Water`.

Материал воды сам по себе не ловит луч.

## 5–15 минут. Настраиваем воду

1. Открой `Scene1`.
2. В Hierarchy найди `WaterBlock_50m`.
3. Выбери объект и посмотри поле **Layer** в верхней части Inspector.
4. Выбери слой `Water`.
5. Если Unity спросит, применить ли слой к дочерним объектам, выбери **Yes, change children**.
6. Нажми **Add Component**.
7. Добавь `Box Collider`.
8. Оставь `Is Trigger` выключенным.
9. Нажми **Edit Collider** и убедись в Scene View, что зелёная рамка покрывает поверхность воды.
10. Сохрани сцену сочетанием `Ctrl+S`.

В проекте слой `Water` уже существует под номером 4. Создавать новый слой с похожим названием не нужно.

## 15–25 минут. Проверяем входные ссылки

Выбери `Capsule` и найди компонент `PlayerFishingInput`.

Проверь:

- `Current Fishing Spot` содержит `NewFishingSpot`;
- `Cast Target` оставлен пустым — тогда используется Raycast от мыши;
- `Max Cast Distance` равен `100`.

Теперь найди объекты систем:

1. Объект `FishingController` должен ссылаться на:
   - компонент `CastingSystem`;
   - объект `BiteSystem`;
   - объект `ReelingSystem`;
   - данные `NewFishingSpot`;
   - данные `Карась`.
2. На объекте с `CastingSystem` должны быть назначены:
   - `Line Visual` — компонент `LineVisualizer`;
   - `Cast Origin` — маленький Transform на конце удочки.
3. На том же объекте должен быть `LineRenderer`.
4. У `LineRenderer` включи **Use World Space**.
5. Поставь небольшую ширину лески, например `0.02` в начале и в конце.

## 25–40 минут. Приводим CastingSystem в понятный вид

Полностью замени содержимое существующего `Assets/scripts/CastingSystem.cs` следующим кодом. Новый файл создавать не нужно.

```csharp
using System;
using UnityEngine;
using Fishing.Core;
using Fishing.Visual;

namespace Fishing.Systems
{
    public class CastingSystem : MonoBehaviour
    {
        [Header("Настройки заброса")]
        [SerializeField] private float castDuration = 1.5f;
        [SerializeField] private AnimationCurve heightCurve =
            AnimationCurve.EaseInOut(0, 0, 1, 0);

        [Header("Ссылки")]
        [SerializeField] private LineVisualizer lineVisual;
        [SerializeField] private Transform castOrigin;

        private Action onCompleteCallback;
        private Vector3 targetPosition;
        private float castProgress;
        private bool isCasting;

        public void Initialize(FishingController controller)
        {
            // Контроллер пока не нужен: результат возвращаем через callback.
        }

        public void StartCast(Vector3 target, Action callback)
        {
            targetPosition = target;
            onCompleteCallback = callback;
            castProgress = 0f;
            isCasting = true;

            lineVisual?.EnableLine(true);
            Debug.Log($"Заброс в точку {targetPosition}");
        }

        private void Update()
        {
            if (!isCasting)
                return;

            castProgress += Time.deltaTime / castDuration;

            if (castProgress >= 1f)
            {
                lineVisual?.UpdateLine(castOrigin.position, targetPosition);
                isCasting = false;
                onCompleteCallback?.Invoke();
                return;
            }

            Vector3 currentPosition = Vector3.Lerp(
                castOrigin.position,
                targetPosition,
                castProgress
            );

            currentPosition.y += heightCurve.Evaluate(castProgress) * 2f;
            lineVisual?.UpdateLine(castOrigin.position, currentPosition);
        }

        public void ResetCast()
        {
            isCasting = false;
            lineVisual?.EnableLine(false);
        }
    }
}
```

Что здесь важно:

- `Vector3.Lerp` двигает конец лески от удочки к воде;
- `heightCurve` добавляет высоту и превращает прямое движение в дугу;
- callback сообщает контроллеру, что заброс завершён;
- `?.` вызывает метод только тогда, когда ссылка не равна `null`.

## 40–45 минут. Скрываем леску до заброса

Открой `LineVisualizer.cs`. В методе `Awake` после настройки количества точек добавь одну строку:

```csharp
private void Awake()
{
    lineRenderer = GetComponent<LineRenderer>();
    lineRenderer.positionCount = segments;
    lineRenderer.enabled = false; // До заброса леску не показываем.
}
```

## 45–55 минут. Проверяем заброс

1. Вернись в Unity и дождись компиляции.
2. Нажми **Play** в `Scene1`.
3. Наведи центр взгляда или курсор на воду.
4. Нажми ПКМ.
5. В Console должны появиться сообщения о забросе и ожидании поклёвки.
6. Леска должна закончиться на воде.
7. Нажми `R`, чтобы освободить управление перед следующим тестом.
8. Посмотри в небо или на сушу и снова нажми ПКМ.

При клике мимо воды должно появиться предупреждение «Не удалось определить цель для заброса!», а заброс не должен запускаться.

## 55–60 минут. Контрольная точка

Проверь себя:

- Как Collider помогает Raycast?
- Почему одного слоя `Water` недостаточно?
- Что хранится в `castOrigin`?
- Для чего нужен callback в `StartCast`?

Мини-задание: измени `castDuration` на `0.7`, затем на `2.5`, сравни забросы и верни комфортное значение около `1.5`.

---

# Занятие 3. Рыбы, поклёвка и события

## Цель

Настроить данные рыбы, исправить подписку на события и показывать состояние рыбалки на экране.

## Результат занятия

- После заброса система выбирает карася.
- Игрок получает сообщение о поклёвке.
- После улова или срыва состояние на экране меняется.

## 0–5 минут. Данные отдельно от поведения

`FishData` и `FishingSpotData` наследуются от `ScriptableObject`. Они хранят настройки, но не выполняют действия каждый кадр.

Это удобно: один скрипт поведения может работать с разными рыбами и озёрами, если передать ему другие данные.

## 5–15 минут. Проверяем Карася

Выбери `Assets/Карась.asset`.

Проверь значения:

| Поле | Учебное значение | Значение |
|---|---:|---|
| `Fish Name` | `Карась` | имя в сообщениях |
| `Weight Min` | `0.2` | минимальный вес |
| `Weight Max` | `1.5` | максимальный вес |
| `Base Resistance` | `0.5` | базовое сопротивление |
| `Escape Speed` | `0.3` | скорость, с которой рыба устаёт |
| `Experience Reward` | `10` | будущая награда |

Поля `Fish Prefab` и `Icon In UI` пока могут оставаться пустыми: текущий MVP не создаёт модель рыбы в руках и не использует инвентарь.

## 15–20 минут. Проверяем место ловли

Выбери `Assets/NewFishingSpot.asset`.

Проверь:

- `Spot Name` — «Лесное озеро»;
- в `Fish Pool` есть один элемент;
- его `Fish Data` — `Карась`;
- `Spawn Weight` — `100`;
- `Bite Chance Modifier` — `1`.

Вес `100` не означает 100 рыб. Это относительный шанс. Если позже добавить рыбу с весом `20`, карась с весом `100` будет выбираться примерно в пять раз чаще.

На объекте `BiteSystem` в `Scene1` временно поставь:

- `Min Wait Time = 2`;
- `Max Wait Time = 5`.

Так тест не будет занимать почти весь урок.

## 20–40 минут. Исправляем PlayerFishingInput

Сейчас `OnEnable` вызывается раньше `Start`, поэтому ссылка на `FishingController` ещё не получена и события не подписываются. Полностью замени содержимое существующего `PlayerFishingInput.cs` кодом ниже.

```csharp
using TMPro;
using UnityEngine;
using Fishing.Core;
using Fishing.Core.Data;

public class PlayerFishingInput : MonoBehaviour
{
    [Header("Рыбалка")]
    [SerializeField] private FishingSpotData currentFishingSpot;
    [SerializeField] private Transform castTarget;
    [SerializeField] private float maxCastDistance = 100f;

    [Header("Интерфейс")]
    [SerializeField] private TMP_Text statusText;

    private FishingController fishingController;
    private bool isFishingActive;

    private void Start()
    {
        fishingController = FishingController.Instance;

        if (fishingController == null)
        {
            Debug.LogError("FishingController не найден на сцене!");
            enabled = false;
            return;
        }

        fishingController.OnFishHooked += OnFishHooked;
        fishingController.OnFishLanded += OnFishLanded;
        fishingController.OnFishEscaped += OnFishEscaped;

        ShowStatus("Наведи курсор на воду и нажми ПКМ для заброса.");
    }

    private void Update()
    {
        bool pressedCast = Input.GetKeyDown(KeyCode.Space) ||
                           Input.GetMouseButtonDown(1);

        if (pressedCast && !isFishingActive)
            PerformCast();

        if (Input.GetKeyDown(KeyCode.R) && isFishingActive)
            fishingController.OnFishEscape();
    }

    private void PerformCast()
    {
        if (currentFishingSpot == null)
        {
            Debug.LogError("В PlayerFishingInput не назначен Current Fishing Spot!");
            return;
        }

        Vector3 targetPosition = castTarget != null
            ? castTarget.position
            : GetMouseTarget();

        if (targetPosition == Vector3.zero)
        {
            Debug.LogWarning("Не удалось определить цель для заброса!");
            return;
        }

        fishingController.PerformCast(targetPosition, currentFishingSpot);
        isFishingActive = true;
        ShowStatus("Заброс выполнен. Ждём поклёвку...");
    }

    private Vector3 GetMouseTarget()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Камера с тегом MainCamera не найдена!");
            return Vector3.zero;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int waterLayer = LayerMask.GetMask("Water");

        if (Physics.Raycast(ray, out RaycastHit hit, maxCastDistance, waterLayer))
            return hit.point;

        return Vector3.zero;
    }

    private void OnDestroy()
    {
        if (fishingController == null)
            return;

        fishingController.OnFishHooked -= OnFishHooked;
        fishingController.OnFishLanded -= OnFishLanded;
        fishingController.OnFishEscaped -= OnFishEscaped;
    }

    private void OnFishHooked(FishData fish)
    {
        isFishingActive = true;
        ShowStatus($"Поклёвка! {fish.fishName} на крючке!");
    }

    private void OnFishLanded(FishData fish)
    {
        float weight = Random.Range(fish.weightMin, fish.weightMax);
        isFishingActive = false;
        ShowStatus($"Поймана рыба: {fish.fishName}, {weight:F1} кг!");
    }

    private void OnFishEscaped()
    {
        isFishingActive = false;
        ShowStatus("Рыбалка завершена. Можно сделать новый заброс.");
    }

    private void ShowStatus(string message)
    {
        Debug.Log(message);

        if (statusText != null)
            statusText.text = message;
    }
}
```

Почему подписка теперь работает:

- все `Awake` выполняются до `Start`;
- `FishingController` создаёт `Instance` в `Awake`;
- `PlayerFishingInput` получает готовый `Instance` в своём `Start`;
- `OnDestroy` удаляет подписки, чтобы старый объект не продолжал получать события.

## 40–50 минут. Создаём StatusText в сцене

1. Открой `Scene1`.
2. В Hierarchy выбери `GameObject → UI → Canvas`.
3. Назови Canvas `FishingHUD`.
4. У компонента `Canvas` оставь `Render Mode = Screen Space - Overlay`.
5. У `Canvas Scaler` выбери `Scale With Screen Size`.
6. Поставь `Reference Resolution = 1920 × 1080` и `Match = 0.5`.
7. Если Unity автоматически создала второй `EventSystem`, оставь в сцене только один.
8. Щёлкни правой кнопкой по `FishingHUD` и выбери `UI → Text - TextMeshPro`.
9. Если Unity попросит импортировать TMP Essentials, нажми **Import TMP Essentials**. В этом проекте они уже должны присутствовать.
10. Назови объект `StatusText`.
11. Закрепи его сверху по центру.
12. Поставь размер примерно `900 × 100`, размер шрифта `32`, выравнивание по центру.
13. Выбери `Capsule`.
14. В компоненте `PlayerFishingInput` перетащи `StatusText` в поле `Status Text`.
15. Сохрани сцену.

## 50–55 минут. Проверяем события

1. Запусти `Scene1`.
2. Сделай заброс.
3. Увидь сообщение «Ждём поклёвку...».
4. Подожди 2–5 секунд.
5. Увидь сообщение «Поклёвка!».
6. Пока мини-игра ещё не исправлена, рыба может сорваться — это нормально.
7. Нажми `R` и проверь, что сообщение разрешает новый заброс.

У `BiteSystem` есть 10% шанс «ложной поклёвки», поэтому иногда ожидание начнётся ещё раз.

## 55–60 минут. Контрольная точка

- Чем событие отличается от прямого вызова метода UI?
- Почему мы отписываемся от событий?
- Что означает `spawnWeight`?
- Зачем данные рыбы вынесены из `FishingController`?

Мини-задание: временно поставь `Bite Chance Modifier = 2`, измерь ожидание, затем верни `1`.

---

# Занятие 4. Мини-игра вываживания

## Цель

Сделать понятное управление натяжением лески и визуальную обратную связь.

## Результат занятия

- При поклёвке появляется панель со Slider.
- Удержание ЛКМ увеличивает натяжение, отпускание уменьшает.
- Подсказка помогает удерживать леску в правильной зоне.
- Рыбу можно поймать до окончания таймера.

## 0–10 минут. Правила мини-игры

У нас есть два числа от `0` до `1`:

- `CurrentTension` — натяжение, которым управляет игрок;
- `FishResistance` — сопротивление текущей рыбы.

Если разница между числами меньше половины `targetZoneSize`, натяжение считается правильным и рыба устаёт. Если игрок слишком долго находится вне зоны, таймер заканчивается и рыба срывается.

## 10–25 минут. Создаём панель

1. В `Scene1` выбери `FishingHUD`.
2. Создай `UI → Panel` и назови его `ReelingPanel`.
3. Размести панель внизу по центру, например размером `700 × 180`.
4. Внутри панели создай `UI → Slider` с именем `TensionSlider`.
5. У Slider установи:
   - `Min Value = 0`;
   - `Max Value = 1`;
   - `Value = 0`;
   - `Whole Numbers` выключено;
   - `Interactable` выключено.
6. Внутри панели создай `Text - TextMeshPro` с именем `HintText`.
7. Поставь текст по центру над Slider и размер шрифта около `28`.
8. Сними галочку активности с `ReelingPanel`, чтобы до поклёвки он был скрыт.

## 25–40 минут. Заменяем ReelingSystem

Полностью замени содержимое существующего `Assets/scripts/ReelingSystem.cs`:

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fishing.Core;
using Fishing.Core.Interfaces;

namespace Fishing.Systems
{
    public class ReelingSystem : MonoBehaviour
    {
        [Header("Настройки мини-игры")]
        [SerializeField] private float fightDuration = 10f;
        [SerializeField] private float targetZoneSize = 0.3f;
        [SerializeField] private float tensionMultiplier = 0.7f;

        [Header("Интерфейс")]
        [SerializeField] private GameObject reelingUI;
        [SerializeField] private Slider tensionSlider;
        [SerializeField] private TMP_Text hintText;

        private FishingController controller;
        private IFishable currentFish;
        private float fightTimer;
        private bool isFighting;

        public float CurrentTension { get; private set; }
        public float FishResistance => currentFish?.CurrentResistance ?? 0f;

        public void Initialize(FishingController fishingController)
        {
            controller = fishingController;
        }

        public void StartFight(IFishable fish)
        {
            currentFish = fish;
            fightTimer = 0f;
            CurrentTension = 0f;
            isFighting = true;

            if (reelingUI != null)
                reelingUI.SetActive(true);

            UpdateUI(false);
            Debug.Log("Мини-игра началась: удерживай и отпускай ЛКМ.");
        }

        private void Update()
        {
            if (!isFighting || currentFish == null)
                return;

            float wantedTension = Input.GetMouseButton(0) ? 1f : 0f;

            CurrentTension = Mathf.MoveTowards(
                CurrentTension,
                wantedTension,
                tensionMultiplier * Time.deltaTime
            );

            float halfZone = targetZoneSize / 2f;
            bool isInTargetZone =
                CurrentTension >= FishResistance - halfZone &&
                CurrentTension <= FishResistance + halfZone;

            UpdateUI(isInTargetZone);

            if (isInTargetZone)
            {
                if (currentFish.ApplyTension(CurrentTension))
                {
                    OnFishTired();
                    return;
                }
            }
            else
            {
                currentFish.ApplyTension(0f);
            }

            fightTimer += Time.deltaTime;

            if (fightTimer >= fightDuration)
            {
                controller.OnFishEscape();
                StopFight();
            }
        }

        private void UpdateUI(bool isInTargetZone)
        {
            if (tensionSlider != null)
                tensionSlider.value = CurrentTension;

            if (hintText == null)
                return;

            string instruction;

            if (isInTargetZone)
                instruction = "Держи так!";
            else if (CurrentTension < FishResistance)
                instruction = "Зажми ЛКМ — натяни леску";
            else
                instruction = "Отпусти ЛКМ — ослабь леску";

            hintText.text =
                $"Леска: {CurrentTension:F2} | Цель: {FishResistance:F2}\n" +
                instruction;
        }

        private void OnFishTired()
        {
            controller.OnFishTired();
            StopFight();
        }

        public void StopFight()
        {
            isFighting = false;
            currentFish = null;
            CurrentTension = 0f;

            if (tensionSlider != null)
                tensionSlider.value = 0f;

            if (reelingUI != null)
                reelingUI.SetActive(false);
        }

        public void ForceEscape()
        {
            if (isFighting)
                controller.OnFishEscape();

            StopFight();
        }
    }
}
```

## 40–47 минут. Исправляем сопротивление рыбы

Открой `FishingController.cs`. Внутри вложенного класса `FishInstance` найди свойство `CurrentResistance` и замени его:

```csharp
public float CurrentResistance => Mathf.Clamp(maxResistance, 0.25f, 0.8f);
```

Теперь найди метод `ApplyTension` внутри того же класса и полностью замени его:

```csharp
public bool ApplyTension(float tensionPower)
{
    // ReelingSystem передаёт значение больше нуля только в верной зоне.
    if (tensionPower > 0f)
        currentTiredness += Time.deltaTime * data.escapeSpeed;
    else
        currentTiredness -= Time.deltaTime * 0.15f;

    currentTiredness = Mathf.Clamp01(currentTiredness);

    if (currentTiredness >= 1f)
    {
        State = FishState.Tired;
        return true;
    }

    return false;
}
```

`maxResistance` уже вычисляется из базового сопротивления и случайного веса рыбы. `Mathf.Clamp` не даёт цели стать слишком лёгкой или недостижимой.

## 47–52 минуты. Соединяем UI и код

1. Вернись в Unity и дождись компиляции.
2. Выбери объект `ReelingSystem`.
3. Проверь параметры:
   - `Fight Duration = 10`;
   - `Target Zone Size = 0.3`;
   - `Tension Multiplier = 0.7`.
4. Перетащи `ReelingPanel` в поле `Reeling UI`.
5. Перетащи `TensionSlider` в поле `Tension Slider`.
6. Перетащи `HintText` в поле `Hint Text`.
7. Сохрани сцену.

Если поля не появились, сначала посмотри Console. Unity не обновит Inspector, пока в проекте есть ошибка компиляции.

## 52–57 минут. Играем

1. Запусти сцену и сделай заброс.
2. Дождись поклёвки.
3. Удерживай ЛКМ, пока значение лески приближается к цели.
4. Отпусти ЛКМ, если значение стало слишком большим.
5. Старайся удерживать подсказку «Держи так!».
6. Поймай рыбу до окончания десяти секунд.
7. Повтори и специально ничего не нажимай, чтобы увидеть срыв.

## 57–60 минут. Контрольная точка

- Зачем используется `Mathf.MoveTowards`?
- Почему Slider имеет диапазон от 0 до 1?
- Чем `CurrentTension` отличается от `FishResistance`?
- Где определяется ширина правильной зоны?

Мини-задание: сравни `Target Zone Size = 0.15` и `0.5`, затем верни `0.3`.

---

# Занятие 5. Собираем единый игровой цикл

## Цель

Правильно завершать каждый этап, исправить переходы между сценами и проверить игру несколько раз подряд.

## Результат занятия

- После улова, срыва и отмены очищаются леска, UI, корутина и мини-игра.
- Игрок всегда может начать следующий заброс.
- Пауза и главное меню не замораживают новую игру.
- Полный цикл проходит не менее трёх раз без ошибок.

## 0–10 минут. Почему нужна очистка состояния

Одновременно могут работать:

- анимация заброса;
- корутина ожидания;
- мини-игра;
- видимая леска;
- UI-панель.

Если остановить только одну часть, остальные продолжат работу. Например, после `R` старая корутина может всё равно вызвать поклёвку. Поэтому завершение должно выключать все системы в одном месте.

## 10–25 минут. Исправляем FishingController

Открой `FishingController.cs`.

После событий добавь поле:

```csharp
private bool isFishingInProgress;
```

Полностью замени метод `PerformCast`:

```csharp
public void PerformCast(Vector3 targetPosition, FishingSpotData spot)
{
    if (isFishingInProgress)
    {
        Debug.LogWarning("Сначала заверши текущую рыбалку!");
        return;
    }

    if (spot == null)
    {
        Debug.LogError("Для заброса не выбрана FishingSpotData!");
        return;
    }

    isFishingInProgress = true;
    currentSpot = spot;
    castingSystem.StartCast(targetPosition, OnCastComplete);
}
```

Полностью замени `OnBiteOccurred`:

```csharp
public void OnBiteOccurred(FishData fishData)
{
    // Защита от запоздавшей корутины после отмены.
    if (!isFishingInProgress || fishData == null)
        return;

    currentFishData = fishData;
    CurrentFish = new FishInstance(fishData);

    CurrentFish.OnHooked();
    OnFishHooked?.Invoke(fishData);
    reelingSystem.StartFight(CurrentFish);
}
```

Полностью замени `OnFishTired` и `OnFishEscape`, а затем добавь общий метод очистки:

```csharp
public void OnFishTired()
{
    if (CurrentFish == null || currentFishData == null)
        return;

    CurrentFish.State = FishState.Landed;
    FishData landedFish = currentFishData;

    Debug.Log($"Рыба {landedFish.fishName} поймана!");
    ResetFishingSystems();
    OnFishLanded?.Invoke(landedFish);
}

public void OnFishEscape()
{
    if (!isFishingInProgress)
        return;

    CurrentFish?.OnEscape();
    ResetFishingSystems();
    OnFishEscaped?.Invoke();
    Debug.Log("Рыбалка завершена без улова.");
}

private void ResetFishingSystems()
{
    biteSystem.StopWaiting();
    reelingSystem.StopFight();
    castingSystem.ResetCast();

    CurrentFish = null;
    currentFishData = null;
    currentSpot = null;
    isFishingInProgress = false;
}
```

Теперь `R`, таймер проигрыша и успешный улов используют одну и ту же очистку.

## 25–32 минуты. Делаем ожидание устойчивым

В `BiteSystem.cs` замени метод `WaitForBite`. Цикл повторяет ожидание после ложной поклёвки, но легко останавливается через `StopWaiting`.

```csharp
private IEnumerator WaitForBite()
{
    while (currentSpot != null)
    {
        float modifier = Mathf.Max(0.1f, currentSpot.biteChanceModifier);
        float waitTime = Random.Range(minWaitTime, maxWaitTime) / modifier;
        yield return new WaitForSeconds(waitTime);

        FishData selectedFish = SelectFishFromPool();

        if (selectedFish == null)
        {
            Debug.LogWarning("В точке ловли нет рыбы!");
            continue;
        }

        if (Random.value < 0.1f)
        {
            Debug.Log("Ложная поклёвка. Ждём ещё...");
            continue;
        }

        waitingCoroutine = null;
        controller.OnBiteOccurred(selectedFish);
        yield break;
    }

    waitingCoroutine = null;
}
```

В конец `StopWaiting` добавь очистку точки:

```csharp
public void StopWaiting()
{
    if (waitingCoroutine != null)
    {
        StopCoroutine(waitingCoroutine);
        waitingCoroutine = null;
    }

    currentSpot = null;
}
```

## 32–40 минут. Исправляем имена сцен

Полностью замени содержимое существующего `Scenes.cs`:

```csharp
public static class Scenes
{
    public const string MainMenu = "MainMenu";
    public const string Gameplay = "Scene1";
}
```

Полностью замени `MainMenuUI.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(Scenes.Gameplay);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }
}
```

В `PauseUI.cs` замени только `BackToMainMenu`:

```csharp
public void BackToMainMenu()
{
    Time.timeScale = 1f;
    Cursor.lockState = CursorLockMode.None;
    SceneManager.LoadScene(Scenes.MainMenu);
}
```

Без `Time.timeScale = 1f` следующая игровая сцена может загрузиться в замороженном состоянии.

## 40–48 минут. Проверяем кнопки и паузу

В `MainMenu`:

1. Выбери кнопку начала игры.
2. В списке `On Click()` должен быть объект с `MainMenuUI`.
3. Выбери метод `MainMenuUI → StartGame`.
4. Для кнопки выхода выбери `MainMenuUI → ExitGame`.

В `Scene1`:

1. Выбери объект с компонентом `PauseUI`.
2. В поле `Toggle` должна быть назначена команда `UI/OpenPauseMenu` из `InputSystem_Actions`.
3. Эта команда уже привязана к `Esc`.
4. В поле `Root` должна быть назначена панель паузы, а не весь Canvas игрового HUD.
5. На кнопке продолжения выбери `PauseUI → BackToGame`.
6. На кнопке выхода в меню выбери `PauseUI → BackToMainMenu`.
7. До запуска панель паузы должна быть выключена.

## 48–57 минут. Финальный тест

Открой `MainMenu` и начни игру именно оттуда.

| № | Действие | Ожидаемый результат |
|---:|---|---|
| 1 | Нажать начало игры | открывается `Scene1` |
| 2 | Пройтись и осмотреться | движение и камера работают |
| 3 | Нажать ПКМ по суше | появляется предупреждение, заброса нет |
| 4 | Нажать ПКМ по воде | появляется леска и сообщение ожидания |
| 5 | Снова нажать заброс | второй процесс не начинается |
| 6 | Дождаться поклёвки | появляется `ReelingPanel` |
| 7 | Удерживать и отпускать ЛКМ | Slider плавно движется |
| 8 | Удержать правильную зону | появляется сообщение об улове |
| 9 | Сделать новый заброс | цикл начинается заново |
| 10 | Нажать `R` во время ожидания | леска скрывается, новый заброс доступен |
| 11 | Нажать `R` во время мини-игры | UI скрывается, старый таймер не срабатывает |
| 12 | Нажать `Esc` два раза | пауза открывается и закрывается |
| 13 | Из паузы выйти в меню | открывается `MainMenu` |
| 14 | Снова начать игру | движение и таймеры не заморожены |

Пройди полный цикл «заброс → поклёвка → результат» три раза. После каждого раза проверяй Console.

## 57–60 минут. Итог

Объясни своими словами:

- какие системы запускаются после нажатия ПКМ;
- зачем контроллеру общий метод очистки;
- как события связывают игровую механику и UI;
- какие настройки можно менять без переписывания механики.

Мини-задание после курса: подбери собственные значения времени ожидания, ширины правильной зоны и длительности борьбы. Меняй только одно значение за тест — так легче понять его влияние.

---

# Полная проверка MVP

Поставь отметку после успешной проверки каждого пункта.

- [ ] Unity компилирует проект без красных ошибок.
- [ ] В сцене нет компонентов `Missing Script`.
- [ ] `MainMenu` загружает `Scene1`.
- [ ] WASD, мышь и прыжок работают.
- [ ] Вода имеет `BoxCollider` и слой `Water`.
- [ ] Заброс мимо воды не запускается.
- [ ] Заброс в воду показывает леску.
- [ ] Во время одной рыбалки нельзя создать второй процесс.
- [ ] `NewFishingSpot` выбирает `Карась`.
- [ ] Поклёвка отображается в `StatusText`.
- [ ] При поклёвке открывается `ReelingPanel`.
- [ ] Удержание и отпускание ЛКМ изменяет Slider.
- [ ] Правильное натяжение позволяет поймать рыбу.
- [ ] По окончании таймера рыба срывается.
- [ ] После улова можно забросить снова.
- [ ] После срыва можно забросить снова.
- [ ] `R` отменяет заброс, ожидание и мини-игру.
- [ ] После завершения леска скрывается.
- [ ] `Esc` открывает и закрывает паузу.
- [ ] Возврат в меню восстанавливает `Time.timeScale`.
- [ ] Полный игровой цикл работает три раза подряд.

# Если что-то не работает

## Unity не показывает новые поля в Inspector

1. Открой Console.
2. Исправь первую красную ошибку — остальные могут быть её следствием.
3. Проверь фигурные скобки и точки с запятой.
4. Проверь, что `using TMPro;` и `using UnityEngine.UI;` находятся в начале нужного файла.
5. Дождись окончания компиляции.

## На компоненте написано Missing Script

- Убедись, что в `Player.cs` объявлен `public class Player : MonoBehaviour`.
- У остальных компонентов имя класса должно совпадать с именем файла.
- Не создавай копии классов с такими же именами.

## Вода видна, но заброс не работает

- У `WaterBlock_50m` должен быть `BoxCollider`.
- Его слой должен называться точно `Water`.
- `Max Cast Distance` должен быть достаточно большим, например `100`.
- Камера должна иметь тег `MainCamera`.
- Зелёная рамка Collider должна покрывать поверхность воды.

## Появляется NullReferenceException

Щёлкни по сообщению в Console: Unity откроет строку, где отсутствует ссылка. Затем проверь Inspector.

Чаще всего забывают назначить:

- три системы в `FishingController`;
- `Line Visual` и `Cast Origin` в `CastingSystem`;
- `Current Fishing Spot` в `PlayerFishingInput`;
- `Status Text` в `PlayerFishingInput`;
- `Reeling UI`, `Tension Slider` и `Hint Text` в `ReelingSystem`;
- `Root` и `Toggle` в `PauseUI`.

## Поклёвка не происходит

- Проверь, что `NewFishingSpot` назначен и в нём есть `Карась`.
- `Spawn Weight` должен быть больше нуля.
- `Bite Chance Modifier` должен быть больше нуля.
- Для теста поставь ожидание 2–5 секунд.
- Помни о 10% вероятности ложной поклёвки.

## Панель натяжения не появляется

- `ReelingPanel` должен быть назначен в `Reeling UI`.
- В начале сцены панель выключена — это правильно.
- Сам объект `FishingHUD` должен быть включён.
- Проверь, появилось ли сообщение о поклёвке.

## Slider не двигается

- У Slider должны быть пределы `0` и `1`.
- Проверь ссылку `Tension Slider`.
- ЛКМ нужно удерживать после поклёвки, а не во время ожидания.
- Убедись, что игра не стоит на паузе и `Time.timeScale` не равен нулю.

## Событие срабатывает несколько раз

- В `PlayerFishingInput` подписка должна находиться только в `Start`.
- Отписка должна находиться только в `OnDestroy`.
- Удали старые методы `OnEnable` и `OnDisable` из этого скрипта после полной замены.

## Сцена не найдена

- Проверь точное имя файла: `MainMenu` и `Scene1`.
- Проверь константы в `Scenes.cs`.
- Добавь обе сцены в Build Settings или Build Profiles.
- Не используй старое имя `GamePlay`.

## После меню игра заморожена

- В `PauseUI.BackToMainMenu` должна быть строка `Time.timeScale = 1f;`.
- В `MainMenuUI.StartGame` тоже должна быть страховочная строка `Time.timeScale = 1f;`.

# Что можно добавить позже

Эти идеи не входят в пять занятий и потребуют отдельного планирования:

- несколько видов рыб и разные шансы выпадения;
- изображение пойманной рыбы;
- инвентарь и продажа улова;
- магазин наживок;
- опыт и уровни игрока;
- звук всплеска и частицы воды;
- анимация персонажа и удочки;
- сохранение прогресса;
- сборка игры для Windows или WebGL.

Главное правило развития проекта: сначала добейся стабильного маленького цикла, затем добавляй только одну новую механику и снова полностью тестируй игру.
