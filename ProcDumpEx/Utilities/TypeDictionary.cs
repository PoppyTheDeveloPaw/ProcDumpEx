using System.Collections;

namespace ProcDumpEx.Utilities;

internal class TypeDictionary<T>
{
	private Dictionary<string, Type> _internalDictionary = new Dictionary<string, Type>();

	internal void AddType(string key, Type value)
	{
		if (typeof(T).IsAssignableFrom(value))
		{
			_internalDictionary[key] = value;
		}
		else
		{
			throw new ArgumentException($"Type '{value.FullName}' does not implement {typeof(T).FullName}.", nameof(value));
		}
	}

	internal Type GetType(string key)
	{
		if (_internalDictionary.ContainsKey(key))
		{
			return _internalDictionary[key];
		}
		else
		{
			throw new KeyNotFoundException($"Key '{key}' not found in the dictionary.");
		}
	}

	internal bool IsEmpty() => !_internalDictionary.Any();

	internal bool ContainsKey(string key) => _internalDictionary.ContainsKey(key);

	internal T? CreateObject(string key) => (T?)Activator.CreateInstance(GetType(key));

	internal T? CreateObject(string key, string[] paramArray) => (T?)Activator.CreateInstance(GetType(key), paramArray);

	internal IEnumerable<string> GetAllCommandNames() => _internalDictionary.Keys;
}
