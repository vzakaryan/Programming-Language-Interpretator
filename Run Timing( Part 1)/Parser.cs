using System;
using System.Collections.Generic;

namespace Interpritator
{
    enum parser_state
    {
        // Program states
        P_BEG, P1, P2, P3, P4, P_END,
        // Procedure states
        P2_1, P2_2, PF_BEG, PP1, PP2, PFV_BEG, PFV1, PFV2, PFV_END, PP3, PP4, PP5, PP6, PF_END,
        //Var states
        Y_BEG, Y1, Y_END,
        // Variable declaring states
        D_BEG, D1, D2, D_END,
        // Begin/end states
        B_BEG, B1, B2, B_END,
        // Statement states
        S_BEG, S_CHECK, S_CHECK_1, S_CHECK_END, S_UP, S1, S2, S3, S4, S5, S6, S7, S8, S9, S10, S11, S12, S13, S14, S15, S16, S_END, SP_BEG, SP1, SP2, SP_END,
        // Expression states
        E_BEG, E1, E2, E_END,
        // Plus, Minus, Or states
        X_BEG, X1, X_END,
        // Multiple, Divide, And states
        T_BEG, T1, T_END,
        // Everything else
        F_BEG, F1, F2, F3, F4, F5, F_END
    }

    /// <summary>
    /// Save indexes for jump to.
    /// </summary>
    struct temp_blank_struct
    {
        public int pl0, pl1;
    }

    /// <summary>
    /// Get lexema, analyze Syntax and Semantic statemants. Than creating Poliz array with lexemas.   
    /// </summary>
    class Parser
    {
        object c_val; // Current value of lexema
        int c_index; // current index of lexema
        type_of_lex c_type; // current type of lexema
        parser_state p_state; // current parser state
        Lex curr_lex; // current lexema
        Scanner scan; // Scaner's object
        public Poliz prog = new Poliz(1000); // Program's poliz array 
        temp_blank_struct temp_blank; // temp_blank_struct object

        int st_p = 0; // called procedure parameters count
        int proc_curr_tid; // TID index of current procedure
        bool proc_bool = false; // activate parameters counting

        Stack<string> parser_stack = new Stack<string>(); // Marks for control for return from statment 
        Stack<object> semantic_stack = new Stack<object>(); // Check is variable declared or not
        Stack<type_of_lex> lex_stack = new Stack<type_of_lex>(); // Saving lexemas type
        Stack<temp_blank_struct> temp_blank_stack = new Stack<temp_blank_struct>();  // saving indexes for jump to
 
        /// <summary>
        /// Register procedure, to be able to call after. Create new TID and change current TID index to that.
        /// </summary>
        void reg_proc()
        {
            Globals.TID_List[Globals.curr_TID][(int)curr_lex.get_value()].put_declare();
            Globals.TID_List[Globals.curr_TID][(int)curr_lex.get_value()].put_type(type_of_lex.LEX_PROC);
            Globals.TID_List.Add(new Tabl_ident());
            Globals.TID_List[Globals.curr_TID][(int)c_val].set_TID(Globals.TID_List.Count-1);
            Globals.TID_List[Globals.curr_TID + 1].prev_TID = Globals.curr_TID;
            Globals.curr_TID += 1;
        }

        /// <summary>
        /// Declaring variable.
        /// </summary>
        /// <param name="type">Type of variable</param>
        void dec(type_of_lex type)
        {
            int i;
            while (semantic_stack.Count != 0)
            {
                i = (int)semantic_stack.Pop(); 
                if (Globals.TID_List[Globals.curr_TID][i].get_declare())
                    throw new Exception("Declared twice");
                else
                {
                    Globals.TID_List[Globals.curr_TID][i].put_declare();
                    Globals.TID_List[Globals.curr_TID][i].put_type(type);
                }
            }
        }

        /// <summary>
        /// Checking is variable declared or not.
        /// </summary>
        void check_id()
        {
            int tmp = Globals.curr_TID; 

            while (tmp != -1)
            {
                if (Globals.TID_List[tmp][(int)c_val].get_declare())
                {
                    lex_stack.Push(Globals.TID_List[tmp][(int)c_val].get_type());
                    break;
                }
                if (Globals.TID_List[tmp].prev_TID == -1)
                    throw new Exception(" not declared ");
                tmp -= 1;
            }
        }

