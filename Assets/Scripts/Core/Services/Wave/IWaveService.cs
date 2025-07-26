using Game.Wave;

namespace Game.Services
{
    /// <summary>
    /// Интерфейс сервиса управления волнами врагов.
    /// </summary>
    public interface IWaveService
    {
        /// <summary>
        /// Запустить волны врагов.
        /// </summary>
        void StartWaves();
        
        /// <summary>
        /// Остановить волны врагов.
        /// </summary>
        void StopWaves();
        
        /// <summary>
        /// Запустить следующую волну.
        /// </summary>
        void SpawnNextWave();
        
        /// <summary>
        /// Получить номер текущей волны (1-based).
        /// </summary>
        int GetCurrentWaveNumber();
        
        /// <summary>
        /// Проверить активна ли волна.
        /// </summary>
        bool IsWaveActive();
        
        /// <summary>
        /// Получить текущую конфигурацию волны.
        /// </summary>
        WaveConfig GetCurrentWaveConfig();
        
        /// <summary>
        /// Получить общее количество волн на уровне.
        /// </summary>
        int GetTotalWaveCount();
        
        /// <summary>
        /// Проверить можно ли запустить следующую волну досрочно.
        /// </summary>
        bool CanTriggerNextWaveEarly();
    }
}
