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
        private const int BaseBoardStartX = 861;
        private const int BaseBoardStartY = 195;
        private const int BaseBoardWidth = 712;
        private const int BaseBoardHeight = 622;
        private const int BaseMarbleSize = 52;
        private const int BaseMarbleOffsetX = 66;
        private const int BaseMarbleOffsetY = 57;
        private const int BaseNewGameButtonX = 870;
        private const int BaseNewGameButtonY = 885;
        private const int BaseHintButtonY = 884;
        private static readonly int[] BaseHintButtonXs = { 970, 1023, 1065, 1107, 1149, 1209, 1264, 1304, 1344, 1384, 1424, 1464 };

        private static double _scaleX = 1d;
        private static double _scaleY = 1d;
        private static int _boardStartX;
        private static int _boardStartY;
        private static int _boardWidth;
        private static int _boardHeight;
        private static int _marbleSize;
        private static int _marbleOffsetX;
        private static int _marbleOffsetY;
        private static Rectangle _boardRectangle;
        private static Point[] _marbleHintCoordinates;
        private static Point _newGameButtonPosition;

        static SigmarCoordinateHelper()
        {
            RecomputeScaledCoordinates();
        }

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
        public static int BoardStartX => _boardStartX;

        /// <summary>
        /// Y coordinate of the board in the Opus Magnum main window.
        /// </summary>
        public static int BoardStartY => _boardStartY;

        /// <summary>
        /// Width of the board in pixels.
        /// </summary>
        public static int BoardWidth => _boardWidth;

        /// <summary>
        /// Height of the board in pixels.
        /// </summary>
        public static int BoardHeight => _boardHeight;

        /// <summary>
        /// Rectangle of the board in the Opus Magnum main window.
        /// </summary>
        public static Rectangle BoardRectangle => _boardRectangle;

        /// <summary>
        /// Size (in both width and height) of a marble in a tile.
        /// </summary>
        public static int MarbleSize => _marbleSize;

        /// <summary>
        /// Number of tiles in both width and height on the board.
        /// </summary>
        public const int BoardSize = 11;

        /// <summary>
        /// Offset in X coordinates between each potential marble.
        /// </summary>
        public static int MarbleOffsetX => _marbleOffsetX;

        /// <summary>
        /// Offset in Y coordinates between each potential marble.
        /// </summary>
        public static int MarbleOffsetY => _marbleOffsetY;

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
        public static Point[] MarbleHintCoordinates => _marbleHintCoordinates;

        /// <summary>
        /// Coordinates of the "New game" button.
        /// </summary>
        public static Point NewGameButtonPosition => _newGameButtonPosition;

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
            RecomputeScaledCoordinates();
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
        /// Scales a horizontal pixel offset relative to a marble.
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

        private static void RecomputeScaledCoordinates()
        {
            _boardStartX = ScaleXCoordinate(BaseBoardStartX);
            _boardStartY = ScaleYCoordinate(BaseBoardStartY);
            _boardWidth = ScaleXCoordinate(BaseBoardWidth);
            _boardHeight = ScaleYCoordinate(BaseBoardHeight);
            _marbleSize = ScaleXCoordinate(BaseMarbleSize);
            _marbleOffsetX = ScaleXCoordinate(BaseMarbleOffsetX);
            _marbleOffsetY = ScaleYCoordinate(BaseMarbleOffsetY);

            _boardRectangle = new Rectangle(_boardStartX, _boardStartY, _boardWidth, _boardHeight);

            _marbleHintCoordinates = BaseHintButtonXs
                .Select(x => new Point(ScaleXCoordinate(x), ScaleYCoordinate(BaseHintButtonY)))
                .ToArray();

            _newGameButtonPosition = new Point(ScaleXCoordinate(BaseNewGameButtonX), ScaleYCoordinate(BaseNewGameButtonY));
        }
    }
}
