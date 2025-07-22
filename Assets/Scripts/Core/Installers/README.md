# Zenject Services Distribution

## Project Context (Глобальные сервисы)
*Существуют на протяжении всей игры, переживают смену сцен*

### Core Services
- **ISaveService** - Система сохранений (PlayerPrefs + JSON)
- **ISceneManagerService** - Управление сценами и переходами
- **IGameFactory** - Фабрика создания объектов
- **IUIPageService** - Управление UI страницами (глобально)
- **IPoolService** - Пул объектов (глобальный)
- **IConfigService** - Сервис конфигураций (глобальные настройки)

### Data Services (Будущие)
- **IDataService** - Загрузка и управление игровыми данными
- **ILocalizationService** - Система локализации
- **IProgressService** - Глобальный прогресс игрока

---

## Scene Context (Сервисы уровня сцены)
*Создаются заново для каждой сцены*

### Gameplay Services
- **IBattleService** - Управление боевой системой
- **IWaveService** - Управление волнами врагов
- **ITowerService** - Управление башнями на поле
- **ISpellService** - Система магии и заклинаний
- **IHeroService** - Управление героинями и отрядом
- **IEnemyService** - Управление врагами

### Map & Level Services  
- **IBattlefieldService** - Тактическое поле боя
- **ILevelService** - Управление текущим уровнем
- **ILocationService** - Управление локациями

### Character & Progression Services
- **ICharacterService** - Характеристики героинь
- **IInventoryService** - Инвентарь и экипировка
- **IUpgradeService** - Система улучшений
- **ISkillTreeService** - Дерево навыков

### Audio & Visual Services
- **IAudioService** - Звуковые эффекты и музыка
- **IVFXService** - Визуальные эффекты
- **ICameraService** - Управление камерой

---

## Current Installers Structure

### ProjectContextInstaller (Текущие)
- ✅ ISaveService ➤ SaveService
- ✅ ISceneManagerService ➤ SceneManagerService  
- ✅ IGameFactory ➤ GameFactory
- ✅ IUIPageService ➤ UIPageService (глобальный UI)
- ✅ IPoolService ➤ PoolService (глобальный пул)
- ✅ IConfigService ➤ ConfigService (конфигурации)

### GameSceneInstaller (Текущие)
- 🔮 Базовые игровые сервисы для конкретной сцены (пока пустой)

### Неиспользуемые инсталлеры (в Unused/)
- **MenuSceneInstaller** - для сцен меню
- **GameplayServicesInstaller** - для расширенных игровых сервисов
- **AudioVisualInstaller** - для аудио и визуальных сервисов

---

## Distribution Summary

### Project Context (Выживают между сценами):
- ✅ SaveService - глобальные сохранения
- ✅ SceneManagerService - переходы между сценами
- ✅ GameFactory - создание объектов
- ✅ UIPageService - глобальные UI страницы
- ✅ PoolService - глобальный пул объектов
- ✅ ConfigService - глобальные конфигурации
- 🔮 DataService, LocalizationService

### Scene Context (Пересоздаются для каждой сцены):
- 🔮 BattleService, WaveService, HeroService
- 🔮 AudioService, VFXService, CameraService
- 🔮 Локальные сервисы для конкретных сцен

---

## Usage Examples

```csharp
// Глобальные сервисы (Project Context)
[Inject] private ISaveService saveService;
[Inject] private ISceneManagerService sceneManager;
[Inject] private IUIPageService uiPageService;
[Inject] private IPoolService poolService;
[Inject] private IConfigService configService;

// Сервисы уровня сцены (Scene Context)  
[Inject] private IBattleService battleService;
```

## Installation Order
1. **ProjectContextInstaller** (глобальные сервисы)
2. **GameSceneInstaller** (локальные сервисы)
3. *По необходимости:* инсталлеры из Unused/

## Benefits of This Architecture

### UIPageService в Project Context:
- ✅ Переходы UI между сценами
- ✅ Глобальные попапы и диалоги
- ✅ Persistent UI элементы
- ✅ Единое управление всем UI

### PoolService в Project Context:
- ✅ Глобальные пулы для всех сцен
- ✅ Переиспользование объектов между уровнями
- ✅ Оптимизация памяти в масштабе всей игры
- ✅ Постоянная доступность пула
- ✅ Не требует пересоздания при смене сцен
- ✅ Эффективное управление снарядами, эффектами, UI

### ConfigService в Project Context:
- ✅ Глобальные настройки игры
- ✅ Легкое управление конфигурациями
- ✅ Автоматическая загрузка всех конфигов
- ✅ Типобезопасное получение конфигов
- ✅ Поддержка Resources/Configs папки
- ✅ Можность перезагрузки конфигов

## ConfigService Usage Examples

```csharp
// Инжектируем ConfigService
[Inject] private IConfigService configService;

// Получаем конфиг по типу
var gameConfig = configService.GetConfig<GameConfig>();

// Получаем конфиг по имени
var audioConfig = configService.GetConfig<AudioConfig>("AudioConfig");

// Проверяем наличие конфига
if (configService.HasConfig<BalanceConfig>())
{
    var balance = configService.GetConfig<BalanceConfig>();
    float damage = balance.baseTowerDamage;
}

// Перезагружаем конфиг
configService.ReloadConfig<UIConfig>();
```
