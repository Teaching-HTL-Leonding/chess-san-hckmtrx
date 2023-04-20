#region Program
{
    Console.OutputEncoding = System.Text.Encoding.Default;

    for (int i = 0; i < args.Length; i++)
    {
        string currentMove = args[i];
        currentMove = char.IsLower(currentMove[0]) ? $"P{currentMove}" : currentMove;

        if (currentMove is "0-0" or "0-0-0")
        {
            Console.Write(currentMove == "0-0" ? "King" : "Queen");
            Console.WriteLine(" side castling");
        }
        else
        {
            bool isWhite = i % 2 == 0;

            (char piece, string name) = GetPiece(currentMove, isWhite);
            string destination = GetDestinationOutput(currentMove);
            string action = GetActionOutput(currentMove, isWhite);
            bool capture = currentMove.Contains('x');

            bool enPassant = false;
            if (i != args.Length - 1)
            {
                if (args[i + 1] == "e.p.") { enPassant = true; i++; }
            }

            Console.WriteLine($"{name} ({piece} ) {GetPositionOutput(currentMove)}{GetCaptureOutput(capture, enPassant)} {destination}{action}");
        }

    }
}
#endregion

#region Methods
(Index, string) GetPieceIndexFromLetter(char letter)
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

(char, string) GetPiece(string move, bool isWhite)
{
    char[] blackPieces = { '♜', '♞', '♝', '♛', '♚', '♟' };
    char[] whitePieces = { '♖', '♘', '♗', '♕', '♔', '♙' };

    (Index currentPiece, string name) = GetPieceIndexFromLetter(move[0]);

    return (
            isWhite ? whitePieces[currentPiece] : blackPieces[currentPiece],
            name
            );
}

string GetDestinationOutput(string move)
{
    int lowerCaseLetter = move.LastIndexOf(move.Reverse().Where(char.IsLower).First());
    return move.Substring(lowerCaseLetter, 2);
}

string GetPositionOutput(string move)
{
    if (move.TrimEnd('+', '#').Length < 4 || move[1] == 'x') { return ""; }

    bool isQueen = move[0] == 'Q';
    string currentPos = move.Substring(1, isQueen ? 2 : 1);

    string output;
    if (currentPos.Length > 1 && char.IsDigit(currentPos[1])) { output = $"on {currentPos} "; }
    else
    {
        output = $"on the {currentPos[0]}-{(char.IsDigit(currentPos[0]) ? "rank" : "file")} ";
    }

    return output;
}

string GetCaptureOutput(bool capture, bool enPassant)
{
    if (capture)
    {
        return $"captures {(enPassant ? "a" : "the")} piece {(enPassant ? "en passant and goes to" : "on")}";
    }
    else { return "moves to"; }
}

string GetActionOutput(string move, bool isWhite)
{
    bool check = move[^1] == '+';
    bool checkMate = move[^1] == '#';
    Index promotionIndex = ^(1 + (check || checkMate ? 1 : 0));

    bool promotion = move[promotionIndex] is 'Q' or 'R' or 'B' or 'N';
    char promotionPiece = move[promotionIndex];
    (char piece, string name) = GetPiece(promotionPiece.ToString(), isWhite);

    string output = promotion ? $" and is promoted to {name} ({piece} )" : "";

    if (check) { output += " and places opponents King (♔ ) in check"; }
    else if (checkMate) { output += ", checkmate"; }

    return output;
}
#endregion
