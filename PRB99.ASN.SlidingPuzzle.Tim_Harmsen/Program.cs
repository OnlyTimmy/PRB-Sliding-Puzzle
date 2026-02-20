using System.Reflection;

namespace PRB99.ASN.SlidingPuzzle.Tim_Harmsen;

class Program
{
    static void Main(string[] args)
    {
        //Tim Harmsen 27 Oktober 2025 
        //End of project: 5 November 2025
        while (true)
        {
            Console.Clear(); // Cleares the console at start.
            if (MainMenu() == "E") break; // If the user enters E, the program stops.
            string chosenMap = SelectMapMenu(); // Shows the menu with all maps and puts the selected map in variable chosenMap
            char[,] field = ReadMap(chosenMap); // Loads the map to be read into a char array.
            field = ConvertField(field); //Converts the field into a usable field (- = ice, R = rock, P = player)
            if (!CheckMap(field)) continue; // If the map is not valid (thats why the !), return to the main menu.
            field = ChoseStartingPosition(field);            
            GameLoop(field);
        }
    }
    #region MTR – Constant

    // MTR – Constant
    const int height = 4;
    const int width = 6;
    const char rock = '\x25B2'; // https://www.alt-codes.net/triangle-symbols
    const char player = 'P';
    const char ice = ' ';
    const char brokenIce = 'X';
    const char hole = 'O';
    const bool soundEnabled = true; // Set to false to disable sound

