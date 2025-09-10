using System.Collections.Concurrent;
using System.Text.Json;

namespace Clean.Architecture.Persistence.Common;

/// <summary>
/// Base class for repositories that store data in binary files
/// </summary>
public abstract class BinaryFileRepository<TKey, TEntity> where TKey : notnull where TEntity : class
{
    private readonly string _filePath;
    protected readonly ConcurrentDictionary<TKey, TEntity> _entities;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    protected BinaryFileRepository(string fileName)
    {
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        Directory.CreateDirectory(dataDirectory);
        // Ensure .dat extension for binary files
        var fileNameWithExtension = fileName.EndsWith(".dat") ? fileName : Path.ChangeExtension(fileName, ".dat");
        _filePath = Path.Combine(dataDirectory, fileNameWithExtension);
        _entities = new ConcurrentDictionary<TKey, TEntity>();
        LoadFromFile();
    }

    protected abstract TKey GetEntityKey(TEntity entity);

    protected virtual void LoadFromFile()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var binaryData = File.ReadAllBytes(_filePath);
            if (binaryData.Length == 0)
                return;

            var jsonData = System.Text.Encoding.UTF8.GetString(binaryData);
            if (string.IsNullOrEmpty(jsonData))
                return;

            var entities = JsonSerializer.Deserialize<Dictionary<string, TEntity>>(jsonData, GetJsonOptions());
            if (entities != null)
            {
                foreach (var kvp in entities)
                {
                    if (TryParseKey(kvp.Key, out var key))
                    {
                        _entities.TryAdd(key, kvp.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error in production - for now just continue with empty collection
            Console.WriteLine($"Error loading data from {_filePath}: {ex.Message}");
        }
    }

    protected virtual async Task SaveToFileAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var dataToSave = _entities.ToDictionary(
                kvp => kvp.Key.ToString()!,
                kvp => kvp.Value
            );

            var jsonData = JsonSerializer.Serialize(dataToSave, GetJsonOptions());
            var binaryData = System.Text.Encoding.UTF8.GetBytes(jsonData);
            await File.WriteAllBytesAsync(_filePath, binaryData);
        }
        catch (Exception ex)
        {
            // Log error in production
            Console.WriteLine($"Error saving data to {_filePath}: {ex.Message}");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected virtual JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            IncludeFields = true
        };
    }

    protected abstract bool TryParseKey(string keyString, out TKey key);

    protected virtual async Task AddEntityAsync(TEntity entity)
    {
        var key = GetEntityKey(entity);
        _entities.TryAdd(key, entity);
        await SaveToFileAsync();
    }

    protected virtual async Task UpdateEntityAsync(TEntity entity)
    {
        var key = GetEntityKey(entity);
        _entities.AddOrUpdate(key, entity, (_, _) => entity);
        await SaveToFileAsync();
    }

    protected virtual async Task RemoveEntityAsync(TKey key)
    {
        _entities.TryRemove(key, out _);
        await SaveToFileAsync();
    }

    protected virtual TEntity? GetEntity(TKey key)
    {
        _entities.TryGetValue(key, out var entity);
        return entity;
    }

    protected virtual IEnumerable<TEntity> GetAllEntities()
    {
        return _entities.Values;
    }
}
