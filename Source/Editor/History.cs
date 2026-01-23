using System.Collections;
using System.Reflection;

internal class HistoryStack {
    
    private readonly List<Record> _records = [];
    private int _currentIndex = -1;
    private Record? _activeRecord;

    public bool CanUndo => _currentIndex >= 0;
    public bool CanRedo => _currentIndex < _records.Count - 1;
    public bool CanExtend => _currentIndex == _records.Count - 1;

    public void StartRecording(object reference, string? description = null) {
        
        _activeRecord ??= new Record(description);

        if (_activeRecord.Objects.All(o => o.Reference != reference))
            _activeRecord.Objects.Add(new ObjectRecord(reference));
    }

    public void StopRecording() {
        
        if (_activeRecord == null) return;

        foreach (var record in _activeRecord.Objects)
            record.FinalState = History.GetState(record.Reference);

        if (_currentIndex < _records.Count - 1)
            _records.RemoveRange(_currentIndex + 1, _records.Count - (_currentIndex + 1));

        _records.Add(_activeRecord);
        _currentIndex = _records.Count - 1;
        _activeRecord = null;
    }

    public void Undo() {
        
        if (!CanUndo) return;
        
        var currentRecord = _records[_currentIndex];
        
        if (currentRecord.Description != null) Notifications.Show("Undo " + currentRecord.Description);
        
        currentRecord.UndoAction?.Invoke();

        foreach (var record in currentRecord.Objects)
            History.ApplyState(record.Reference, record.StartState);

        _currentIndex--;
    }

    public void Redo() {
        
        if (!CanRedo) return;
        
        _currentIndex++;
        
        var currentRecord = _records[_currentIndex];
        
        if (currentRecord.Description != null) Notifications.Show("Redo " + currentRecord.Description);
        
        currentRecord.RedoAction?.Invoke();

        foreach (var record in currentRecord.Objects)
            History.ApplyState(record.Reference, record.FinalState);
    }
    
    public bool UpdateLastRecord(object reference, string description) {
        
        if (_activeRecord != null) return false; 
        if (_records.Count == 0 || _currentIndex != _records.Count - 1) return false; 

        var last = _records[_currentIndex];
        if (last.Description != description) return false;

        var objRec = last.Objects.FirstOrDefault(o => o.Reference == reference);
        if (objRec == null) return false;

        objRec.FinalState = History.GetState(reference);
        return true;
    }

    public void SetUndoAction(Action action) { if (_activeRecord != null) _activeRecord.UndoAction = action; }
    public void SetRedoAction(Action action) { if (_activeRecord != null) _activeRecord.RedoAction = action; }

    private class Record(string? description = null) {
        
        public readonly List<ObjectRecord> Objects = [];
        public readonly string? Description = description;
        public Action? UndoAction, RedoAction;
    }

    internal class ObjectRecord(object reference) {
        
        public readonly object Reference = reference;
        public readonly object?[] StartState = History.GetState(reference);
        public object?[] FinalState = [];
    }
}

internal static class History {
    
    private static readonly HistoryStack Global = new();

    public static bool CanUndo => Global.CanUndo;
    public static bool CanRedo => Global.CanRedo;
    
    public static void StartRecording(object reference, string? description = null) => Global.StartRecording(reference, description);
    public static void StopRecording() => Global.StopRecording();
    public static void Undo() => Global.Undo();
    public static void Redo() => Global.Redo();
    public static void SetUndoAction(Action action) => Global.SetUndoAction(action);
    public static void SetRedoAction(Action action) => Global.SetRedoAction(action);

    // Helpers exposed for HistoryStack
    public static object?[] GetState(object reference) {
        
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        var props = reference.GetType().GetProperties(flags)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .OrderBy(p => p.Name)
            .Select(f => CloneValue(f.GetValue(reference)));
            
        var fields = reference.GetType().GetFields(flags)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .OrderBy(f => f.Name)
            .Select(f => CloneValue(f.GetValue(reference)));
            
        return props.Concat(fields).ToArray();
    }

    private static object? CloneValue(object? value) {
        
        switch (value) {
            
            case null:
                return null;
            
            case ICloneable cloneable:
                return cloneable.Clone();
            
            case IList list: {
                
                // Handle generic lists (specifically List<T>)
                var type = value.GetType();
                
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                
                    // Create new instance of List<T>
                    var newList = (IList?)Activator.CreateInstance(type);
                    if (newList == null) return value; // Fallback
                
                    foreach (var item in list) {
                        
                        // Deep clone items if possible, otherwise copy reference
                        var clonedItem = CloneValue(item);
                        newList.Add(clonedItem);
                    }
                    
                    return newList;
                }

                break;
            }
        }

        return value;
    }

    public static void ApplyState(object reference, object?[] state) {
        
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        var props = reference.GetType().GetProperties(flags)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .OrderBy(p => p.Name)
            .ToArray();
            
        var fields = reference.GetType().GetFields(flags)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .OrderBy(f => f.Name)
            .ToArray();
            
        var i = 0;
        
        foreach (var prop in props) prop.SetValue(reference, state[i++]);
        foreach (var field in fields) field.SetValue(reference, state[i++]);

        if (reference is Component comp) comp.UnloadAndQuit();
    }
}


