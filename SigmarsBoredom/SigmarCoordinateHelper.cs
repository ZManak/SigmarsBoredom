using System;
using System.Drawing;
using System.Linq;

namespace SigmarsBoredom
{
    /// <summary>
    /// Provides constants and methods that help with the coordinates of the items to identify in Opus Magnum.
    /// </summary>
    public static class SigmarCoordinateHelper
    {
        /// <summary>
        /// Baseline game window width used to calibrate absolute pixel coordinates.
        /// </summary>
        public const int ReferenceWidth = 1920;

        /// <summary>
        /// Baseline game window height used to calibrate absolute pixel coordinates.
        /// </summary>
        public const int ReferenceHeight = 1080;

        private static double _scaleX = 1d;
        private static double _scaleY = 1d;

        /// <summary>
        /// Current horizontal scale factor relative to the 1920x1080 reference.
        /// </summary>
        public static double ScaleX => _scaleX;

        /// <summary>
        /// Current vertical scale factor relative to the 1920x1080 reference.
        /// </summary>
        public static double ScaleY => _scaleY;

        /// <summary>
        /// X coordinate of the board in the Opus Magnum main window.
        /// </summary>
        public static int BoardStartX => ScaleXCoordinate(861);

        /// <summary>
        /// Y coordinate of the board in the Opus Magnum main window.
        /// </summary>
        public static int BoardStartY => ScaleYCoordinate(195);

        /// <summary>
        /// Width of the board in pixels.
        /// </summary>
        public static int BoardWidth => ScaleXCoordinate(712);

        /// <summary>
        /// Height of the board in pixels.
        /// </summary>
        public static int BoardHeight => ScaleYCoordinate(622);

        /// <summary>
        /// Rectangle of the board in the Opus Magnum main window.
        /// </summary>
        public static Rectangle BoardRectangle => new Rectangle(BoardStartX, BoardStartY, BoardWidth, BoardHeight);

        /// <summary>
        /// Size (in both width and height) of a marble in a tile.
        /// </summary>
        public static int MarbleSize => ScaleXCoordinate(52);

        /// <summary>
        /// Number of tiles in both width and height on the board.
        /// </summary>
        public const int BoardSize = 11;

        /// <summary>
        /// Offset in X coordinates between each potential marble.
        /// </summary>
        public static int MarbleOffsetX => ScaleXCoordinate(66);

        /// <summary>
        /// Offset in Y coordinates between each potential marble.
        /// </summary>
        public static int MarbleOffsetY => ScaleYCoordinate(57);

        /// <summary>
        /// Coordinates of tile spots on the board that are not tiles because they're outside of the board.
        /// </summary>
        public static readonly Point[] DeadTileSpots = {
            // Top-left corner
            new Point(00, 00), new Point(01, 00), new Point(02, 00), new Point(00, 01), new Point(01, 01), new Point(00, 02), new Point(01, 02), new Point(00, 03), new Point(00, 04),
            // Bottom-left corner
            new Point(00, 06), new Point(00, 07), new Point(00, 08), new Point(01, 08), new Point(00, 09), new Point(01, 09), new Point(00, 10), new Point(01, 10), new Point(02, 10),
            // Top-right corner
            new Point(09, 00), new Point(10, 00), new Point(09, 01), new Point(10, 01), new Point(10, 02), new Point(10, 03),
            // Bottom-right corner
            new Point(10, 07), new Point(10, 08), new Point(09, 09), new Point(10, 09), new Point(09, 10), new Point(10, 10)
        };

        /// <summary>
        /// Coordinates of buttons that highlight marbles of the same type.
        /// </summary>
        public static Point[] MarbleHintCoordinates =>
            new[]
            {
                new Point(ScaleXCoordinate(970), ScaleYCoordinate(884)), // Salt
                new Point(ScaleXCoordinate(1023), ScaleYCoordinate(884)), // Air
                new Point(ScaleXCoordinate(1065), ScaleYCoordinate(884)), // Fire
                new Point(ScaleXCoordinate(1107), ScaleYCoordinate(884)), // Water
                new Point(ScaleXCoordinate(1149), ScaleYCoordinate(884)), // Earth
                new Point(ScaleXCoordinate(1209), ScaleYCoordinate(884)), // Quicksilver
                new Point(ScaleXCoordinate(1264), ScaleYCoordinate(884)), // Lead
                new Point(ScaleXCoordinate(1304), ScaleYCoordinate(884)), // Tin
                new Point(ScaleXCoordinate(1344), ScaleYCoordinate(884)), // Iron
                new Point(ScaleXCoordinate(1384), ScaleYCoordinate(884)), // Copper
                new Point(ScaleXCoordinate(1424), ScaleYCoordinate(884)), // Silver
                new Point(ScaleXCoordinate(1464), ScaleYCoordinate(884)) // Gold
            };