        /// <summary>
        /// Checking is operation can be done with this type of variables
        /// </summary>
        void check_op()
        {
            // t1, t2 is variables. op is operation. (t,r,s) types for compare variables type
            type_of_lex t1, t2, op, t = type_of_lex.LEX_INT, r = type_of_lex.LEX_BOOL, s = type_of_lex.LEX_STRING;

            t2 = lex_stack.Pop();
            op = lex_stack.Pop();
            t1 = lex_stack.Pop();

            if (op == type_of_lex.LEX_PLUS || op == type_of_lex.LEX_MINUS || op == type_of_lex.LEX_TIMES || op == type_of_lex.LEX_SLASH)

                if (t1 == type_of_lex.LEX_CHAR)
                {
                    r = type_of_lex.LEX_CHAR;
                }
                else if (t1 == type_of_lex.LEX_INT)
                {
                    r = type_of_lex.LEX_INT;
                }
                else if (t1 == type_of_lex.LEX_FLOAT)
                {
                    r = type_of_lex.LEX_FLOAT;
                }
                else if (t1 == type_of_lex.LEX_INT_ARR)
                {
                    r = type_of_lex.LEX_INT_ARR;
                }
                else if(t1 == type_of_lex.LEX_FLOAT_ARR)
                {
                    r = type_of_lex.LEX_FLOAT_ARR;
                }
                else
                {
                    if (op == type_of_lex.LEX_PLUS)
                        r = type_of_lex.LEX_STRING;
                    else
                        throw new Exception("Wrong operation with string");
                }

            if (op == type_of_lex.LEX_OR || op == type_of_lex.LEX_AND)
                t = type_of_lex.LEX_BOOL;
            if (t1 == t2 && t1 == t)
                lex_stack.Push(r);
            else if (t1 == type_of_lex.LEX_CHAR)
            {
                lex_stack.Push(type_of_lex.LEX_CHAR);
            }
            else if (t1 == type_of_lex.LEX_INT_ARR && (t1 == t2 || t2 == type_of_lex.LEX_INT))
            {
                lex_stack.Push(r);
            }
            else if (t1 == type_of_lex.LEX_FLOAT_ARR && (t1 == t2 || t2 == type_of_lex.LEX_FLOAT || t2 == type_of_lex.LEX_INT || t2 == type_of_lex.LEX_INT_ARR))
            {
                lex_stack.Push(r);
            }
            else if (t2 == type_of_lex.LEX_FLOAT_ARR && (t1 == t2 || t1 == type_of_lex.LEX_FLOAT || t1 == type_of_lex.LEX_INT || t1 == type_of_lex.LEX_INT_ARR))
            {
                lex_stack.Push(r);
            }
            else if (t1 == type_of_lex.LEX_FLOAT && (t1 == t2 || t2 == type_of_lex.LEX_INT_ARR || t2 == type_of_lex.LEX_INT || t2 == type_of_lex.LEX_FLOAT_ARR))
            {
                lex_stack.Push(r);
            }
            else if (t2 == type_of_lex.LEX_FLOAT && (t1 == t2 || t1 == type_of_lex.LEX_INT_ARR || t1 == type_of_lex.LEX_INT || t1 == type_of_lex.LEX_FLOAT_ARR))
            {
                lex_stack.Push(r);
            }
            else if ((t1 == t2 || t2 == type_of_lex.LEX_CHAR) && t1 == s)
                lex_stack.Push(r);
            else if (t1 == t2 || t2 == type_of_lex.LEX_INT)
                lex_stack.Push(r);
            else
                throw new Exception(t1 + " , " + t2 + " Wrong types are in operation");

            prog.put_lex(new Lex(op));
        }

        /// <summary>
        /// Checking is variables type is boolean for 'not'(!) operation.
        /// </summary>
        void check_not()
        {
            if (lex_stack.Pop() != type_of_lex.LEX_BOOL)
                throw new Exception(" Wrong type is in not");
            else
            {
                lex_stack.Push(type_of_lex.LEX_BOOL);
                prog.put_lex(new Lex(type_of_lex.LEX_NOT));
            }
        }

        /// <summary>
        /// Checking is variables types equals or not.
        /// </summary>
        void eq_type()
        {
            type_of_lex t1, t2; // variables for compare
            t1 = lex_stack.Pop();
            t2 = lex_stack.Pop();
            if ((t1 == type_of_lex.LEX_INT && t2 == type_of_lex.LEX_INT_ARR) || (t1 == type_of_lex.LEX_INT_ARR && t2 == type_of_lex.LEX_INT) ||
                (t1 == type_of_lex.LEX_FLOAT && t2 == type_of_lex.LEX_FLOAT_ARR) || (t1 == type_of_lex.LEX_FLOAT_ARR && t2 == type_of_lex.LEX_FLOAT))
                return;
            if (t1 != t2)
                throw new Exception(" Wrong types are in :=");
        }

        /// <summary>
        /// Checking, if expression returns type bollean or not.
        /// </summary>
        void eq_bool()
        {
            type_of_lex t;
            t = lex_stack.Pop();
            if (t != type_of_lex.LEX_BOOL)
                throw new Exception(" Expression is not boolean");
        }

        /// <summary>
        /// Checking is variable declared for reading.
        /// </summary>
        void check_id_in_read()
        {
            if (!Globals.TID_List[Globals.curr_TID][(int)c_val].get_declare())
                throw new Exception(" not declared ");
        }

