using System;
using System.Linq;

namespace Interpritator
{
    /// <summary>
    /// Scans the program text. Divides it into tokens and returns one lexeme to the class Parser.
    /// </summary>
    class Scanner
    {
        /// <summary>
        /// Finite state machine states for Scanner
        /// </summary>
        enum state { H, IDENT, NUMB, M_NUMB, CHAR, STRING, FLOAT_FRACTION, MINUS, ALE, DELIM, NEQ, ARR_SIZE};
        // H = start, IDENT = Detect an identifier, NUMB, M_NUMB = Detect a number, CHAR = Detect a character, STRING = Detect a string, 
        // FLOAT_FRACTION = Detect the value of float after the point, COM = minus, ALE = ( <=, >=, -- etc.) , DELIM = the symbol from TD, 
        // NEQ = !=, ARR_SIZE = Count defined array size.

        /// <summary>
        /// Key Words that uses programming language
        /// </summary>
        static string[] TW = {"", "and", "{", "bool", "else", "}", "if", "false", "int",
                              "int[]", "char", "string", "string[]", "float", "float[]", "not", "or",
                              "ArloopaScript", "void","read", "true", "while", "write", null};

        /// <summary>
        /// Key Words' types
        /// </summary>
        static type_of_lex[] words = {type_of_lex.LEX_NULL, type_of_lex.LEX_AND, type_of_lex.LEX_BEGIN, type_of_lex.LEX_BOOL,
                                      type_of_lex.LEX_ELSE, type_of_lex.LEX_END, type_of_lex.LEX_IF, type_of_lex.LEX_FALSE, type_of_lex.LEX_INT,
                                      type_of_lex.LEX_INT_ARR, type_of_lex.LEX_CHAR, type_of_lex.LEX_STRING, type_of_lex.LEX_STRING_ARR,
                                      type_of_lex.LEX_FLOAT, type_of_lex.LEX_FLOAT_ARR, type_of_lex.LEX_NOT, type_of_lex.LEX_OR, type_of_lex.LEX_PROGRAM,
                                      type_of_lex.LEX_PROC, type_of_lex.LEX_READ, type_of_lex.LEX_TRUE,
                                      type_of_lex.LEX_WHILE, type_of_lex.LEX_WRITE, type_of_lex.LEX_NULL};

        /// <summary>
        /// Symbols that uses programming language
        /// </summary>
        static string[] TD = { "", ".", "'", "\"", "@", ";", ",", ":", "=", "(", ")", "==", "<", ">", "+", "-", "*", "/", "<=", "!=", ">=", null };

        /// <summary>
        /// Symbols' types
        /// </summary>
        static type_of_lex[] dlms = {type_of_lex.LEX_NULL, type_of_lex.LEX_DOT, type_of_lex.LEX_APOSTROPHE, type_of_lex.LEX_QUOTE, type_of_lex.LEX_FIN,
                                     type_of_lex.LEX_SEMICOLON, type_of_lex.LEX_COMMA, type_of_lex.LEX_COLON, type_of_lex.LEX_ASSIGN, type_of_lex.LEX_LPAREN,
                                     type_of_lex.LEX_RPAREN, type_of_lex.LEX_EQ, type_of_lex.LEX_LSS, type_of_lex.LEX_GTR, type_of_lex.LEX_PLUS,
                                     type_of_lex.LEX_MINUS, type_of_lex.LEX_TIMES, type_of_lex.LEX_SLASH, type_of_lex.LEX_LEQ, type_of_lex.LEX_NEQ,
                                     type_of_lex.LEX_GEQ, type_of_lex.LEX_NULL};

        state CS; // current state
        string code; // program text
        int index = 0; // the index of current character
        char c; // current character
        string buf; // buffer for making word
        int array_size; // saves the size of array if the variable is array
        bool isminusvalue = false;

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        void clear()
        {
            isminusvalue = false;
            array_size = 0;
            buf = "";
        }

        /// <summary>
        /// Add character to buffer.
        /// </summary>
        void add()
        {
            buf += c.ToString();
        }

        /// <summary>
        /// Looks in Key Words and Symbols tables if the buffers word contains or not.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="list"></param>
        /// <returns> Returns the index of table's element.</returns>
        int look(string buf, string[] list)
        {
            int i = 0;
            while (i < list.Length)
            {
                if (buf.Equals(list[i]))
                    return i;
                ++i;
            }
            return 0;
        }

        /// <summary>
        /// Get new character.
        /// </summary>
        void gc()
        {
            if(index<code.Length)
            c = code[index];
            index++;
        }

        /// <summary>
        /// Saving code, seting state to Start, Calling functions clear() and gc().
        /// </summary>
        /// <param name="text"></param>
        public Scanner(string text)
        {
            code = text;
            CS = state.H;
            clear();
            gc();
        }