        /// <summary>
        /// Coordinates of the "New game" button.
        /// </summary>
        public static Point NewGameButtonPosition => new Point(ScaleXCoordinate(870), ScaleYCoordinate(885));

        /// <summary>
        /// Initializes scaling based on the Opus Magnum main window size.
        /// </summary>
        /// <param name="windowSize">Current game window size in pixels.</param>
        public static void Initialize(Size windowSize)
        {
            if (windowSize.Width <= 0 || windowSize.Height <= 0)
                throw new ArgumentException("Window size must be strictly positive.", nameof(windowSize));

            _scaleX = windowSize.Width / (double)ReferenceWidth;
            _scaleY = windowSize.Height / (double)ReferenceHeight;
        }

        /// <summary>
        /// Gets the coordinates in the Opus Magnum window of the tile at the specified board coordinates.
        /// </summary>
        /// <param name="boardCoordinates">Board coordinates of the target tile.</param>
        public static Point GetWindowCoordinateOfTileCenterOnBoard(Point boardCoordinates)
        {
            var boardImagePos = GetImageCoordinateOfTileOnBoard(boardCoordinates.X, boardCoordinates.Y);
            return new Point(boardImagePos.X + BoardStartX + MarbleSize / 2, boardImagePos.Y + BoardStartY + MarbleSize / 2);
        }

        /// <summary>
        /// Gets the coordinates on the board image of the tile at the specified board coordinates.
        /// </summary>
        /// <param name="boardX">X coordinate of the target tile on the board.</param>
        /// <param name="boardY">Y coordinate of the target tile on the board.</param>
        public static Point GetImageCoordinateOfTileOnBoard(int boardX, int boardY)
        {
            int offsetX = (boardY % 2 - 1) * MarbleOffsetX / 2;
            return new Point(MarbleOffsetX * boardX + offsetX, boardY * MarbleOffsetY);
        }

        /// <summary>
        /// Gets the rectangle in image coordinates that contains the marble (or center of the empty tile) at the given board coordinates.
        /// </summary>
        /// <param name="boardX">X coordinate of the target tile on the board.</param>
        /// <param name="boardY">Y coordinate of the target tile on the board.</param>
        public static Rectangle GetMarbleRectangle(int boardX, int boardY)
        {
            return new Rectangle(GetImageCoordinateOfTileOnBoard(boardX, boardY), new Size(MarbleSize, MarbleSize));
        }

        /// <summary>
        /// Scales an horizontal pixel offset relative to a marble.
        /// </summary>
        /// <param name="value">Reference horizontal offset at 1920x1080.</param>
        public static int ScaleOffsetX(int value)
        {
            return ScaleXCoordinate(value);
        }

        /// <summary>
        /// Scales a vertical pixel offset relative to a marble.
        /// </summary>
        /// <param name="value">Reference vertical offset at 1920x1080.</param>
        public static int ScaleOffsetY(int value)
        {
            return ScaleYCoordinate(value);
        }

        /// <summary>
        /// Determines whether the given coordinates match a valid tile on the board.
        /// </summary>
        /// <param name="boardX">Target X coordinate on the board.</param>
        /// <param name="boardY">Target Y coordinate on the board.</param>
        public static bool IsValidBoardCoordinate(int boardX, int boardY)
        {
            return boardX >= 0 && boardY >= 0 && boardX < BoardSize && boardY < BoardSize && !DeadTileSpots.Contains(new Point(boardX, boardY));
        }

        private static int ScaleXCoordinate(int baseCoordinate)
        {
            return (int)Math.Round(baseCoordinate * _scaleX);
        }

        private static int ScaleYCoordinate(int baseCoordinate)
        {
            return (int)Math.Round(baseCoordinate * _scaleY);
        }
    }
}
