﻿using BlockEngine.Client.Framework.ECS.Entities;

namespace BlockEngine.Client.Framework.ECS.Components;

public abstract class Component
{
    public bool IsEnabled { get; private set; }
    
    protected Entity Entity { get; private set; } = null!;
    
    
    public void SetEntity(Entity entity)
    {
        Entity = entity;
    }


    public void Enable()
    {
        IsEnabled = true;
        OnEnable();
    }
    
    
    public void Update(double time)
    {
        OnUpdate(time);
    }
    
    
    public void Render(double time)
    {
        OnRender(time);
    }
    
    
    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }
    
    
    public void Destroy()
    {
        OnDestroy();
    }


    protected virtual void OnEnable() { }
    
    protected virtual void OnUpdate(double time) { }
    
    protected virtual void OnRender(double time) { }
    
    protected virtual void OnDisable() { }
    
    protected virtual void OnDestroy() { }
}