        /// <summary>
        /// Getting new lexema from Scanner.
        /// </summary>
        void gl()
        {
            curr_lex = scan.get_lex();
            c_type = curr_lex.get_type();
            c_val = curr_lex.get_value();
            c_index = curr_lex.get_index();
        }

        public Parser(string t)
        {
            scan = new Scanner(t);
        }

        /// <summary>
        /// Analyzing code text for syntax and semantic errors.
        /// </summary>
        public void analyze()
        {
            gl();
            p_state = parser_state.P_BEG;
            parser_stack.Push("(start");

            do
            {
                switch (p_state)
                {
                    #region P states
                    case parser_state.P_BEG:
                        {
                            if (c_type == type_of_lex.LEX_PROGRAM)
                            {
                                gl();
                                p_state = parser_state.P1;
                            }
                            else
                            {
                                throw new Exception(" 'ArloopaScript' expected ");
                            }

                            break;
                        }

                    case parser_state.P1:
                        {
                            parser_stack.Push("(PY");
                            p_state = parser_state.Y_BEG;
                            break;
                        }

                    case parser_state.P2:
                        {
                            if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                //////p_state = parser_state.P3;
                                p_state = parser_state.P2_1;
                            }
                            else
                            {
                                throw new Exception(" ';' expected");
                            }
                            break;
                        }

                    case parser_state.P2_1:
                        {
                            parser_stack.Push("(PPF");
                            p_state = parser_state.PF_BEG;
                            break;
                        }

                    case parser_state.PF_BEG:
                        {
                            if (c_type == type_of_lex.LEX_PROC)
                            {
                                gl();
                                p_state = parser_state.PP1;
                            }
                            else
                                p_state = parser_state.PF_END;
                            break;
                        }

                    case parser_state.PP1:
                        {
                            if (c_type == type_of_lex.LEX_ID)
                            {
                                if(Globals.TID_List[Globals.curr_TID][(int)c_val].get_name().Equals("update"))
                                {
                                    Globals.isUpdateFunction = true;
                                }
                                reg_proc();
                                p_state = parser_state.PP2;
                                gl();
                            }
                            else
                                throw new Exception(" Identifier expected");
                            break;
                        }

                    case parser_state.PP2:
                        {
                            parser_stack.Push("(PFV");
                            p_state = parser_state.PFV_BEG;
                            break;
                        }

                    case parser_state.PFV_BEG:
                        {
                            
                            if (c_type == type_of_lex.LEX_LPAREN)
                            {
                                p_state = parser_state.PFV1;
                                proc_bool = true;
                                gl();
                            }
                            else
                                throw new Exception(" '(' expected ");
                            break;
                        }

                    case parser_state.PFV1:
                        {
                            if (c_type != type_of_lex.LEX_RPAREN && !Globals.isUpdateFunction)
                            {
                                parser_stack.Push("(PFVD");
                                
                                p_state = parser_state.D_BEG;
                            }
                            else
                            {
                                gl();
                                p_state = parser_state.PFV_END;
                            }
                            break;
                        }

                    case parser_state.PFV2:
                        {
                            if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                p_state = parser_state.PFV1;
                            }
                            else if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                gl();
                                proc_bool = false;
                                p_state = parser_state.PFV_END;
                            }
                            else
                                throw new Exception(" Wrong expression ");
                            break;
                        }

                    case parser_state.PFV_END:
                        {
                            if (parser_stack.Pop() == "(PFV")
                            {
                                proc_bool = false;
                                p_state = parser_state.PP3;
                            }
                            else
                                throw new Exception(" Wrong expression ");
                            break;
                        }

                    case parser_state.PP3:
                        {
                            parser_stack.Push("(PPY");
                            p_state = parser_state.Y_BEG;
                            break;
                        }

                    case parser_state.PP4:
                        {
                            if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                p_state = parser_state.PP5;
                            }
                            else throw new Exception(" ';' expected");
                            break;
                        }

                    case parser_state.PP5:
                        {
                            parser_stack.Push("(PPF1");
                            p_state = parser_state.PF_BEG;
                            break;
                        }

                    case parser_state.PP6:
                        {
                            parser_stack.Push("(PPB");
                            Globals.TID_List[Globals.curr_TID].pl0 = prog.get_free();
                            p_state = parser_state.B_BEG;
                            break;
                        }

                    case parser_state.PF_END:
                        {
                            string tmp = parser_stack.Pop();
                            if (tmp == "(PPF")
                                p_state = parser_state.P2_2;
                            else if (tmp == "(PPF1")
                                p_state = parser_state.PP6;
                            else
                                throw new Exception(" Wrong expression");
                            break;
                        }

