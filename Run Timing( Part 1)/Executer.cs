using System;
using System.Collections.Generic;

namespace Interpritator
{
    /// <summary>
    /// Execute compiled program using POLIZ.
    /// </summary>
    class Executer
    {
        Lex pc_el = new Lex(); // program current element

        /// <summary>
        /// Execute compiled program using POLIZ.
        /// </summary>
        /// <param name="prog"></param>
        public void execute(Poliz prog) 
        {
            Stack<object> args = new Stack<object>(); // Saving all arguments in program
            object i, j; // taking values from args stack, for operations
            int index = 0; // current index of POLIZ array
            int size = prog.get_free(); // size of POLIZ array
            int arr_index = 0; // index of variable

            // Find the start of the program
            for (int k = 0; k < prog.get_free(); k++)
            {
                if(prog[k].get_type() == type_of_lex.POLIZ_START)
                {
                    index= k + 1;
                    break;
                }
            }

            while (index < size)
            {
                pc_el = prog[index];
                switch (pc_el.get_type())
                {
                    case type_of_lex.LEX_TRUE:
                        {
                            args.Push(0);
                            args.Push(true);
                            break;
                        }
                    case type_of_lex.LEX_FALSE:
                        {
                            args.Push(0);
                            args.Push(false);
                            break;
                        }

                    case type_of_lex.LEX_INT_ARR:
                        {
                            args.Push(pc_el.get_index());
                            args.Push(pc_el.get_index());
                            break;
                        }
                    case type_of_lex.LEX_CHAR:
                    case type_of_lex.LEX_STRING:
                    case type_of_lex.LEX_FLOAT:
                    case type_of_lex.POLIZ_LABEL:
                        {
                            args.Push(pc_el.get_index());
                            args.Push(pc_el.get_value());
                            break;
                        }
                    case type_of_lex.POLIZ_ADDRESS:
                    case type_of_lex.LEX_NUM:
                        {
                            args.Push(pc_el.get_index());
                            args.Push(pc_el.get_value());
                            break;
                        }

                    case type_of_lex.LEX_ID:
                        {
                            i = pc_el.get_value();
                            if (Globals.TID_List[pc_el.get_tid_index()][(int)i].get_assign())
                            {
                                args.Push(pc_el.get_index());
                                if (pc_el.get_index() > 0)
                                    args.Push(Globals.TID_List[pc_el.get_tid_index()][(int)i].get_value(pc_el.get_index()));
                                else                                
                                    args.Push(Globals.TID_List[pc_el.get_tid_index()][(int)i].get_value());
                                break;
                            }
                            else
                                throw new Exception(" POLIZ: indefinite identifier");
                        }

                    case type_of_lex.LEX_NOT:
                        {
                            if ((int)args.Pop() != 0)
                                args.Push(0);
                            else
                                args.Push(1);
                            break;
                        }

                    case type_of_lex.LEX_OR:
                        {
                            i = (int)args.Pop();
                            if ((int)args.Pop() == 0 && (int)i == 0)
                                args.Push(0);
                            else args.Push(1);
                            break;
                        }

                    case type_of_lex.LEX_AND:
                        {
                            i = args.Pop();
                            if ((int)args.Pop() == 0 || (int)i == 0)
                                args.Push(0);
                            else args.Push(1);
                            break;
                        }

                    case type_of_lex.POLIZ_GO:
                        {
                            index = (int)args.Pop() - 1;
                            break;
                        }

                    case type_of_lex.POLIZ_GO_PROC:
                        {
                            int proc_id = (int)Globals.proc_index.Dequeue();
                            index = (int)args.Pop() - 1;
                            for (int k = 1; k <= Globals.TID_List[proc_id].param_count; k++)
                            {
                                object tmp = Globals.proc_queue.Dequeue();
                                if (tmp.GetType() == typeof(Lex))
                                {
                                    Globals.TID_List[proc_id][k].put_value(Globals.TID_List[(tmp as Lex).get_tid_index()][(int)(tmp as Lex).get_value()].get_value());
                                }
                                else
                                {
                                    Globals.TID_List[proc_id][k].put_value(tmp);
                                }
                                Globals.TID_List[proc_id][k].put_assign();
                            }
                            break;
                        }

                    case type_of_lex.POLIZ_FGO:
                        {
                            i = args.Pop();
                            args.Pop();
                            if ((int)args.Pop() == 0)
                                index = (int)i - 1;
                            break;
                        }

                    case type_of_lex.LEX_WRITE:
                        {
                            Console.WriteLine(args.Pop());
                            args.Pop();
                            break;
                        }

                    case type_of_lex.LEX_READ:
                        {
                            int k;
                            i = args.Pop();
                            if (Globals.TID_List[pc_el.get_tid_index()][(int)i].get_type() == type_of_lex.LEX_INT)
                            {
                                Console.WriteLine("Input int value for " + Globals.TID_List[pc_el.get_tid_index()][(int)i].get_name());
                                k = Int32.Parse(Console.ReadLine());
                            }
                            else
                            {
                                string t = "";
                                rep:
                                Console.WriteLine("Input boolean value (true or false) for " + Globals.TID_List[pc_el.get_tid_index()][(int)i].get_name());
                                t = Console.ReadLine();

                                if (t.Equals("true"))
                                {
                                    k = 1;
                                }
                                else if (t.Equals("false"))
                                {
                                    k = 0;
                                }
                                else
                                {
                                    Console.WriteLine("Error in input:true/false");
                                    goto rep;
                                }
                            }
                            Globals.TID_List[pc_el.get_tid_index()][(int)i].put_value(k);
                            Globals.TID_List[pc_el.get_tid_index()][(int)i].put_assign();
                            break;
                        }

                    case type_of_lex.LEX_PLUS:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Push(obj_Sum(a,b));
                            break;
                        }

                    case type_of_lex.LEX_TIMES:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Push(obj_Mul(a, b));
                            break;
                        }

                    case type_of_lex.LEX_MINUS:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Push(obj_Min(b, a));
                            break;
                        }

