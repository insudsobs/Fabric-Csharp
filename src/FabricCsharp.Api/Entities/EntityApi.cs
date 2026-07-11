namespace FabricCsharp.Api.Entities;

/// <summary>
/// Entity type registration. Maps to net.minecraft.entity.EntityType.
/// </summary>
public abstract class EntityType<T> where T : Entity
{
    /// <summary>
    /// Builder for creating entity types.
    /// </summary>
    public class Builder
    {
        /// <summary>Create a builder for a living entity.</summary>
        public static Builder Create(Func<EntityType<T>, World, T> factory, SpawnGroup spawnGroup)
        {
            throw new NotSupportedException(
                "EntityType.Builder.Create is a transpiler intrinsic and should not be called at runtime.");
        }

        /// <summary>Set entity dimensions (width, height).</summary>
        public Builder Dimensions(float width, float height)
        {
            throw new NotSupportedException(
                "EntityType.Builder.Dimensions is a transpiler intrinsic and should not be called at runtime.");
        }

        /// <summary>Set max tracking range in blocks.</summary>
        public Builder MaxTrackingRange(int range)
        {
            throw new NotSupportedException(
                "EntityType.Builder.MaxTrackingRange is a transpiler intrinsic and should not be called at runtime.");
        }

        /// <summary>Set tracking tick interval.</summary>
        public Builder TrackingTickInterval(int interval)
        {
            throw new NotSupportedException(
                "EntityType.Builder.TrackingTickInterval is a transpiler intrinsic and should not be called at runtime.");
        }

        /// <summary>Make the entity always update its velocity.</summary>
        public Builder AlwaysUpdateVelocity(bool value)
        {
            throw new NotSupportedException(
                "EntityType.Builder.AlwaysUpdateVelocity is a transpiler intrinsic and should not be called at runtime.");
        }

        /// <summary>Build the entity type.</summary>
        public EntityType<T> Build()
        {
            throw new NotSupportedException(
                "EntityType.Builder.Build is a transpiler intrinsic and should not be called at runtime.");
        }
    }
}

/// <summary>
/// Spawn group categories for entities.
/// Maps to: net.minecraft.entity.SpawnGroup
/// </summary>
public enum SpawnGroup
{
    Monster,
    Creature,
    Ambient,
    Axolotls,
    UndergroundWaterCreature,
    WaterCreature,
    WaterAmbient,
    Misc
}

/// <summary>
/// Stub for entity renderer registration.
/// Maps to: net.fabricmc.fabric.api.client.rendering.v1.EntityRendererRegistry
/// </summary>
public static class EntityRendererRegistry
{
    /// <summary>Register a renderer for an entity type.</summary>
    public static void Register<T>(EntityType<T> type, Func<EntityRendererFactory.Context, EntityRenderer<T>> factory)
        where T : Entity
    {
        throw new NotSupportedException(
            "EntityRendererRegistry.Register is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Stub for entity renderer.
/// </summary>
public abstract class EntityRenderer<T> where T : Entity { }

/// <summary>
/// Stub for entity renderer factory context.
/// </summary>
public abstract class EntityRendererFactory
{
    /// <summary>
    /// Rendering context passed to entity renderer factories.
    /// </summary>
    public abstract class Context { }
}

/// <summary>
/// Stub for default attribute registry.
/// Maps to: net.fabricmc.fabric.api.object.builder.v1.entity.FabricDefaultAttributeRegistry
/// </summary>
public static class DefaultAttributeRegistry
{
    /// <summary>Register default attributes for an entity type.</summary>
    public static void Register(EntityType<Entity> type, AttributeContainer attributes)
    {
        throw new NotSupportedException(
            "DefaultAttributeRegistry.Register is a transpiler intrinsic and should not be called at runtime.");
    }
}

/// <summary>
/// Stub for entity attribute container (builder pattern).
/// Maps to: net.minecraft.entity.attribute.DefaultAttributeContainer
/// </summary>
public abstract class AttributeContainer { }
