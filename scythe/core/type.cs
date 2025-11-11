namespace scythe;

public abstract class type(obj obj) {
    
    protected readonly obj obj = obj;

    public virtual string label_icon => icons.obj;
    public virtual color label_color => colors.gui_type_object;
    
    public abstract void loop_3d(bool is_editor);
    public abstract void loop_ui(bool is_editor);
    public abstract void loop_3d_editor(viewport viewport);
    public abstract void loop_ui_editor(viewport viewport);
    public abstract void quit();

}