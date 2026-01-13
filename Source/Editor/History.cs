using System.Reflection;

internal static class History {
    
    private static readonly List<Record> Records = [];

    private static int _currentIndex = -1;
    private static Record? _activeRecord;

    public static void StartRecording(object reference, string? description = null) {

        _activeRecord ??= new Record(description);

        if (_activeRecord.Objects.All(o => o.Reference != reference)) {
            
            _activeRecord.Objects.Add(new ObjectRecord(reference));
        }
    }

    public static void StopRecording() {
        
        if (_activeRecord == null) return;

        foreach (var record in _activeRecord.Objects)
            record.FinalState = GetState(record.Reference);
        
        if (_currentIndex < Records.Count - 1)
            Records.RemoveRange(_currentIndex + 1, Records.Count - (_currentIndex + 1));
        
        Records.Add(_activeRecord);

        _currentIndex = Records.Count - 1;
        _activeRecord = null;
    }
    
    private static object[] GetState(object reference) {

        var values = reference
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistory)))
            .Select(f => f.GetValue(reference)!)
            .ToArray();

        return values;
    }

    private static void ApplyState(object reference, object[] state) {

        var props = reference
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => Attribute.IsDefined(f, typeof(RecordHistory)))
            .ToArray();
        
        for (var i = 0; i < props.Length; i++)
            props[i].SetValue(reference, state[i]);
    }

    public static void Undo() {
        
        if (_currentIndex < 0) return;
        
        var currentRecord = Records[_currentIndex];
        
        if (currentRecord.Description != null)
            Notifications.Show("Undo " + currentRecord.Description);

        foreach (var record in currentRecord.Objects)
            ApplyState(record.Reference, record.StartState);

        _currentIndex--;
    }
    
    public static void Redo() {
        
        if (_currentIndex >= Records.Count - 1) return;

        _currentIndex++;
        
        var currentRecord = Records[_currentIndex];
        
        if (currentRecord.Description != null)
            Notifications.Show("Redo " + currentRecord.Description);

        foreach (var record in currentRecord.Objects)
            ApplyState(record.Reference, record.FinalState);
    }
    
    private class Record(string? description = null) {

        public readonly List<ObjectRecord> Objects = [];
        public readonly string? Description = description;
    }
    
    private class ObjectRecord(object reference) {
            
        public readonly object Reference = reference;
        public readonly object[] StartState = GetState(reference);
        public object[] FinalState = [];
    }
}