namespace Reactics.Battle
{
    public interface IMapRenderer
    {
        /// <summary>
        /// The target <c>Map</c> to render.
        /// </summary>
        /// <value>The Map used for rendering</value>
        IMap Map { get; }

        /// <summary>
        /// The width of the Map Renderer in tiles.
        /// </summary>
        /// <value>The Width property is equivalent to a call on <c>Map.width</c></value>
        ushort Width { get; }
        /// <summary>
        /// The length of the Map Renderer in tiles.
        /// </summary>
        /// <value>The Length property is equivalent to a call on <c>Map.length</c></value>
        ushort Length { get; }
        /// <summary>
        /// Signals the renderer to highlight a set of tiles on the grid. Multiple calls to this method with the same <c>HighlightType</c> will clear any tiles that are already highlighted for that type.
        /// </summary>
        void Highlight(HighlightType highlightType, params Point[] points);
        /// <summary>
        /// Checks to see if the provided point is within the map bounds.
        /// </summary>
        /// <returns>True if the map is within bounds, false otherwise.</returns>
        bool Contains(Point point);


    }
    public enum HighlightType
    {
        HOVER = 1,
        PLAYER_MOVEMENT = 2,
        PLAYER_ATTACK = 3,
        PLAYER_SUPPORT = 4,
        ENEMY_MOVEMENT = 5,
        ENEMY_ATTACK = 6,
        ENEMY_SUPPORT = 7,
        ALLY_MOVEMENT = 8,
        ALLY_ATTACK = 9,
        ALLY_SUPPORT = 10,
        UTILITY = 11
    }

}