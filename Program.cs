Console.OutputEncoding = System.Text.Encoding.Default;

#region Constants/Variables
const char WHITE_PAWN = '♙';
const char WHITE_ROOK = '♖';
const char WHITE_KNIGHT = '♘';
const char WHITE_BISHOP = '♗';
const char WHITE_QUEEN = '♕';
const char WHITE_KING = '♔';

const char BLACK_PAWN = '♟';
const char BLACK_ROOK = '♜';
const char BLACK_KNIGHT = '♞';
const char BLACK_BISHOP = '♝';
const char BLACK_QUEEN = '♛';
const char BLACK_KING = '♚';

char[] WHITE_PIECES = { WHITE_ROOK, WHITE_KNIGHT, WHITE_BISHOP, WHITE_QUEEN, WHITE_KING, WHITE_PAWN };
char[] BLACK_PIECES = { BLACK_ROOK, BLACK_KNIGHT, BLACK_BISHOP, BLACK_QUEEN, BLACK_KING, BLACK_PAWN };

var CASTLE = (KingSide: "O-O", QueenSide: "O-O-O");

char[][] ASCII_CHESS_BOARD = @" _____ _____ _____ _____ _____ _____ _____ _____
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|
|     |     |     |     |     |     |     |     |
|     |     |     |     |     |     |     |     |
|_____|_____|_____|_____|_____|_____|_____|_____|".Split('\n').Select(v => v.ToCharArray()).ToArray();
#endregion

#region Program
{
    var pieces = new Dictionary<char, List<(int, int)>>
    {
        { WHITE_PAWN, Enumerable.Range(0, 8).Select(v => (v, 1)).ToList() },
        { WHITE_ROOK, new List<(int, int)> { (0, 0), (7, 0) } },
        { WHITE_KNIGHT, new List<(int, int)> { (1, 0), (6, 0) } },
        { WHITE_BISHOP, new List<(int, int)> { (2, 0), (5, 0) } },
        { WHITE_QUEEN, new List<(int, int)> { (3, 0) } },
        { WHITE_KING, new List<(int, int)> { (4, 0) } },
        { BLACK_PAWN, Enumerable.Range(0, 8).Select(v => (v, 6)).ToList() },
        { BLACK_ROOK, new List<(int, int)> { (0, 7), (7, 7) } },
        { BLACK_KNIGHT, new List<(int, int)> { (1, 7), (6, 7) } },
        { BLACK_BISHOP, new List<(int, int)> { (2, 7), (5, 7) } },
        { BLACK_QUEEN, new List<(int, int)> { (3, 7) } },
        { BLACK_KING, new List<(int, int)> { (4, 7) } }
    };

    PrintBoardAndMove(pieces, "Initial board");

    for (int i = 0, currentMove = 0; i < args.Length; i++, currentMove++)
    {
        bool isWhite = currentMove % 2 == 0;
        (string output, Dictionary<char, List<((int, int), bool)>> piecesToChange, bool capture) = EvaluateMove(pieces, $"{(char.IsLower(args[i][0]) ? "P" : "")}{args[i]}", ref i, isWhite);

        bool movesArePossible = EvaluateNewPosition(piecesToChange, pieces);

        if (movesArePossible)
        {
            MovePieces(piecesToChange, ref pieces);
            PrintBoardAndMove(pieces, output);
        }
        else { throw new Exception($"Move at index {i} not possible: {args[i]}"); }
    }
}
#endregion

