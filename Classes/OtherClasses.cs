using System.Globalization;
using MoonSharp.Interpreter;

namespace RoHelper;

[MoonSharpUserData]
public class UDim{
    private double _Scale;
    private int _Offset;
    [MoonSharpHidden]
    public EventHandler? changedEvent;
    public double Scale{
        get=>_Scale;
        set{ _Scale = value;
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    }
    public int Offset{
        get=>_Offset;
        set{ _Offset = value;
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    }
    [MoonSharpHidden]
    public void CloneFrom(UDim uDim){
        _Scale = uDim.Scale;
        _Offset = uDim.Offset;
        changedEvent?.Invoke(this,EventArgs.Empty);
    }
    private void UDimInit(double scale=0, int offset=0){
        _Scale = scale; _Offset = offset;
    }
    public UDim(){UDimInit();} 
    public UDim(double scale){UDimInit(scale);} 
    public UDim(double scale, int offset){UDimInit(scale,offset);} 
    public UDim(double scale, double offset){UDimInit(scale,(int)(offset));}
} [MoonSharpUserData] 
public class UDim_Creator{
    private Script script;
    public DynValue _new(double scale =0,int offset =0){
        return DynValue.FromObject(script,new UDim(scale,offset));
    }
    public DynValue fromScale(double scale=0){
        return DynValue.FromObject(script,new UDim(scale));
    } public DynValue fromOffset(int offset=0){
        return DynValue.FromObject(script,new UDim(0,offset));
    }
    public UDim_Creator(Script script){
        this.script = script;
    }
}
[MoonSharpUserData]
public class UDim2{
    private UDim _X =new();
    private UDim _Y =new();
    public UDim X{
        get=>_X;
        set{ _X.CloneFrom(value);
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    } public UDim Y{
        get=>_Y;
        set{ _Y.CloneFrom(value);
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    } [MoonSharpHidden]
    public EventHandler? changedEvent;
    public UDim2(UDim? x, UDim? y){
        if (x!=null) _X.CloneFrom(x);
        if (y!=null) _Y.CloneFrom(y);
        _X.changedEvent += (_,_)=>changedEvent?.Invoke(this,EventArgs.Empty);
        _Y.changedEvent += (_,_)=>changedEvent?.Invoke(this,EventArgs.Empty);
    } [MoonSharpHidden]
    static public UDim2 Clone(UDim2 uDim2){return new(uDim2.X,uDim2.Y);}
} [MoonSharpUserData]
public class UDim2_Creator{
    private Script script;
    public DynValue _new(UDim? x, UDim? y){
        return DynValue.FromObject(script,new UDim2(x,y));
    }
    public DynValue fromScale(double xScale=0,double yScale =0){
        UDim x = new(xScale,0);
        UDim y = new(yScale,0);
        return DynValue.FromObject(script,new UDim2(x,y));
    } public DynValue fromOffset(int xOffset=0,int yOffset =0){
        UDim x = new(0,xOffset);
        UDim y = new(0,yOffset);
        return DynValue.FromObject(script,new UDim2(x,y));
    }
    public UDim2_Creator(Script script){
        this.script = script;
    }
}

[MoonSharpUserData]
public class PixelsSize{
    private int _Width;
    private int _Height;
    [MoonSharpHidden]
    public EventHandler? changedEvent;
    public int Width{
        get=>_Width;
        set{ _Width = value;
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    } public int Height{
        get=>_Height;
        set{ _Height = value;
            changedEvent?.Invoke(this,EventArgs.Empty);
        }
    }
    [MoonSharpHidden]
    public void RawSet(int width=0,int height=0){
        _Width = width;
        _Height = height;
    } [MoonSharpHidden]
    public void CloneFrom(PixelsSize size){
        _Width = size.Width;
        _Height = size.Height;
        changedEvent?.Invoke(this,EventArgs.Empty);
    }
    public PixelsSize(int width=0,int height=0){
        RawSet(width,height);
    }
} [MoonSharpUserData]
class PixelsSize_Creator{
    private Script script;
    public DynValue _new(int width =0, int height =0){
        return DynValue.FromObject(script,new PixelsSize(width,height));
    }
    private static Rectangle ScreenBounds = (Screen.PrimaryScreen?? Screen.AllScreens.First()).Bounds;
    public DynValue fromScreenScale(double widthScale=0, double heightScale =0){
        int width = (int)(ScreenBounds.Width*widthScale);
        int height = (int)(ScreenBounds.Height*heightScale);
        return DynValue.FromObject(script,new PixelsSize(width,height));
    }
    public PixelsSize_Creator(Script script){
        this.script = script;
    }
}

[MoonSharpUserData]
public class CSharpLuaError{
    public string description;
    public CSharpLuaError(string errorDescription =""){
        description = errorDescription;
    }
}