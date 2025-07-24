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
        /// Получить номер текущей волны.
        /// </summary>
        int GetCurrentWaveNumber();
        
        /// <summary>
        /// Проверить активна ли волна.
        /// </summary>
        bool IsWaveActive();
    }
}