#region Primary Methods
(string, Dictionary<char, List<((int, int), bool)>>, bool) EvaluateMove(Dictionary<char, List<(int, int)>> pieces, string move, ref int i, bool isWhite)
{
    string output;

    var piecesToChange = new Dictionary<char, List<((int, int), bool)>>();
    bool capture = move.Contains('x');

    if (move == CASTLE.KingSide || move == CASTLE.QueenSide)
    {
        bool kingSideCastle = move == CASTLE.KingSide;
        output = $"{(kingSideCastle ? "King" : "Queen")}side castling";


        var y = isWhite ? 0 : 7;

        var king = isWhite ? WHITE_KING : BLACK_KING;
        var kingX = (New: kingSideCastle ? 6 : 2, Old: kingSideCastle ? 4 : 0);

        var rook = isWhite ? WHITE_ROOK : BLACK_ROOK;
        var rookX = (New: kingSideCastle ? 5 : 3, Old: kingSideCastle ? 7 : 0);


        var changesToKingsPos = new List<((int, int), bool)>
        {
            ((kingX.Old, y), false),
            ((kingX.New, y), true)
        };

        var changesToRooksPos = new List<((int, int), bool)>
        {
            ((rookX.Old, y), false),
            ((rookX.New, y), true)
        };

        piecesToChange.Add(king, changesToKingsPos);
        piecesToChange.Add(rook, changesToRooksPos);
    }
    else
    {
        bool enPassant = false;
        if (i != args.Length - 1)
        {
            if (args[i + 1] == "e.p.") { enPassant = true; i++; }
        }

        (char piece, string name) = GetPiece(move, isWhite);
        string newPosOutput = GetNewPos(move, out (int X, int Y) newPosition);
        string oldPosOutput = GetOldPos(move, piece, isWhite, capture, newPosition, pieces, out (int X, int Y) oldPosition);
        string captureOutput = EvaluateCapture(capture, enPassant);
        string action = EvaluateSpecialMoves(move, piece, isWhite, (oldPosition, newPosition), out Dictionary<char, List<((int, int), bool)>>? piecesToChangeBasedOnActions);

        output = $"{name} ({piece} ) {oldPosOutput}{captureOutput} {newPosOutput}{action}";

        piecesToChange = piecesToChangeBasedOnActions ?? new Dictionary<char, List<((int, int), bool)>>{
            {
                piece,
                new List<((int, int), bool)> { (oldPosition, false), (newPosition, true) }
            }
        };

        if (capture)
        {
            newPosition.Y += (enPassant ? (isWhite ? -1 : 1) : 0);
            var (vertical, horizontal) = PieceIndexToBoardIndex(newPosition.Y, newPosition.X);
            var takenPiece = ASCII_CHESS_BOARD[vertical][horizontal];

            piecesToChange.Add(
                takenPiece,
                new List<((int, int), bool)> { (newPosition, false) }
                );
        }
    }

    return (output, piecesToChange, capture);
}

bool EvaluateNewPosition(Dictionary<char, List<((int, int) Position, bool Add)>> newPiecesDict, Dictionary<char, List<(int, int)>> existingPiecesDict)
{
    List<(int, int)> existingPiecesToRemove = newPiecesDict.Values.SelectMany(v => v.Where(w => !w.Add).Select(x => x.Position)).ToList();
    List<(int, int)> newPiecesPositions = newPiecesDict.Values.SelectMany(v => v.Where(w => w.Add).Select(x => x.Position)).ToList();
    List<(int, int)> existingPiecesPositions = existingPiecesDict.Values.SelectMany(v => v).ToList();

    foreach (var position in newPiecesPositions)
    {
        if (existingPiecesPositions.Contains(position))
        {
            if (!existingPiecesToRemove.Contains(position)) { return false; }
        }
    }

    return true;
}

void MovePieces(Dictionary<char, List<((int, int), bool)>> piecesToChange, ref Dictionary<char, List<(int, int)>> pieces)
{
    foreach (var piece in piecesToChange)
    {
        foreach (var move in piece.Value)
        {
            var currentPiece = piece.Key;

            if (move.Item2) { pieces[currentPiece].Add(move.Item1); }
            else { pieces[currentPiece].Remove(move.Item1); }
        }
    }
}

void PrintBoardAndMove(Dictionary<char, List<(int X, int Y)>> pieces, string output)
{
    for (int i = 0; i < 8; i++)
    {
        for (int j = 0; j < 8; j++)
        {
            var (vertical, horizontal) = PieceIndexToBoardIndex(i, j);
            ASCII_CHESS_BOARD[vertical][horizontal] = ' ';
        }
    }

    for (int i = 0; i < pieces.Count; i++)
    {
        var keys = pieces.Keys;

        char currentPiece = keys.ElementAt(i);

        for (int j = 0; j < pieces[currentPiece].Count; j++)
        {
            var (vertical, horizontal) = PieceIndexToBoardIndex(pieces[currentPiece].ElementAt(j).Y, pieces[currentPiece].ElementAt(j).X);

            ASCII_CHESS_BOARD[vertical][horizontal] = currentPiece;
        }
    }

    Console.WriteLine(string.Join('\n', ASCII_CHESS_BOARD.Select(v => string.Join("", v))));
    Console.WriteLine(output);

    Console.ReadKey();
    Console.Clear();
}
#endregion

