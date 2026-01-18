using System.Reflection;

internal static class History {
    
    private static readonly List<Record> Records = [];

    private static int _currentIndex = -1;
    public static Record? ActiveRecord;
    
    public static bool CanUndo => _currentIndex >= 0;
    public static bool CanRedo => _currentIndex < Records.Count - 1;
    
    public static void StartRecording(object reference, string? description = null) {

        ActiveRecord ??= new Record(description);

        if (ActiveRecord.Objects.All(o => o.Reference != reference)) {
            
            ActiveRecord.Objects.Add(new ObjectRecord(reference));
        }
    }

    public static void StopRecording() {
        
        if (ActiveRecord == null) return;

        foreach (var record in ActiveRecord.Objects)
            record.FinalState = GetState(record.Reference);
        
        if (_currentIndex < Records.Count - 1)
            Records.RemoveRange(_currentIndex + 1, Records.Count - (_currentIndex + 1));
        
        Records.Add(ActiveRecord);

        _currentIndex = Records.Count - 1;
        ActiveRecord = null;
    }
    
    private static object[] GetState(object reference) {

        var values = reference
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .Select(f => f.GetValue(reference)!)
            .ToArray();

        return values;
    }

    private static void ApplyState(object reference, object[] state) {

        var props = reference
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistoryAttribute)))
            .ToArray();
        
        for (var i = 0; i < props.Length; i++)
            props[i].SetValue(reference, state[i]);
    }

    public static void Undo() {
        
        if (!CanUndo) return;
        
        var currentRecord = Records[_currentIndex];
        
        if (currentRecord.Description != null)
            Notifications.Show("Undo " + currentRecord.Description);
        
        currentRecord.UndoAction?.Invoke();        

        foreach (var record in currentRecord.Objects)
            ApplyState(record.Reference, record.StartState);

        _currentIndex--;
    }
    
    
    public static void Redo() {
        
        if (!CanRedo) return;
        
        _currentIndex++;
        var currentRecord = Records[_currentIndex];
        
        if (currentRecord.Description != null)
            Notifications.Show("Redo " + currentRecord.Description);

        currentRecord.RedoAction?.Invoke();
        
        foreach (var record in currentRecord.Objects)
            ApplyState(record.Reference, record.FinalState);
    }

    public class Record(string? description = null) {

        public readonly List<ObjectRecord> Objects = [];
        public readonly string? Description = description;
        
        public Action? UndoAction, RedoAction;
    }

    public class ObjectRecord(object reference) {
            
        public readonly object Reference = reference;
        public readonly object[] StartState = GetState(reference);
        public object[] FinalState = [];
    }
}