                    case parser_state.P2_2:
                        {
                            if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                p_state = parser_state.P2_1;
                            }
                            else
                            p_state = parser_state.P3;
                            break;
                        }

                    case parser_state.P3:
                        {
                            prog.put_lex(new Lex(type_of_lex.POLIZ_START));
                            parser_stack.Push("(PB");
                            p_state = parser_state.B_BEG;
                            break;
                        }

                    case parser_state.P4:
                        {
                            if (c_type == type_of_lex.LEX_FIN)
                            {
                                p_state = parser_state.P_END;
                            }
                            else
                            {
                                throw new Exception(" '@' expected ");
                            }
                            break;
                        }
                    #endregion

                    #region Y states
                    case parser_state.Y_BEG:
                        {
                            parser_stack.Push("(YD");
                            p_state = parser_state.D_BEG;
                            break;
                        }

                    case parser_state.Y_END:
                        {
                            string tmp;
                            if (c_type == type_of_lex.LEX_COMMA)
                            {
                                gl();
                                p_state = parser_state.Y1;
                            }
                            else if ((tmp = parser_stack.Pop()) == "(PY")
                            {
                                p_state = parser_state.P2;
                            }
                            else if (tmp == "(PPY")
                            {
                                p_state = parser_state.PP4;
                            }
                            else
                            {
                                throw new Exception(" ',' expected or wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region D states
                    case parser_state.D_BEG:
                        {
                            if (c_type == type_of_lex.LEX_ID)
                            {
                                //Console.WriteLine("Cur_tid = " + Globals.curr_TID);
                                if (proc_bool)
                                    Globals.TID_List[Globals.curr_TID].param_count++;

                                semantic_stack.Push(c_val);
                                gl();
                                p_state = parser_state.D1;
                            }
                            else
                            {
                                throw new Exception(" Identifier expected ");
                            }
                            break;
                        }

                    case parser_state.D1:
                        {
                            if (c_type == type_of_lex.LEX_COLON)
                            {
                                Globals.TID_List[proc_curr_tid].param_count++;
                                gl();
                                p_state = parser_state.D2;
                            }
                            else if (c_type == type_of_lex.LEX_COMMA)
                            {
                                Globals.TID_List[proc_curr_tid].param_count++;
                                gl();
                                p_state = parser_state.D_BEG;
                            }
                            else
                            {
                                throw new Exception(" ',' or ':' expected ");
                            }
                            break;
                        }

                    case parser_state.D2:
                        {
                            if (c_type == type_of_lex.LEX_INT || c_type == type_of_lex.LEX_BOOL || c_type == type_of_lex.LEX_CHAR || c_type == type_of_lex.LEX_STRING ||
                                c_type == type_of_lex.LEX_STRING_ARR || c_type == type_of_lex.LEX_FLOAT || c_type == type_of_lex.LEX_INT_ARR || c_type == type_of_lex.LEX_FLOAT_ARR)
                            {
                                if (c_type == type_of_lex.LEX_INT)
                                    dec(type_of_lex.LEX_INT);
                                else if (c_type == type_of_lex.LEX_BOOL)
                                    dec(type_of_lex.LEX_BOOL);
                                else if (c_type == type_of_lex.LEX_CHAR)
                                    dec(type_of_lex.LEX_CHAR);
                                else if (c_type == type_of_lex.LEX_STRING)
                                    dec(type_of_lex.LEX_STRING);
                                else if (c_type == type_of_lex.LEX_FLOAT)
                                    dec(type_of_lex.LEX_FLOAT);
                                else if (c_type == type_of_lex.LEX_INT_ARR)
                                    dec(type_of_lex.LEX_INT_ARR);
                                else if (c_type == type_of_lex.LEX_FLOAT_ARR)
                                    dec(type_of_lex.LEX_FLOAT_ARR);
                                else if (c_type == type_of_lex.LEX_STRING_ARR)
                                    dec(type_of_lex.LEX_STRING_ARR);

                                gl();
                                p_state = parser_state.D_END;
                                
                            }
                            else
                            {
                                throw new Exception(" Type expected");
                            }
                            break;
                        }

                    case parser_state.D_END:
                        {
                            string tmp = parser_stack.Pop();
                            if (tmp == "(YD")
                            {
                                p_state = parser_state.Y_END;
                            }
                            else if(tmp == ("(PFVD"))
                            {
                                p_state = parser_state.PFV2;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region B states
                    case parser_state.B_BEG:
                        {
                            if (c_type == type_of_lex.LEX_BEGIN)
                            {
                                gl();
                                p_state = parser_state.B1;
                            }
                            else
                            {
                                throw new Exception(" 'begin' expected ");
                            }
                            break;
                        }

                    case parser_state.B1:
                        {
                            parser_stack.Push("(BS");
                            p_state = parser_state.S_BEG;
                            break;
                        }

                    case parser_state.B2:
                        {
                            if (c_type == type_of_lex.LEX_END)
                            {
                                gl();
                                p_state = parser_state.B_END;
                            }
                            else if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                p_state = parser_state.B1;
                            }
                            else throw new Exception(" 'end' or ';' expected ");
                            break;
                        }

                    case parser_state.B_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(PB")
                            {
                                p_state = parser_state.P4;
                            }
                            else if (tmp == "(SB")
                            {
                                p_state = parser_state.S_END;
                            }
                            else if (tmp == "(PPB")
                            {
                                Globals.TID_List[Globals.curr_TID].pl1 = prog.get_free();

                                if (Globals.isUpdateFunction)
                                {
                                    prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, Globals.TID_List[Globals.curr_TID].pl0));
                                }
                                else
                                {
                                    prog.blank();
                                }
                                prog.put_lex(new Lex(type_of_lex.POLIZ_GO));
                                Globals.curr_TID = Globals.TID_List[Globals.curr_TID].prev_TID;
                                Globals.isUpdateFunction = false;
                                p_state = parser_state.PF_END;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region S states
                    case parser_state.S_BEG:
                        {
                            if (c_type == type_of_lex.LEX_ID)
                            {
                                
                                check_id();
                               
                                p_state = parser_state.S_CHECK_1;
                                parser_stack.Push("(SC");

                                int tmp = Globals.curr_TID;
                                while (tmp != -1)
                                {
                                    if (Globals.TID_List[tmp][(int)c_val].get_declare())
                                    {
                                        if (Globals.TID_List[tmp][(int)c_val].get_name().Equals("update"))
                                        {
                                            Globals.isUpdateFunction = true;
                                        }
                                        proc_curr_tid = Globals.TID_List[tmp][(int)c_val].get_TID();
                                        if (Globals.TID_List[tmp][(int)c_val].get_type() == type_of_lex.LEX_PROC)
                                        {

                                        }
                                        else {
                                            prog.put_lex(new Lex(type_of_lex.POLIZ_ADDRESS, c_val, c_index));
                                        }
                                        break;
                                    }
                                    if (Globals.TID_List[tmp].prev_TID == -1)
                                        throw new Exception(" Undefined identifier ");
                                    tmp -= 1;
                                }
                            }

                            else if (c_type == type_of_lex.LEX_IF)
                            {
                                gl();
                                p_state = parser_state.S3;
                            }
                            else if (c_type == type_of_lex.LEX_WHILE)
                            {
                                gl();
                                p_state = parser_state.S8;
                            }
                            else if (c_type == type_of_lex.LEX_READ)
                            {
                                gl();
                                p_state = parser_state.S11;
                            }
                            else if (c_type == type_of_lex.LEX_WRITE)
                            {
                                gl();
                                p_state = parser_state.S14;
                            }
                            else if (c_type == type_of_lex.LEX_SEMICOLON)
                            {
                                gl();
                                p_state = parser_state.S_END;
                            }
                            else
                            {
                                parser_stack.Push("(SB");
                                p_state = parser_state.B_BEG;
                            }
                            break;
                        }

                    case parser_state.S_CHECK:
                        {
                            if (curr_lex.get_type() == type_of_lex.LEX_INT_ARR)
                            {
                                p_state = parser_state.S_CHECK_1;
                            }
                            else
                            {
                                p_state = parser_state.S_CHECK_END;
                            }
                            break;
                        }

                    case parser_state.S_CHECK_1:
                        {
                            int i = curr_lex.get_index();
                            if ((Globals.TID_List[Globals.curr_TID][(int)curr_lex.get_value()].get_type() == type_of_lex.LEX_INT_ARR && i == 0) || i < Globals.TID_List[Globals.curr_TID][(int)curr_lex.get_value()].get_size())
                            {
                                p_state = parser_state.S_CHECK_END;
                            }
                            else
                            {
                                throw new Exception(" index expected or out of range");
                            }
                            break;
                        }

                    case parser_state.S_CHECK_END:
                        {
                            if (parser_stack.Pop() == "(SC")
                            {
                                gl();
                                p_state = parser_state.S1;
                            }
                            else
                            {
                                throw new Exception(" Wrong expession ");
                            }
                            break;
                        }

                    case parser_state.S1:
                        {
                            if (c_type == type_of_lex.LEX_ASSIGN)
                            {
                                gl();
                                p_state = parser_state.S2;
                            }
                            else
                            {
                                p_state = parser_state.SP_BEG;
                                parser_stack.Push("(SSP");
                            }
                            break;
                        }

                    case parser_state.SP_BEG:
                        {
                            
                            if (c_type == type_of_lex.LEX_LPAREN)
                            {
                                Globals.proc_index.Enqueue(proc_curr_tid);
                                gl();
                                p_state = parser_state.SP1;
                            }
                            else
                            {
                                throw new Exception(" ':=' or '(' expected ");
                            }
                            break;
                        }

                    case parser_state.SP1:
                        {
                            if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                gl();
                                p_state = parser_state.SP_END;
                            }
                            else if(Globals.isUpdateFunction)
                            {
                                throw new Exception("Wrong call of Update function");
                            }
                            else if (c_type == type_of_lex.LEX_ID)
                            {
                                st_p++;
                                lex_stack.Push(Globals.TID_List[Globals.curr_TID][(int)c_val].get_type());
                                Globals.proc_queue.Enqueue(curr_lex);
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_NUM)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_INT);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_TRUE || c_type == type_of_lex.LEX_FALSE)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_BOOL);
                                if (c_type == type_of_lex.LEX_TRUE)
                                {
                                    Globals.proc_queue.Enqueue(true);
                                }
                                else
                                {
                                    Globals.proc_queue.Enqueue(false);
                                }
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_STRING)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_STRING);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_FLOAT)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_FLOAT);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_INT_ARR)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_INT_ARR);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_FLOAT_ARR)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_FLOAT_ARR);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_STRING_ARR)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_STRING_ARR);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else if (c_type == type_of_lex.LEX_CHAR)
                            {
                                st_p++;
                                lex_stack.Push(type_of_lex.LEX_CHAR);
                                Globals.proc_queue.Enqueue(curr_lex.get_value());
                                gl();
                                p_state = parser_state.SP2;
                            }
                            else throw new Exception("Wrong call of procedure");
                            break;
                        }


                    case parser_state.SP2:
                        {
                            if (c_type == type_of_lex.LEX_COMMA)
                            {
                                gl();
                                p_state = parser_state.SP1;
                            }
                            else if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                if (Globals.TID_List[proc_curr_tid].param_count == st_p)
                                {
                                    for (int i = 1; i <= Globals.TID_List[proc_curr_tid].param_count; i++)
                                    {
                                        Console.Error.WriteLine("proc cur tid param " + i + " = " + Globals.TID_List[proc_curr_tid][Globals.TID_List[proc_curr_tid].param_count - i + 1].get_type());
                                        Console.Error.WriteLine(" lex stack param " + i + " = " + lex_stack.Peek());
                                        if (lex_stack.Pop() == Globals.TID_List[proc_curr_tid][Globals.TID_List[proc_curr_tid].param_count - i + 1].get_type())
                                        {
                                            //Globals.TID_List[proc_curr_tid][i].put_value(Globals.proc_queue.Dequeue());
                                            //Globals.TID_List[proc_curr_tid][i].put_assign();
                                            st_p--;
                                        }
                                        else
                                            throw new Exception(" Wrong type of parameter");
                                    }
                                }
                                else
                                    throw new Exception(" Wrong number of parameters ");
                                gl();
                                p_state = parser_state.SP_END;
                            }
                            else throw new Exception(" ',' or ')' expected");
                            break;
                        }

                    case parser_state.SP_END:
                        {
                            if (parser_stack.Pop() == "(SSP")
                            {
                                prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, Globals.TID_List[proc_curr_tid].pl0));
                                prog.put_lex(new Lex(type_of_lex.POLIZ_GO_PROC));

                                if (!Globals.isUpdateFunction)
                                {
                                    Console.WriteLine("mtav");
                                    prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, prog.get_free()), Globals.TID_List[proc_curr_tid].pl1);
                                }
                                else
                                    Globals.isUpdateFunction = false;
                                
                               
                                p_state = parser_state.S_END;
                            }
                            else throw new Exception(" Wrong expression ");
                            break;
                        }

                    case parser_state.S2:
                        {
                            parser_stack.Push("(SE1");
                            p_state = parser_state.E_BEG;
                            break;
                        }

                    case parser_state.S3:
                        {
                            parser_stack.Push("(SE2");
                            p_state = parser_state.E_BEG;
                            break;
                        }

                    case parser_state.S5:
                        {
                            parser_stack.Push("(SS1");
                            temp_blank_stack.Push(temp_blank);
                            p_state = parser_state.S_BEG;
                            break;
                        }

                    case parser_state.S6:
                        {

                            if (c_type == type_of_lex.LEX_ELSE)
                            {
                                gl();
                                p_state = parser_state.S7;
                            }
                            else
                            {
                                throw new Exception(" 'else' expected ");
                            }
                            break;
                        }

                    case parser_state.S7:
                        {
                            parser_stack.Push("(SS2");
                            temp_blank_stack.Push(temp_blank);
                            p_state = parser_state.S_BEG;
                            break;
                        }

                    case parser_state.S8:
                        {
                            parser_stack.Push("(SE3");
                            temp_blank.pl0 = prog.get_free();
                            p_state = parser_state.E_BEG;
                            break;
                        }

                    case parser_state.S10:
                        {
                            parser_stack.Push("(SS3");
                            temp_blank_stack.Push(temp_blank);
                            p_state = parser_state.S_BEG;
                            break;
                        }

                    case parser_state.S11:
                        {
                            if (c_type == type_of_lex.LEX_LPAREN)
                            {
                                gl();
                                p_state = parser_state.S12;
                            }
                            else
                            {
                                throw new Exception(" '(' expexted ");
                            }
                            break;
                        }

                    case parser_state.S12:
                        {
                            if (c_type == type_of_lex.LEX_ID)
                            {
                                check_id_in_read();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_ADDRESS, c_val));
                                gl();
                                p_state = parser_state.S13;
                            }
                            else
                            {
                                throw new Exception(" Identifier expected ");
                            }
                            break;
                        }

                    case parser_state.S13:
                        {
                            if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                prog.put_lex(new Lex(type_of_lex.LEX_READ));
                                gl();
                                p_state = parser_state.S_END;
                            }
                            else
                            {
                                throw new Exception(" ')' expected ");
                            }
                            break;
                        }

                    case parser_state.S14:
                        {

                            if (c_type == type_of_lex.LEX_LPAREN)
                            {
                                gl();
                                p_state = parser_state.S15;
                            }
                            else
                            {
                                throw new Exception(" '(' expecte ");
                            }
                            break;
                        }

                    case parser_state.S15:
                        {
                            parser_stack.Push("(SE4");
                            p_state = parser_state.E_BEG;
                            break;
                        }

                    case parser_state.S16:
                        {
                            if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                prog.put_lex(new Lex(type_of_lex.LEX_WRITE));
                                gl();
                                p_state = parser_state.S_END;
                            }
                            else
                            {
                                throw new Exception(" ')' expected ");
                            }
                            break;
                        }

                    case parser_state.S_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(BS")
                            {
                                p_state = parser_state.B2;
                            }
                            else if (tmp == "(SS1")
                            {
                                temp_blank = temp_blank_stack.Pop();
                                temp_blank.pl1 = prog.get_free();
                                prog.blank();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_GO));
                                prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, prog.get_free()), temp_blank.pl0);
                                p_state = parser_state.S6;
                            }
                            else if (tmp == "(SS2")
                            {
                                temp_blank = temp_blank_stack.Pop();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, prog.get_free()), temp_blank.pl1);
                                p_state = parser_state.S_END;
                            }
                            else if (tmp == "(SS3")
                            {
                                temp_blank = temp_blank_stack.Pop();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, temp_blank.pl0));
                                prog.put_lex(new Lex(type_of_lex.POLIZ_GO));
                                prog.put_lex(new Lex(type_of_lex.POLIZ_LABEL, prog.get_free()), temp_blank.pl1);
                                p_state = parser_state.S_END;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region E states
                    case parser_state.E_BEG:
                        {
                            parser_stack.Push("(EX1");
                            p_state = parser_state.X_BEG;
                            break;
                        }

                    case parser_state.E1:
                        {
                            if (c_type == type_of_lex.LEX_EQ || c_type == type_of_lex.LEX_NEQ || c_type == type_of_lex.LEX_GEQ || c_type == type_of_lex.LEX_LEQ || c_type == type_of_lex.LEX_GTR || c_type == type_of_lex.LEX_LSS)
                            {
                                lex_stack.Push(c_type);
                                gl();
                                p_state = parser_state.E2;
                            }
                            else
                            {
                                p_state = parser_state.E_END;
                            }
                            break;
                        }

                    case parser_state.E2:
                        {
                            parser_stack.Push("(EX2");
                            p_state = parser_state.X_BEG;
                            break;
                        }

                    case parser_state.E_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(SE1")
                            {
                                eq_type();
                                prog.put_lex(new Lex(type_of_lex.LEX_ASSIGN));
                                p_state = parser_state.S_END;
                            }
                            else if (tmp == "(SE2")
                            {
                                eq_bool();
                                temp_blank.pl0 = prog.get_free();
                                prog.blank();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_FGO)); 
                                p_state = parser_state.S5;
                            }
                            else if (tmp == "(SE3")
                            {
                                eq_bool();
                                temp_blank.pl1 = prog.get_free();
                                prog.blank();
                                prog.put_lex(new Lex(type_of_lex.POLIZ_FGO));
                                p_state = parser_state.S10;
                            }
                            else if (tmp == "(SE4")
                            {
                                //lex_stack.Pop(); 
                                p_state = parser_state.S16;
                            }
                            else if (tmp == "(FE")
                            {
                                p_state = parser_state.F2;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }

                    #endregion

                    #region X states
                    case parser_state.X_BEG:
                        {
                            parser_stack.Push("(XT1");
                            p_state = parser_state.T_BEG;
                            break;
                        }

                    case parser_state.X1:
                        {
                            if (c_type == type_of_lex.LEX_OR || c_type == type_of_lex.LEX_PLUS || c_type == type_of_lex.LEX_MINUS)
                            {
                                lex_stack.Push(c_type);
                                parser_stack.Push("(XT2");
                                gl();
                                p_state = parser_state.T_BEG;
                            }
                            else
                            {
                                p_state = parser_state.X_END;
                            }
                            break;
                        }

                    case parser_state.X_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(EX1")
                            {
                                p_state = parser_state.E1;
                            }
                            else if (tmp == "(EX2")
                            {
                                check_op();
                                p_state = parser_state.E_END;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region T states
                    case parser_state.T_BEG:
                        {
                            parser_stack.Push("(TF1");
                            p_state = parser_state.F_BEG;
                            break;
                        }

                    case parser_state.T1:
                        {
                            if (c_type == type_of_lex.LEX_AND || c_type == type_of_lex.LEX_TIMES || c_type == type_of_lex.LEX_SLASH)
                            {
                                lex_stack.Push(c_type);
                                parser_stack.Push("(TF2");
                                gl();
                                p_state = parser_state.F_BEG;
                            }
                            else
                            {
                                p_state = parser_state.T_END;
                            }
                            break;
                        }

                    case parser_state.T_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(XT1")
                            {
                                p_state = parser_state.X1;
                            }
                            else if(tmp == "(XT2")
                            {
                                check_op();
                                p_state = parser_state.X1;
                            }
                            else
                            {
                                throw new Exception(" Wrong expression ");
                            }
                            break;
                        }
                    #endregion

                    #region F states
                    case parser_state.F_BEG:
                        {
                            if (c_type == type_of_lex.LEX_LPAREN)
                            {
                                gl();
                                p_state = parser_state.F1;
                            }
                            else if (c_type == type_of_lex.LEX_NOT)
                            {
                                gl();
                                p_state = parser_state.F3;
                            }
                            else if (c_type == type_of_lex.LEX_ID || c_type == type_of_lex.LEX_NUM || c_type == type_of_lex.LEX_TRUE || c_type == type_of_lex.LEX_FALSE || c_type == type_of_lex.LEX_CHAR || c_type == type_of_lex.LEX_STRING || c_type == type_of_lex.LEX_FLOAT || c_type == type_of_lex.LEX_INT_ARR || c_type == type_of_lex.LEX_FLOAT_ARR || c_type == type_of_lex.LEX_PROC)
                            {
                                if (c_type == type_of_lex.LEX_ID)
                                {
                                    check_id();
                                    prog.put_lex(curr_lex);
                                }
                                else if (c_type == type_of_lex.LEX_NUM)
                                {
                                    prog.put_lex(curr_lex);
                                    if (c_index == 0)
                                        lex_stack.Push(type_of_lex.LEX_INT);
                                    else
                                        lex_stack.Push(type_of_lex.LEX_INT_ARR);
                                }
                                else if (c_type == type_of_lex.LEX_CHAR)
                                {
                                    prog.put_lex(curr_lex);
                                    lex_stack.Push(type_of_lex.LEX_CHAR);
                                }
                                else if (c_type == type_of_lex.LEX_STRING)
                                {
                                    prog.put_lex(curr_lex);
                                    lex_stack.Push(type_of_lex.LEX_STRING);
                                }
                                else if (c_type == type_of_lex.LEX_TRUE || c_type == type_of_lex.LEX_FALSE)
                                {
                                    lex_stack.Push(type_of_lex.LEX_BOOL);
                                    prog.put_lex(curr_lex);
                                }
                                else if (c_type == type_of_lex.LEX_FLOAT)
                                {
                                    prog.put_lex(curr_lex);
                                    if (c_index == 0)
                                        lex_stack.Push(type_of_lex.LEX_FLOAT);
                                    else
                                        lex_stack.Push(type_of_lex.LEX_FLOAT_ARR);
                                }
                                else if (c_type == type_of_lex.LEX_FLOAT_ARR)
                                {
                                    lex_stack.Push(type_of_lex.LEX_FLOAT_ARR);
                                    prog.put_lex(curr_lex);
                                }
                                gl();
                                p_state = parser_state.F_END;
                            }
                            else
                            {
                                throw new Exception(" Wrong expressions ");
                            }
                            break;
                        }

                    case parser_state.F1:
                        {
                            parser_stack.Push("(FE");
                            p_state = parser_state.E_BEG;
                            break;
                        }

                    case parser_state.F2:
                        {
                            if (c_type == type_of_lex.LEX_RPAREN)
                            {
                                gl();
                                p_state = parser_state.F_END;
                            }
                            else
                            {
                                throw new Exception(" ')' expected ");
                            }
                            break;
                        }

                    case parser_state.F3:
                        {
                            parser_stack.Push("(FF");
                            p_state = parser_state.F_BEG;
                            break;
                        }

                    case parser_state.F_END:
                        {
                            string tmp = parser_stack.Pop().ToString();
                            if (tmp == "(FF")
                            {
                                check_not();
                                p_state = parser_state.F_END;
                            }
                            else if (tmp == "(TF1")
                            {
                                p_state = parser_state.T1;
                            }
                            else if (tmp == "(TF2")
                            {
                                check_op();
                                p_state = parser_state.T1;
                            }
                            
                            break;
                        }
                        #endregion
                }
            } while (p_state != parser_state.P_END) ;

            if (parser_stack.Pop().ToString() != "(start")
                throw new Exception(" Wrong Code ");
            else if (c_type == type_of_lex.LEX_FIN)
            {
                Console.WriteLine("Compile Succeeded");
            }
            else throw new Exception(" '@' expected ");
        }


    }
}