#region Secondary Methods
(char, string) GetPiece(string move, bool isWhite)
{
    (Index currentPiece, string name) = GetPieceIndexAndName(move[0]);

    return (isWhite ? WHITE_PIECES[currentPiece] : BLACK_PIECES[currentPiece], name);
}

string GetNewPos(string move, out (int, int) newPosition)
{
    int lastLowerCaseLetter = move.LastIndexOf(move.Reverse().Where(char.IsLower).First());
    string destination = move.Substring(lastLowerCaseLetter, 2);

    newPosition = ChessPositionToBoardPosition(destination);
    return destination;
}

string GetOldPos(string move, char piece, bool isWhite, bool capture, (int, int) newPosition, Dictionary<char, List<(int, int)>> pieces, out (int, int) oldPosition)
{
    string output = "";
    var piecesToRemove = GetPieceToRemove(piece, isWhite, capture, newPosition, pieces);

    if (move.TrimEnd('+', '#').Length >= 4 && move[1] != 'x')
    {
        string currentPosition = move.Substring(1, 2);

        if (char.IsDigit(currentPosition[1]))
        {
            oldPosition = ChessPositionToBoardPosition(currentPosition);
            output = $"on {currentPosition} ";
        }
        else
        {
            var position = currentPosition[0];
            bool rank = char.IsDigit(position);

            (int?, int?) previousPosition = (rank ? null : position - 'a', rank ? position - '1' : null);
            piecesToRemove = piecesToRemove.Where(v => v == (previousPosition.Item1 ?? v.Item1, previousPosition.Item2 ?? v.Item2)).ToArray();

            output = $"on the {position}-{(rank ? "rank" : "file")} ";
        }
    }

    if (piecesToRemove.Length != 1) { throw new Exception($"unable to determine the exact position of the previous piece: {piece}"); }
    else { oldPosition = piecesToRemove[0]; }

    return output;
}

string EvaluateCapture(bool capture, bool enPassant)
{
    if (capture)
    {
        return $"captures {(enPassant ? "a" : "the")} piece {(enPassant ? "en passant and goes to" : "on")}";
    }
    else { return "moves to"; }
}

string EvaluateSpecialMoves(string move, char piece, bool isWhite, ((int, int) OldPosition, (int, int) NewPosition) position, out Dictionary<char, List<((int, int), bool)>>? piecesToChange)
{
    piecesToChange = null;
    bool check = move[^1] == '+';
    bool checkMate = move[^1] == '#';
    Index promotionIndex = ^(1 + (check || checkMate ? 1 : 0));

    bool promotion = move[promotionIndex] is 'Q' or 'R' or 'B' or 'N';
    char promotionPieceLetter = move[promotionIndex];
    (char promotionPieceSymbol, string name) = GetPiece(promotionPieceLetter.ToString(), isWhite);

    string output = "";
    if (promotion)
    {
        output += $" and promotes to {name} ({promotionPieceLetter} )";

        piecesToChange = new Dictionary<char, List<((int, int), bool)>>
        {
            {
                piece,
                new List<((int, int), bool)> { (position.OldPosition, false) }
            },
            {
                promotionPieceSymbol,
                new List<((int, int), bool)> { (position.NewPosition, true) }
            }
        };
    }

    if (check) { output += $" and places opponents King ({(isWhite ? BLACK_KING : WHITE_KING)} ) in check"; }
    else if (checkMate) { output += ", checkmate"; }

    return output;
}
#endregion

#region Helper Methods
(int, int) ChessPositionToBoardPosition(string currentPos) => (currentPos[0] - 'a', int.Parse(currentPos[1].ToString()) - 1);

