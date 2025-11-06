namespace scythe;

#pragma warning disable CS8981
public abstract class type(obj obj) {

    public obj obj = obj;
    
    public abstract void loop_3d(bool is_editor);
    public abstract void loop_ui(bool is_editor);
    public abstract void loop_editor(viewport viewport);
    public abstract void quit();
}