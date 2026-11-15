namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The role an NPC plays in the character's life.
    /// </summary>
    public enum RelationshipType
    {
        /// <summary>Mother or father.</summary>
        Parent = 0,

        /// <summary>Brother or sister.</summary>
        Sibling = 1,

        /// <summary>Son or daughter.</summary>
        Child = 2,

        /// <summary>Boyfriend or girlfriend.</summary>
        Partner = 3,

        /// <summary>Husband or wife.</summary>
        Spouse = 4,

        /// <summary>Former partner or spouse.</summary>
        Ex = 5,

        /// <summary>Friend or acquaintance.</summary>
        Friend = 6,

        /// <summary>Someone from work.</summary>
        Colleague = 7,

        /// <summary>Business network contact.</summary>
        Contact = 8,
    }
}