(Index, string) GetPieceIndexAndName(char letter)
{
    return letter switch
    {
        'R' => (0, "Rook"),
        'N' => (1, "Knight"),
        'B' => (2, "Bishop"),
        'Q' => (3, "Queen"),
        'K' => (4, "King"),
        _ => (5, "Pawn")
    };
}

(int, int)[] GetPieceToRemove(char piece, bool isWhite, bool capture, (int X, int Y) newPosition, Dictionary<char, List<(int, int)>> pieces)
{
    var possiblePositions = new List<(int, int)>();
    switch (piece)
    {
        case char queen when (queen is WHITE_QUEEN or BLACK_QUEEN):
            var queenPos1 = CalculatePossiblePositions(WHITE_ROOK, isWhite, capture, newPosition);
            var queenPos2 = CalculatePossiblePositions(WHITE_BISHOP, isWhite, capture, newPosition);

            possiblePositions = queenPos1.Concat(queenPos2).ToList();
            break;

        default:
            possiblePositions = CalculatePossiblePositions(piece, isWhite, capture, newPosition);
            break;
    }

    var matchingPositions = pieces[piece].Where(v => possiblePositions.Contains(v)).ToArray();

    return matchingPositions;
}

List<(int, int)> CalculatePossiblePositions(char piece, bool isWhite, bool capture, (int X, int Y) newPosition)
{
    var possiblePositions = new List<(int, int)>();

    switch (piece)
    {
        case char pawn when (pawn is WHITE_PAWN or BLACK_PAWN):
            for (int i = 1; i <= 2; i++)
            {
                possiblePositions.Add((
                    newPosition.X,
                    newPosition.Y + (isWhite ? -i : i)
                    ));
            }

            if (capture)
            {
                for (int i = -1; i <= 1; i += 2)
                {
                    possiblePositions.Add((
                        newPosition.X + i,
                        newPosition.Y + (isWhite ? -1 : 1)
                        ));
                }
            }
            break;

        case char rook when (rook is WHITE_ROOK or BLACK_ROOK):
            for (int i = 1; i < 8; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    var offset = i * j;

                    possiblePositions.Add((
                        newPosition.X + offset,
                        newPosition.Y
                        ));

                    possiblePositions.Add((
                        newPosition.X,
                        newPosition.Y + offset
                        ));
                }
            }
            break;

        case char knight when (knight is WHITE_KNIGHT or BLACK_KNIGHT):
            for (int i = -1; i <= 1; i += 2)
            {
                possiblePositions.Add((
                    newPosition.X + i,
                    newPosition.Y + 2 * i
                    ));

                possiblePositions.Add((
                    newPosition.X - i,
                    newPosition.Y + 2 * i
                    ));

                possiblePositions.Add((
                    newPosition.X + 2 * i,
                    newPosition.Y + i
                    ));

                possiblePositions.Add((
                    newPosition.X - 2 * i,
                    newPosition.Y + i
                    ));
            }
            break;

        case char bishop when (bishop is WHITE_BISHOP or BLACK_BISHOP):
            for (int i = 1; i < 8; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    var offset = i * j;

                    possiblePositions.Add((
                        newPosition.X + offset,
                        newPosition.Y + offset
                        ));

                    possiblePositions.Add((
                        newPosition.X - offset,
                        newPosition.Y + offset
                        ));
                }
            }
            break;

        case char king when (king is WHITE_KING or BLACK_KING):
            for (int j = -1; j <= 1; j += 2)
            {
                possiblePositions.Add((
                    newPosition.X + j,
                    newPosition.Y
                    ));

                possiblePositions.Add((
                    newPosition.X + j,
                    newPosition.Y + j
                    ));

                possiblePositions.Add((
                    newPosition.X,
                    newPosition.Y + j
                    ));

                possiblePositions.Add((
                    newPosition.X + j,
                    newPosition.Y - j
                    ));
            }
            break;

        default:
            throw new Exception($"Invalid piece: {piece}");
    }

    return possiblePositions;
}

(Index, Index) PieceIndexToBoardIndex(int vertical, int horizontal) => (^(vertical * 3 + 2), horizontal * 6 + 3);
#endregion
