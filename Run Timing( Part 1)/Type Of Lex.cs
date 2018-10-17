
namespace Interpritator
{
    //Types of each Lexem in programming language
    enum type_of_lex
    {
        LEX_NULL, 
        LEX_PROGRAM, LEX_BEGIN, LEX_END, LEX_FIN, LEX_PROC,
        LEX_WRITE, LEX_READ,

        LEX_BOOL, LEX_TRUE, LEX_FALSE,

        LEX_INT, LEX_INT_ARR, LEX_NUM,
        LEX_CHAR, LEX_STRING, LEX_STRING_ARR, LEX_FLOAT, LEX_FLOAT_ARR,
        LEX_ID, LEX_INDEX,

        LEX_PLUS, LEX_MINUS, LEX_TIMES, LEX_SLASH,
        LEX_AND, LEX_OR, LEX_NOT, LEX_EQ, LEX_LSS,LEX_GTR, LEX_LEQ, LEX_NEQ, LEX_GEQ,

        LEX_IF, LEX_ELSE,
        LEX_WHILE,

        LEX_SEMICOLON, LEX_COMMA, LEX_COLON, LEX_ASSIGN, LEX_LPAREN, LEX_RPAREN, LEX_APOSTROPHE, LEX_QUOTE, LEX_DOT,

        POLIZ_START,
        POLIZ_LABEL, 
        POLIZ_ADDRESS, 
        POLIZ_GO,
        POLIZ_GO_PROC,
        POLIZ_FGO,
        POLIZ_TABLE
    }

}