                    case type_of_lex.LEX_SLASH:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            if ((float)a != 0)
                            {
                                args.Push(obj_Div(b,a));
                                break;
                            }
                            else
                            {
                                throw new Exception(" POLIZ: divide by zero");
                            }
                        }

                    case type_of_lex.LEX_EQ:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Eq(a, b))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_LSS:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Lss(b,a))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_GTR:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Gtr(b,a))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_LEQ:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Leq(b,a))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_GEQ:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Geq(b,a))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_NEQ:
                        {
                            object a = args.Pop();
                            args.Pop();
                            object b = args.Pop();
                            args.Pop();
                            if ((bool)obj_Neq(a,b))
                            {
                                args.Push(1);
                            }
                            else
                            {
                                args.Push(0);
                            }
                            break;
                        }

                    case type_of_lex.LEX_ASSIGN:
                        {
                            i = args.Pop();
                            args.Pop();
                            j = (int)args.Pop();
                            arr_index = (int)args.Pop();

                            if(arr_index > 0)
                                Globals.TID_List[pc_el.get_tid_index()][(int)j].put_value(i, arr_index);
                            else
                                Globals.TID_List[pc_el.get_tid_index()][(int)j].put_value(i);

                            Globals.TID_List[pc_el.get_tid_index()][(int)j].put_assign();
                            break;
                        }

                    default:
                        {
                            throw new Exception("POLIZ: unexpected elem");
                        }
                }
                ++index;
            };
            Console.WriteLine("Finish of executing!!!!");
        }

        object obj_Sum(object a, object b)
        {
            if (a.GetType() == typeof(int[]) && b.GetType() == typeof(int[]))
            {
                return ((int)a + (int)b);
            }
            if ((a.GetType() == typeof(float[]) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(float[]) ||b.GetType() == typeof(int[])))
            {
                return ((float)a + (float)b);
            }
            if (a.GetType() == typeof(string) && b.GetType() == typeof(string))
            {
                string t1 = a.ToString(), t2 = b.ToString();
                return (t2 + t1);
            }
            else if (a.GetType() == typeof(char) && b.GetType() == typeof(string)) 
            {
                return (string)((string)b + (char)a) as object;
            }
            else if (a.GetType() == typeof(int) && b.GetType() == typeof(char)) 
            {
                return (char)((int)a + (char)b) as object;
            }
            else if (a.GetType() == typeof(int) && b.GetType() == typeof(int))  
            {
                return ((int)a + (int)b);
            }
            else if (a.GetType() == typeof(char) && b.GetType() == typeof(char))
            {
                return (char)((char)a + (char)b);
            }
            else if ((a.GetType() == typeof(int) || a.GetType() == typeof(float)) && b.GetType() == typeof(float)) 
            {
                if (a.GetType() == typeof(int))
                    return ((int)a + (float)b);
                else
                    return ((float)(a) + (float)b);
            }
            else
                throw new Exception("not permited operation");

        }

        object obj_Mul(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a * (int)b);
            }
         else if (a.GetType() == typeof(float)  && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) || 
                a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                b.GetType() == typeof(float[]) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a * (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a * (int)b);
                else
                    return ((float)a * (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Min(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a - (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a - (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a - (int)b);
                else
                    return ((float)a - (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Div(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a / (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a / (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a / (int)b);
                else
                    return ((float)a / (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Lss(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a < (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a < (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a < (int)b);
                else
                    return ((float)a < (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Gtr(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a > (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a > (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a > (int)b);
                else
                    return ((float)a > (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Leq(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a <= (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a <= (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a <= (int)b);
                else
                    return ((float)a <= (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Geq(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a >= (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a >= (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a >= (int)b);
                else
                    return ((float)a >= (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Eq(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a == (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a == (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a == (int)b);
                else
                    return ((float)a == (float)b);
            }
            else
                throw new Exception("not permited operation");
        }

        object obj_Neq(object a, object b)
        {
            if ((a.GetType() == typeof(int) || a.GetType() == typeof(int[])) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[])))
            {
                return ((int)a != (int)b);
            }
            else if (a.GetType() == typeof(float) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   a.GetType() == typeof(float[]) && (b.GetType() == typeof(int) || b.GetType() == typeof(int[]) || b.GetType() == typeof(float) || b.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])) ||
                   b.GetType() == typeof(float) && (a.GetType() == typeof(int) || a.GetType() == typeof(int[]) || a.GetType() == typeof(float) || a.GetType() == typeof(float[])))
            {
                if (a.GetType() == typeof(int) || a.GetType() == typeof(int[]))
                    return ((int)a != (float)b);
                else if (b.GetType() == typeof(int) || b.GetType() == typeof(int[]))
                    return ((float)a != (int)b);
                else
                    return ((float)a != (float)b);
            }
            else
                throw new Exception("not permited operation");
        }
    }
}