        /// <summary>
        /// Reading code symbol by symbol, if its correct word, than returns new lexema, else throwing expection about it. 
        /// </summary>
        /// <returns>Return lexema</returns>
        public Lex get_lex()
        {
            int d = 0; // Digit value 
            int j; // Saving index from TW or TD 
            int i =1; // The number of zeros after point in float value 
            float f = 0; // Float value
            CS = state.H; // Saving current state

            while (true)
            {
                    switch (CS)
                    {
                        case state.H:
                        if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                            gc();
                        else if (Char.IsLetter(c) || c == '{' || c == '}')
                        {
                            clear();
                            add();
                            gc();
                            CS = state.IDENT;
                        }
                        else if (Char.IsDigit(c))
                        {
                            d = c - '0';
                            gc();
                            CS = state.NUMB;
                        }
                        //else if (c == '{')
                        //{
                        //    gc();
                        //    CS = state.COM;
                        //}
                        else if (c == '=' || c == '<' || c == '>' || c == '-')
                        {
                            clear();
                            add();
                            if (c == '-')
                            {
                                CS = state.MINUS;
                                gc();
                            }
                            else
                            {
                                gc();
                                CS = state.ALE;
                            }
                        }
                        else if (c == '\'')
                        {
                            clear();
                            gc();
                            add();
                            CS = state.CHAR;
                        }
                        else if (c == '"')
                        {
                            clear();
                            gc();
                            CS = state.STRING;
                        }
                        else if (c == '@')
                            return new Lex(type_of_lex.LEX_FIN);
                        else if (c == '!')
                        {
                            clear();
                            add();
                            gc();
                            CS = state.NEQ;
                        }
                        else
                            CS = state.DELIM;
                            break;

                        case state.IDENT:
                        {
                            if (Char.IsLetterOrDigit(c) && index != code.Length + 1)
                            {
                                add();
                                gc();
                            }
                            else if (c == '[' && index != code.Length + 1)
                            {
                                j = look(buf, TW);
                                if(j != 0)
                                    add();
                                gc();
                                CS = state.ARR_SIZE;
                            }
                            else
                            {
                                j = look(buf, TW);
                                if (buf.Contains('[') && buf.Contains(']'))
                                {
                                    Globals.TID_List[Globals.curr_TID].put_size(array_size);
                                }
                                if (j != 0)
                                {
                                    return new Lex(words[j], j);
                                }
                                else
                                {
                                    j = Globals.TID_List[Globals.curr_TID].put(buf);
                                    return new Lex(type_of_lex.LEX_ID, j, array_size);
                                }
                            }
                            break;
                        }

                    case state.ARR_SIZE:
                        {
                            if (Char.IsDigit(c) && index != code.Length + 1)
                            {
                                d = d * 10 + (c - '0');
                                gc();
                            }
                            else if (c == ']')
                            {
                                array_size = d;
                                j = look(buf+']', TW);
                                if(j!=0)
                                    add();
                                gc();
                                CS = state.IDENT;
                            }
                            else
                                throw new Exception(" ']' expected ");
                            break;
                        }

                        case state.NUMB:
                        {
                            if (Char.IsDigit(c) && index != code.Length + 1)
                            {
                                d = d * 10 + (c - '0');
                                gc();
                            }
                            else if (c == '.' && index != code.Length + 1)
                            {
                                f = d;
                                gc();
                                CS = state.FLOAT_FRACTION;
                            }
                            else
                            {
                                if (isminusvalue)
                                    d *= -1;
                                return new Lex(type_of_lex.LEX_NUM, d);
                            }
                            break;
                        }

                    case state.M_NUMB:
                        {
                            if (Char.IsDigit(c))
                            {
                                CS = state.NUMB;
                            }
                            else throw new Exception(" Digit expected ");
                            break;
                        }

                    case state.FLOAT_FRACTION:
                        {
                            if (Char.IsDigit(c) && index != code.Length + 1)
                            {
                                f += (c - '0') / (float)(i * 10);
                                i++;
                                gc();
                            }
                            else
                                return new Lex(type_of_lex.LEX_FLOAT, f);
                            break;
                        }

                    case state.CHAR:
                        {
                            gc();
                            if (c == '\'')
                            {
                                gc();
                                return new Lex(type_of_lex.LEX_CHAR, buf[0]);
                            }
                            else
                                throw new Exception(" '  expected");
                        }

                    case state.STRING:
                        {
                            if (c != '"' && index != code.Length+1)
                            {
                                add();
                                gc();
                            }
                            else
                            {
                                if(index == code.Length+1)
                                {
                                    throw new Exception(" \" expected");
                                }
                                gc();
                                return new Lex(type_of_lex.LEX_STRING, buf);
                            }
                            break;
                        }

                    case state.MINUS:
                        {
                           if (c == '-')
                            {
                                isminusvalue = true;
                                gc();
                                CS = state.M_NUMB;
                                break;
                            }
                            else
                            {
                                j = look(buf, TD);
                                return new Lex(dlms[j], j);
                            }
                        }

                    case state.ALE:
                        {
                            if (c == '=')
                            {
                                add();
                                gc();
                                j = look(buf, TD);
                                return new Lex(dlms[j], j);
                            }                        
                            else
                            {
                                j = look(buf, TD);
                                return new Lex(dlms[j], j);
                            }
                        }

                        case state.NEQ:
                        {
                            if (c == '=')
                            {
                                add();
                                gc();
                                j = look(buf, TD);
                                return new Lex(type_of_lex.LEX_NEQ, j);
                            }
                            else
                                throw new Exception("Unexpected symbol - !");
                        }

                        case state.DELIM:
                        {
                            clear();
                            add();
                            j = look(buf, TD);
                            if (j != 0)
                            {
                                gc();
                                return new Lex(dlms[j], j);
                            }
                            else
                                throw new Exception("Unexpected symbol '" + c.ToString() + "'");
                        }
                    }
                }
            }
        }
    }