    #endregion
    static string[] ReadMaps()
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "Maps");

        if (!Directory.Exists(basePath))
        {
            Console.WriteLine($"Maps folder not found: {basePath}");
            Console.ReadLine();
            return Array.Empty<string>();
        }

        string[] mapPaths = Directory.GetFiles(basePath, "*.map");

        if (mapPaths.Length == 0)
        {
            Console.WriteLine($"No .map files found in: {basePath}");
            Console.ReadLine();
            return Array.Empty<string>();
        }

        string[] maps = new string[mapPaths.Length];

        for (int i = 0; i < mapPaths.Length; i++)
        {
            string name = Path.GetFileNameWithoutExtension(mapPaths[i]);
            Console.WriteLine($"Map {name} loaded successfully...");
            maps[i] = name;
        }

        Console.WriteLine("Maps loaded successfully...");
        Console.ReadLine();
        Console.Clear();
        return maps;
    }
    static char[,] ReadMap(string file)
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "Maps");
        string filePath = Path.Combine(basePath, file + ".map");

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Map file not found: {filePath}");
            Console.ReadLine();
            return new char[0, 0];
        }

        string[] lines = File.ReadAllLines(filePath);

        char[,] board = new char[lines.Length, lines[0].Length];

        for (int i = 0; i < lines.Length; i++)
        for (int j = 0; j < lines[i].Length; j++)
            board[i, j] = lines[i][j];

        return board;
    }
    static string MainMenu()
    { // loads the game title on screen and displays what to do to play; returns the input. Only S and E are valid.
        PrintStartMenu();
        return GetValidInput("S - Start spel\nE - Einde spel", ["S", "E"]); // MTR – Escape characters
    }
    static string SelectMapMenu()
    {
        Console.Clear();
        string[] maps = ReadMaps(); // creates an array with all possible maps.
        Array.Sort(maps); // For me, it didn’t want to go in order from easy to hard, now it just happens that this order is also alphabetical.
        string prompt = "";
        string[] valid = new string[maps.Length]; //makes 'valid' equal to the number of maps (if 10 maps, numbers 1–10 are valid).
        // MTR – For lus
        for (int i = 0; i < maps.Length; i++) 
        {//for each map name, adds a prompt with a number in front (+1 because array index starts at 0, players at 1).
            prompt += $"{i + 1} - {maps[i]}\n"; // MTR – Escape characters //MTR –  Interpolated string
            valid[i] = Convert.ToString(i + 1);
        }

        string choice = GetValidInput(prompt, valid);
        return maps[Convert.ToInt32(choice) - 1];
    }
    static void PrintStartMenu()
    { //displays the game name on screen
        Console.ForegroundColor =
            ConsoleColor
                .Cyan; //Bron: https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        //Bron: https://patorjk.com/software/taag/#p=testall&f=Slant&t=Sliding+Puzzle&x=none&v=4&h=4&w=80&we=false
        Console.WriteLine( // MTR – Escape characters
            " ▗▄▄▖▗▖   ▗▄▄▄▖▗▄▄▄ ▗▄▄▄▖▗▖  ▗▖ ▗▄▄▖    ▗▄▄▖ ▗▖ ▗▖▗▄▄▄▄▖▗▄▄▄▄▖▗▖   ▗▄▄▄▖\n▐▌   ▐▌     █  ▐▌  █  █  ▐▛▚▖▐▌▐▌       ▐▌ ▐▌▐▌ ▐▌   ▗▞▘   ▗▞▘▐▌   ▐▌   \n ▝▀▚▖▐▌     █  ▐▌  █  █  ▐▌ ▝▜▌▐▌▝▜▌    ▐▛▀▘ ▐▌ ▐▌ ▗▞▘   ▗▞▘  ▐▌   ▐▛▀▀▘\n▗▄▄▞▘▐▙▄▄▖▗▄█▄▖▐▙▄▄▀▗▄█▄▖▐▌  ▐▌▝▚▄▞▘    ▐▌   ▝▚▄▞▘▐▙▄▄▄▖▐▙▄▄▄▖▐▙▄▄▖▐▙▄▄▖\n");
        Console.ResetColor();
    }
    static string GetValidInput(string prompt, string[] validInputs)
    { // Function usable anywhere to define both the prompt and valid inputs.
        Console.WriteLine(prompt);
        string userInput;
        do // MTR – While Lus
        { // keeps asking for input until a valid one is entered, and displays a message each time.
            userInput = Console.ReadLine().ToUpper();
            if (validInputs.Contains(userInput)) break; // if the input matches a valid one, break and return.
            Console.WriteLine("Foutieve menukeuze!");
        } while (true);

        return userInput;
    }
    
    static char[,] CreateField()
    { // temporary starting field.
        char[,] field = new char[height, width];
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                field[i, j] = ice;
            }
        }

        field[0, 0] = rock;
        field[0, 5] = rock;
        field[1, 0] = rock;
        field[1, 2] = rock;
        field[2, 4] = rock;

        field[0, 4] = player;
        return field;
    }
    
    static void DrawBoard(char[,] field, int heightY)
    {
        if (soundEnabled) Console.Write("\a"); //MTR - Escape character (sound on each move)
        Console.Clear();
        int height = field.GetLength(0);
        int width = field.GetLength(1);
        
        Console.Write("   "); // zorgt voor uitlijning van de nummers
        Console.WriteLine(Numbering(field)); // zet de nummers er op van de kolommen.
        
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            Console.Write("   ");  // zorgt voor uitlijning van de vakjes 
            for (int k = 0; k < width; k++)
            {
                Console.Write("+---");
            }

            Console.WriteLine("+");
            Console.Write(i+1+" "); // schrijft de nummer van de rij op links van het veld
            if (i<9) Console.Write(" ");
            for (int j = 0; j < width; j++)
            {
                Console.Write("| ");
                Console.Write(field[i, j]);
                Console.Write(" ");
                
            }

            Console.WriteLine("|");
            
        }


        Console.Write("   "); // zorgt voor uitlijning van de onderste rij
        for (int k = 0; k < width; k++)
        {
            Console.Write("+---");
        }

        Console.WriteLine("+");
    } 
    // MTR – Overloading
    static void DrawBoard(char[,] field, bool useColor, int heightY)//board with colors to make elements visible 
    { 
        if (soundEnabled) Console.Write("\a"); //MTR - Escape character (sound on each move)
        Console.Clear();
        int height = field.GetLength(0);
        int width = field.GetLength(1); 
        
        Console.Write("   "); // zorgt voor uitlijning van de nummers
        Console.WriteLine(Numbering(field)); // zet de nummers er op van de kolommen.
        
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            Console.Write("   ");  // zorgt voor uitlijning van de vakjes  
            for (int k = 0; k < width; k++)
            {
                Console.Write("+---");
            }
            Console.WriteLine("+");
            // if (height >= 10) Console.Write(" ");
            // {
            //     
            // }
            Console.Write(i+1+" "); // schrijft de nummer van de rij op links van het veld
            if (i<9) Console.Write(" ");
            for (int j = 0; j < width; j++)
            {
                Console.Write("| ");
                
                if (field[i, j] == rock) Console.ForegroundColor = ConsoleColor.Red; 
                else if (field[i, j] == player) Console.ForegroundColor = ConsoleColor.Magenta;
                else if (field[i, j] == brokenIce) Console.ForegroundColor = ConsoleColor.Cyan;
                else Console.ResetColor();
                
                Console.Write(field[i, j]);
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.WriteLine("|");
        }
        Console.Write("   "); // zorgt voor uitlijning van de onderste rij
        for (int k = 0; k < width; k++)
        {
            Console.Write("+---");
        }

        Console.WriteLine("+");
    }

    static void DrawBoard(char[,] field)
    {
        if (soundEnabled) Console.Write("\a"); //MTR - Escape character (sound on each move)
        Console.Clear();
        int height = field.GetLength(0);
        int width = field.GetLength(1);
        
        Console.Write("  "); // zorgt voor uitlijning van de nummers
        Console.WriteLine(Numbering(field)); // zet de nummers er op van de kolommen.
        
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            Console.Write("  ");  // zorgt voor uitlijning van de vakjes  
            for (int k = 0; k < width; k++)
            {
                Console.Write("+---");
            }

            Console.WriteLine("+");
            Console.Write(i+1+" "); // schrijft de nummer van de rij op links van het veld
            for (int j = 0; j < width; j++)
            {
                Console.Write("| ");
                Console.Write(field[i, j]);
                Console.Write(" ");
                
            }

            Console.WriteLine("|");
            
        }


        Console.Write("  "); // zorgt voor uitlijning van de onderste rij

        for (int k = 0; k < width; k++)
        {
            Console.Write("+---");
        }

        Console.WriteLine("+");
    }
    // MTR – Overloading
    static void DrawBoard(char[,] field, bool useColor)//board with colors to make elements visible 
    { 
        if (soundEnabled) Console.Write("\a"); //MTR - Escape character (sound on each move)
        Console.Clear();
        int height = field.GetLength(0);
        int width = field.GetLength(1); 
        
        Console.Write("  "); // zorgt voor uitlijning van de nummers
        Console.WriteLine(Numbering(field)); // zet de nummers er op van de kolommen.
        
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            Console.Write("  ");  // zorgt voor uitlijning van de vakjes  
            for (int k = 0; k < width; k++)
            {
                Console.Write("+---");
            }
            Console.WriteLine("+");
            // if (height >= 10) Console.Write(" ");
            // {
            //     
            // }
            Console.Write(i+1+" "); // schrijft de nummer van de rij op links van het veld
            for (int j = 0; j < width; j++)
            {
                Console.Write("| ");
                
                if (field[i, j] == rock) Console.ForegroundColor = ConsoleColor.Red; 
                else if (field[i, j] == player) Console.ForegroundColor = ConsoleColor.Magenta;
                else if (field[i, j] == brokenIce) Console.ForegroundColor = ConsoleColor.Cyan;
                else Console.ResetColor();
                
                Console.Write(field[i, j]);
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.WriteLine("|");
        }
        Console.Write("  "); // zorgt voor uitlijning van de onderste rij
        for (int k = 0; k < width; k++)
        {
            Console.Write("+---");
        }

        Console.WriteLine("+");
    }
    
    static string Numbering(char[,] field)
    {
        int width = field.GetLength(1);
        string numbering = "";
        for (int i = 0; i < width; i++)
        {
            if (i > 8) numbering += (" " + (i + 1))+" ";
            else numbering += ("  "+ (i+1) + " ");
        }

        return numbering;
    }
    static string GameLoop(char[,] field)
    {
        while (true)
        {
            int height = field.GetLength(0);
            if (height > 8) DrawBoard(field,true, 10);
            else DrawBoard(field,true); // puts the board on the screen
            int[] move = GetMoveDirection(); // stores the move direction in a variable to use in movePlayer
            string result = MovePlayer(field, move);
            if (result == "won" || result == "lost")
            return result;
        }
    }

    static char[,] ChoseStartingPosition(char[,] newField)
    {

        int height = newField.GetLength(0);
        int width =  newField.GetLength(1);
        if (height > 8) DrawBoard(newField,true, 10);
        else DrawBoard(newField,true); // puts the board on the screen
        string[] rowValidInput = new string[height];
        string[] columValidInput =  new string[width];
        for (int i = 0; i < (width); i++)
        {
            columValidInput[i] = Convert.ToString(i+1);
        }
        for (int i = 0; i < height; i++)
        {
            rowValidInput[i] = Convert.ToString(i+1);
        }

        int columChoice;
        int rowChoice;
        while (true)
        {
            columChoice = Convert.ToInt32(GetValidInput("Kies de kolom waar u wilt beginnen", columValidInput));
            rowChoice = Convert.ToInt32(GetValidInput("Kies de rij waar u wilt beginnen", rowValidInput));
            if (newField[rowChoice - 1, columChoice - 1] == ice || newField[rowChoice - 1, columChoice - 1] == player) break;
            Console.WriteLine("Deze positie is niet mogelijk omdat er een rots staat.\nKies een andere startpositie!\n");
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (newField[i, j] == player)
                {
                    newField[i, j] = ice;
                }
            }
        }
        newField[rowChoice-1, columChoice-1] = player;
        
        return newField;

    }
    static int[] FindPlayer(char[,] field)
    {
        // MTR – For lus
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                if (field[i, j] == player) return [i, j];
            }
        }

        return [-1, -1];
    }
    
    static char[,] ConvertField(char[,] rawField)
    { // Checks every tile of the map and converts it (R = rock, P = player, - = ice, O = brokenIce)
        int height = rawField.GetLength(0);
        int width = rawField.GetLength(1);
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (rawField[i, j] == 'R')
                {
                    rawField[i, j] = rock;
                }

                if (rawField[i, j] == 'P')
                {
                    rawField[i, j] = player;
                }

                if (rawField[i, j] == '-')
                {
                    rawField[i, j] = ice;
                }

                if (rawField[i, j] == 'O')
                {
                    rawField[i, j] = brokenIce;
                }
            }
        }

        return rawField;
    }

    static int[] GetMoveDirection()
    {
        // converts input L R B O into a direction vector array
        int vectorX = 0;
        int vectorY = 0;

        string input = GetValidInput("L - Links\nR - Rechts\nB - Boven\nO - Onder", ["L", "R", "B", "O"]);

        // MTR – Switch
        switch (input)
        {
            case "L":
                vectorX = -1;
                break;
            case "R":
                vectorX = 1;
                break;
            case "B":
                vectorY = -1;
                break;
            case "O":
                vectorY = 1;
                break;
        }

        return [vectorY, vectorX]; // arrays always use [y , x]
    }

    static string MovePlayer(char[,] field, int[] move)
    {
        int vectorY = move[0];
        int vectorX = move[1];
        int[] playerPosition = FindPlayer(field);
        int positionY = playerPosition[0];
        int positionX = playerPosition[1];
        int newPositionX = positionX + vectorX;
        int newPositionY = positionY + vectorY;
        // MTR - while lus
        while (true)
        {   //while loop allows sliding; it continues until a result is returned ("won", "lost" or "continue"). "continue" means you can move again.
            if (BoundaryOrRockCheck(field, newPositionX, newPositionY)) return "continue"; 
            if (field[newPositionY, newPositionX] == brokenIce) 
            { // if you move over a tile that was already broken, gameEnd is called with "lost" and the correct positions are updated.
                field[newPositionY, newPositionX] = hole;
                field[positionY, positionX] = brokenIce;
                GameEnd(field, "lost");
                return "lost";
            }
            //If you don’t lose and can move, the previous tile becomes broken and the new tile becomes the player
                field[positionY, positionX] = brokenIce;
                field[newPositionY, newPositionX] = player;
            

            if (WinCondition(field))
            { // if all tiles have been visited, you win.
                GameEnd(field, "won");
                return "won";
            }
            //update old position to new position and calculate next step for sliding.
            positionX = newPositionX;
            positionY = newPositionY;
            newPositionX += vectorX;
            newPositionY += vectorY;
        }
    }
    static bool WinCondition(char[,] field)
    { // checks the entire field; if there is still an ice tile, return false (not won yet). If none remain, return true.
        int height = field.GetLength(0);
        int width = field.GetLength(1);
        for (int i = 0; i < height; i++)
        for (int j = 0; j < width; j++)
            if (field[i, j] == ice)
                return false;
        return true;
    }
    static bool BoundaryOrRockCheck(char[,] field, int newX, int newY)
    { // checks if the new position is outside the field (edge) or is a rock (adjacent to a rock)
        int height = field.GetLength(0);
        int width = field.GetLength(1);
        bool check = (newX < 0 || newY < 0 || newY >= height || newX >= width || field[newY, newX] == rock);
        return check;
    }
    static void GameEnd(char[,] field, string result)
    { //called once you win or lose (the game ends)
        int height = field.GetLength(0);
        // displays a message showing if you won or lost and turns the board red or green
        Console.ForegroundColor = result == "won" ? ConsoleColor.Green : ConsoleColor.Red; //https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        if (height > 8) DrawBoard(field, 10);
        else DrawBoard(field);
        Console.WriteLine(result == "won" ? "Gewonnen !!!\nDruk op enter om naar het hoofdmenu te gaan" : "Verloren !!!\nDruk op enter om naar het hoofdmenu te gaan");
        Console.ResetColor();
        Console.ReadLine();
    }
    static bool CheckMap(char[,] field)
    { // checks if the map is valid according to all criteria; if everything is correct, the game can start; otherwise, an error message appears.
        bool isValid = CheckMapSize(field) && CheckCharacters(field) && CheckPlayerCount(field) &&
                       CheckIceField(field) && CheckForIsolatedIce(field);
        return isValid;
    }
    static bool CheckMapSize(char[,] field)
    { // checks if the map is not too small or too large (min 2x2, max 15x15)
        int height = field.GetLength(0);
        int width = field.GetLength(1);

        if (height < 2 || width < 2)
        {
            Console.WriteLine("Map is te klein! Minimumgrootte is 2x2.");
            Console.ReadLine();
            return false;
        }
        if (height > 15 || width > 15) {
            Console.WriteLine("Map is te groot! Maximumgrootte is 15x15.");
            Console.ReadLine();
            return false;
        }
        return true;
    }
    static bool CheckCharacters(char[,] field)
    { // checks that there are no invalid symbols in the map
        // MTR – For lus
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            { // checks every position in the array; if a symbol is not player, rock, ice, or brokenIce, it returns false and displays which one is wrong
                char currentCharacter = field[i, j];
                if (currentCharacter != player && currentCharacter != rock && currentCharacter != ice &&
                    currentCharacter != brokenIce)
                {
                    Console.WriteLine($"Ongeldig symbool '{currentCharacter}' gevonden op positie ({i}, {j})!");
                    Console.WriteLine("Geldige symbolen zijn: P (speler), R (rots), - (ijs), O (gebroken ijs).");
                    Console.ReadLine();
                    return false;
                }
            }
        }

        return true;
    }
    static bool CheckPlayerCount(char[,] field)
    { // checks that there is exactly 1 player.
        int count = 0;
        // MTR – For lus
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            { // goes through every tile and increases count by 1 for each player found
                if (field[i, j] == player) count++;
            }
        }
        if  (count == 1) return true; // if count equals 1, exactly one player exists, so it's valid.
        Console.WriteLine(count == 0 // if count is 0, show that there are no players
            ? "Er staat geen speler (P) op de map." // if count is not 0 (2 or more since 1 was already checked)
            : "Er staan meerdere spelers (P) op de map."); // then show that there are multiple players.
        Console.ReadLine();
        return false; // in both invalid cases (0 or multiple), return false.
    }
    static bool CheckIceField(char[,] field)
    { // checks if there is at least one ice tile.
        int count = 0;
        // MTR – For lus
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {// goes through every tile and increases count by 1 for each ice tile found
                if (field[i, j] == ice) count++;
            }
        }
        if  (count >= 1) return true; // if at least one ice tile exists, return true
        Console.WriteLine("Er zijn geen ijsvlakken");
        Console.ReadLine(); // if no ice tiles exist, return false with an error message
        return false;
    }
    static bool CheckForIsolatedIce(char[,] field)
    { // checks if there is any ice tile completely surrounded by rocks or borders, making it unreachable (and thus unwinnable).
        int height = field.GetLength(0);
        int width  = field.GetLength(1);
        // MTR – For lus
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            { // goes through every tile and checks if it’s an ice tile
                if (field[i, j] == ice) 
                { // if it’s ice, checks if there is ice to the left, right, above, or below. One ice tile in any of the four directions is enough to return true.
                    bool isValid = CheckIsolatedIceRight(field,i,j) || CheckIsolatedIceLeft(field,i,j) || CheckIsolatedIceBelow(field,i,j) || CheckIsolatedIceAbove(field,i,j);
                    if (!isValid) //    if isValid is false
                    { //                then the map is unwinnable and returns false with an error message.
                        Console.WriteLine("Er is een ijsvlak niet toegankelijk en deze map is dus unwinnable...");
                        Console.ReadLine();
                        return false;
                    }
                };
            }
        }
        return true;
    }
    static bool CheckIsolatedIceRight(char[,] field, int y, int x)
    {  
        int width  = field.GetLength(1);
        if (x == width-1) return false; // checks if you are on the right border
        if (field[y, x + 1] == ice) return true; // if not on the edge, check if the right tile is ice; if yes, return true (reachable).
        return false; // if it's not ice (then it's a rock), return false
    }
    static bool CheckIsolatedIceLeft(char[,] field, int y, int x)
    { // same as right but for the left side
        if (x == 0) return false;
        if (field[y, x - 1] == ice) return true;
        return false;
    }
    static bool CheckIsolatedIceBelow(char[,] field, int y, int x)
    { // same but for below
        int height = field.GetLength(0);
        if (y == height-1) return false;
        if (field[y + 1, x] == ice) return true;
        return false;
    }
    static bool CheckIsolatedIceAbove(char[,] field, int y, int x)
    { // same but for above
        if (y == 0) return false;
        if (field[y - 1, x] == ice) return true;
        return false;
    }
}



