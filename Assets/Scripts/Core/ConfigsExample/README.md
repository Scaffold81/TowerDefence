# Config Service Examples

В этой папке находятся примеры ScriptableObject конфигураций для использования с ConfigService.

## Использование:

1. Создайте свои ScriptableObject классы наследуясь от ScriptableObject
2. Разместите созданные .asset файлы в папке Resources/Configs/
3. ConfigService автоматически загрузит все конфиги при инициализации

## Пример использования:

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
}
```

## Создание своих конфигов:

```csharp
[CreateAssetMenu(fileName = "MyConfig", menuName = "Game/Configs/My Config")]
public class MyConfig : ScriptableObject
{
    [Header("Settings")]
    public float someValue = 1.0f;
    public string someName = "Default";
}
```

Затем создайте .asset файл через меню Create > Game > Configs > My Config и поместите в Resources/Configs/
