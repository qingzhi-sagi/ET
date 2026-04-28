using System.Collections.Generic;

namespace ET
{
    public static class ConditionExprCompiler
    {
        public static ConditionRoot Compile(string expr, int defaultErrorCode = 1)
        {
            ConditionExprLexer lexer = new(expr);
            List<ConditionToken> tokens = lexer.Tokenize();
            ConditionExprParser parser = new(tokens, defaultErrorCode);
            return parser.Parse();
        }
    }
}
