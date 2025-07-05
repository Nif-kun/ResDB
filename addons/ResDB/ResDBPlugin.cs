#if TOOLS
using Godot;

namespace Addons.ResDB;

[Tool]
public partial class ResDBPlugin : EditorPlugin {
    
    private PanelContainer _databaseTab;
    
    public override void _EnterTree() {
        var scene = GD.Load<PackedScene>("res://addons/ResDB/ui/DatabaseTab.tscn");
        _databaseTab = scene.Instantiate<PanelContainer>();
        _databaseTab.Hide();
        
        EditorInterface.Singleton.GetEditorMainScreen().AddChild(_databaseTab);
    }

    public override void _ExitTree() {
        if (_databaseTab != null && _databaseTab.IsInsideTree())
            _databaseTab.QueueFree();
    }

    public override bool _HasMainScreen() => true;

    public override void _MakeVisible(bool visible) {
        if (_databaseTab != null) _databaseTab.Visible = visible;
    }

    public override string _GetPluginName() => "Database";

    public override Texture2D _GetPluginIcon() => GD.Load<Texture2D>("res://addons/ResDB/ResDBIcon.svg");
    
}

#endif
