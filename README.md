<div align="center">

# 🚨 THE WARD — Tactical Dispatch Simulator

**Тактический симулятор диспетчера экстренных служб с кастомной физикой UI и процедурным расчетом рисков**

[![Unity](https://img.shields.io/badge/Engine-Unity%206-black?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Architecture](https://img.shields.io/badge/Architecture-Event--Driven%20%7C%20SO-blue)]()

[**🇷🇺 Русская версия**](#-о-проекте) | [**🇬🇧 English Version (Click below or scroll)**](#-english-version)

</div>

---

<details>
<summary><b>🌐 Switch to English / Read in English (Click to expand)</b></summary>

<br>

## 🎯 Project Overview (English Version)

**The Ward** is a tactical dispatch management prototype inspired by *This Is the Police*. The player manages two shifts of operatives, evaluates incoming emergency calls across city districts, assembles squads via Drag-and-Drop UI, and resolves high-stakes situations through an interactive radar-chart physics mini-game. Additionally, the project includes an interactive first-aid reference module (`MainMenuManager`, `EmergencyTopic`).

### ⚙️ Technical Highlights (Unity / C#)
* **Decoupled Event-Driven Architecture:** Zero runtime `FindObjectOfType` or `GameObject.Find` lookups during gameplay loops. Map nodes (`MissionNode`), moving units (`SquadController`), and UI panels communicate strictly via `System.Action` callbacks and C# events (`RosterManager.OnRosterChanged`).
* **Custom Procedural UI Mesh (`UI_RadarChart`):** Inherits directly from `UnityEngine.UI.Graphic` and overrides `OnPopulateMesh(VertexHelper vh)` to dynamically render 5-axis polygon radar charts for mission requirements and squad synergy without relying on external sprite assets.
* **Polar Coordinate 2D Physics (`MissionBall`):** Custom unscaled-time (`Time.unscaledDeltaTime`) ball physics operating in polar coordinates (`Mathf.Atan2`), calculating real-time boundary reflections against the procedural polygon edges of `UI_RadarChart`.
* **Data-Driven Configuration (`ScriptableObject`):** Operatives (`AgentProfileSO`) and missions (`MissionSO`) are completely decoupled from scene logic, allowing game designers to balance stats, spawn delays, and fatigue penalties directly in the Inspector.
* **Custom Editor Tooling (`DistrictRoute`):** Uses `[ExecuteAlways]`, `OnValidate`, and `Gizmos` to bake waypoint hierarchies into `LineRenderer` paths directly in the Unity Editor view.

### 📊 System Game Design & Math Model
1. **Fatigue & Stat Degradation (`Agent.cs`):**
   Each operative has 5 core attributes (`Weaponry`, `Coding`, `Charisma`, `Stealth`, `Cruelty`) rated `0–8`, and a `Fatigue` meter (`0–100`). Every `20` points of fatigue apply a `-1` penalty to all effective stats:
   $$\text{EffectiveStat} = \text{Clamp}\left(\text{BaseStat} - \left\lfloor\frac{\text{Fatigue}}{20}\right\rfloor, 0, 8\right)$$
2. **Preliminary Success Estimation (`MissionSO.cs`):**
   In the dispatch window, each operative's suitability is calculated against mission requirements (`delta = EffectiveStat - RequiredLevel`). Surplus stats grant `+10%` per point (capped at `+50%`), while deficits penalize chance by `-20%` per missing point, clamped between `5%` and `95%`.
3. **Shift Rotation & Stress Penalty (`RosterManager.cs`, `SquadController.cs`):**
   Operatives are divided into **Alpha** and **Beta** shifts. Resting during an inactive shift restores `30` fatigue points. Failing the tactical mini-game increases the mission's fatigue cost by `1.5x`.

</details>

---

## 🎮 О проекте

**The Ward** — прототип тактического симулятора диспетчера в духе *This Is the Police*. Игрок управляет двумя сменами оперативников, распределяет экипажи по вызовам на карте города в реальном времени, оценивает шансы на успех и разрешает кризисные ситуации через физическую мини-игру на базе процедурных радарных диаграмм. Также в проект интегрирован интерактивный справочный модуль по экстренной первой помощи (`EmergencyTopic`).

---

## 🛠 Архитектура и технические решения (C# / Unity)

Проект спроектирован с упором на чистоту кода, слабую связность компонентов (Low Coupling) и расширяемость для геймдизайнеров:

* **Событийная архитектура без поиска по сцене:**
  В игровом цикле полностью исключены дорогие вызовы `FindObjectOfType` и `GameObject.Find`. Взаимодействие между картой (`MapManager`), точками вызовов (`MissionNode`), движущимися экипажами (`SquadController`) и UI построено на делегатах `System.Action` и подписках на события (`RosterManager.OnRosterChanged`).
* **Процедурная генерация UI-мешей (`UI_RadarChart`):**
  Наследник `UnityEngine.UI.Graphic`, переопределяющий `OnPopulateMesh(VertexHelper vh)`. Строит пятиугольные радарные графики характеристик команды и требований миссии процедурно через вершинную геометрию, поддерживая плавную нелинейную анимацию раскрытия во время паузы (`Time.unscaledDeltaTime`).
* **Кастомная физика в полярных координатах (`MissionBall`):**
  Симуляция движения, трения и упругого отскока шарика внутри невыпуклого/выпуклого многоугольника UI. Коллизии со стенками графика рассчитываются на лету через перевод позиции в полярные координаты (`Mathf.Atan2`) и интерполяцию радиуса грани `UI_RadarChart.GetRadiusAtAngle(angle)`.
* **Data-Driven подход (`ScriptableObject`):**
  Все профили оперативников (`AgentProfileSO`) и сценарии вызовов (`MissionSO`) вынесены в конфигурационные ассеты. Добавление новых характеристик в `StatType` не требует переписывания логики модели `Agent`.
* **Инструментарий редактора (`DistrictRoute`):**
  Скрипт маршрутизации работает в режиме редактирования (`[ExecuteAlways]`), автоматически запекая дочерние точки маршрута в `LineRenderer` при изменении иерархии (`OnTransformChildrenChanged`, `OnValidate`) и отрисовывая отладочные `Gizmos`.

---

## 📐 Геймдизайн и математическая модель

В проекте реализован замкнутый кор-луп управления ресурсами (время, люди, выносливость):

| Механика | Формула / Правило | Геймплейная задача |
| :--- | :--- | :--- |
| **Деградация статов от усталости** | `EffectiveStat = Clamp(Base - (Fatigue / 20), 0, 8)` | Заставляет игрока ротировать состав, не позволяя проходить всю смену одной «элитной» группой. |
| **Рейтинг эффективности (EFF)** | Среднее арифметическое всех эффективных статов агента (`0.0 – 8.0`) | Быстрый визуальный индикатор боеспособности карточки в списке (`AgentUIEntry`). |
| **Прогноз шанса в окне отправки** | `1.0 + 0.1 * delta` (при `delta >= 0`, макс `+0.5`)<br>`1.0 - 0.2 * \|delta\|` (при недоборе стата) | Показывает предварительную оценку синергии отряда в диапазоне от `5%` до `95%`. |
| **Стресс при провале вызова** | `FatiguePenalty = FatigueCost * 1.5` (если шарик остановился вне зеленой зоны) | Наказывает за отправку неподходящего отряда повышенным износом оперативников. |
| **Смена караула (Alpha / Beta)** | Неактивная смена восстанавливает `-30` ед. усталости за пересменку | Создает стратегический выбор: дожимать текущую смену на штрафах или вызвать свежий резерв. |

---

## 📂 Структура скриптов (`Assets/Scripts`)

```text
Assets/Scripts/
├── Core & Data
│   ├── GameEnums.cs          # Перечисление базовых характеристик (StatType)
│   ├── Agent.cs              # Доменная модель оперативника, расчет усталости и статов
│   ├── AgentProfileSO.cs     # ScriptableObject-конфиг базовых параметров агента
│   ├── MissionSO.cs          # ScriptableObject-конфиг вызова и формула расчета шанса
│   └── RosterManager.cs      # Управление сменами Alpha/Beta и реактивные события ростера
├── Map & Navigation
│   ├── MapManager.cs         # Пул точек спавна и таймеры появления вызовов по сценарию
│   ├── MissionNode.cs        # Интерактивный маркер вызова на карте
│   ├── DistrictRoute.cs      # Editor-инструмент построения маршрутов для LineRenderer
│   └── SquadController.cs    # Движение экипажа по вейпоинтам и жизненный цикл миссии
└── UI & Mini-Game
    ├── UIManager.cs          # Отрисовка ростера и подписка на смену караула
    ├── AgentUIEntry.cs       # Drag-and-Drop карточка оперативника с адаптивной версткой
    ├── MissionSlot.cs        # Слот приема карточки в окне диспетчера (IDropHandler)
    ├── DispatchWindow.cs     # Окно формирования отряда и расчета предварительного шанса
    ├── MissionActiveUI.cs    # Экран разрешения кризиса и анимация графиков
    ├── UI_RadarChart.cs      # Процедурная отрисовка полигональной диаграммы на Canvas
    ├── MissionBall.cs        # Физика шарика в полярных координатах радар-чарта
    ├── MainMenuManager.cs    # Навигация главного меню и видеоплеера
    └── EmergencyTopic.cs     # Модуль интерактивных видеоинструкций первой помощи
```

---

## 🚀 Как запустить проект

1. Клонировать репозиторий:
   ```bash
   git clone [https://github.com/zibpwnzab/The-Ward.git](https://github.com/zibpwnzab/The-Ward.git)
   ```
2. Открыть проект через **Unity Hub** (версия **Unity 6 / 2022.3 LTS+**).
3. Открыть стартовую сцену в `Assets/Scenes/` и нажать **Play**.
