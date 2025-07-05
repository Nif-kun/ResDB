using Godot;

namespace Addons.ResDB.UI;

[Tool]
public partial class DatabaseTab : PanelContainer {

    private DatabasePanel _databasePanel;
    private SelectionPanel _selectionPanel;
    
    public override void _Ready() {
        _databasePanel = GetNode<DatabasePanel>("Frame/Content/DatabasePanel");
        _selectionPanel = GetNode<SelectionPanel>("Frame/Content/SelectionPanel");
        
        _databasePanel.DatabasePressed += OnDatabasePressed;
        _databasePanel.DatabaseDeleted += OnDatabaseDeleted;
    }

    private void OnDatabasePressed(string name, string path) {
        _selectionPanel.Load(name, path);
    }

    private void OnDatabaseDeleted(string name) {
        _selectionPanel.Unload(name);
    }
    
}