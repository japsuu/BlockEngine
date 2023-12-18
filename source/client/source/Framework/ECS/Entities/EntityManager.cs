﻿namespace BlockEngine.Client.Framework.ECS.Entities;

public class EntityManager
{
    private readonly List<Entity> _entities = new();


    public EntityManager()
    {
        Entity.EnableEvent += OnEntityEnabledChanged;
    }


    public void Update()
    {
        foreach (Entity obj in _entities)
        {
            obj.Update();
        }
    }


    public void Draw()
    {
        foreach (Entity obj in _entities)
        {
            obj.Draw();
        }
    }


    private void OnEntityEnabledChanged(Entity obj, bool enabled)
    {
        if (enabled)
        {
            RegisterEntity(obj);
        }
        else
        {
            UnregisterEntity(obj);
        }
    }


    private void RegisterEntity(Entity obj)
    {
        _entities.Add(obj);
    }


    private void UnregisterEntity(Entity obj)
    {
        _entities.Remove(obj);
    }
}