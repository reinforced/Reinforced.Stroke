using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Reinforced.Stroke
{
    internal class InterpolationParseringExtensions
    {
        public static string RevealQuery(DbContext context, LambdaExpression expr, bool fullQualified, out object[] parameters)
        {
            const string err = "SQL Storke must be in form of context.Stroke(x=>$\"SOME SQL WITH {x} AND {x.Field} USAGE\")";
            var bdy = expr.Body as MethodCallExpression;
            if (bdy == null) throw new Exception(err);
            if (bdy.Method.DeclaringType != typeof(String) && bdy.Method.Name != "Format")
            {
                throw new Exception(err);
            }

            var fmtExpr = bdy.Arguments[0] as ConstantExpression;
            if (fmtExpr == null) throw new Exception(err);
            var format = fmtExpr.Value.ToString();

            int startingIndex = 1;
            var arguments = bdy.Arguments;
            bool longFormat = false;
            if (bdy.Arguments.Count == 2)
            {
                var secondArg = bdy.Arguments[1];
                if (secondArg.NodeType == ExpressionType.NewArrayInit)
                {
                    var array = secondArg as NewArrayExpression;
                    arguments = array.Expressions;
                    startingIndex = 0;
                    longFormat = true;
                }
            }

            List<string> formatArgs = new List<string>();
            List<object> sqlParams = new List<object>();
            for (int i = startingIndex; i < arguments.Count; i++)
            {
                var cArg = Unconvert(arguments[i]);
                if (!IsScopedParameterAccess(cArg))
                {
                    var lex = Expression.Lambda(cArg);
                    var compiled = lex.Compile();
                    var result = compiled.DynamicInvoke();
                    formatArgs.Add(string.Format("{{{0}}}", sqlParams.Count));
                    sqlParams.Add(result);
                    continue;
                }
                if (cArg.NodeType == ExpressionType.Parameter)
                {
                    if (fullQualified)
                    {
                        var par = cArg as ParameterExpression;
                        var argIdx = longFormat ? i : i - 1;
                        if (NeedsDec(format, argIdx))
                        {
                            formatArgs.Add(string.Format("[{0}] [{1}]", context.GetTableName(cArg.Type), par.Name));
                        }
                        else
                        {
                            formatArgs.Add(par.Name);
                        }
                    }
                    else formatArgs.Add(string.Format("[{0}]", context.GetTableName(cArg.Type)));

                    continue;
                }
                var argProp = cArg as MemberExpression;

                if (argProp.Expression.NodeType != ExpressionType.Parameter)
                {
                    var root = GetRootMember(argProp);
                    throw new Exception(string.Format("Please refer only top-level properties of {0}", root.Type));
                }


                var colId = string.Format("[{0}]", context.GetColumnName(argProp.Member.DeclaringType, argProp.Member.Name));
                if (fullQualified)
                {
                    var root = GetRootMember(argProp);
                    var parRef = root as ParameterExpression;
                    colId = string.Format("[{0}].{1}", parRef, colId);
                }
                formatArgs.Add(colId);
            }
            var sqlString = string.Format(format, formatArgs.ToArray());
            parameters = sqlParams.ToArray();
            return sqlString;
        }

        private static bool NeedsDec(string format, int argNumber)
        {
            var searchString = "{" + argNumber + "}";
            var idx = format.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) - 1;
            if (idx <= 0) return false;
            while (char.IsWhiteSpace(format, idx)) idx--;
            if (idx - 4 < 0) return false;
            var s = format.Substring(idx - 3, 4);
            return string.Compare(s, "FROM", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                   string.Compare(s, "JOIN", StringComparison.InvariantCultureIgnoreCase) == 0;
        }


        private static Expression Unconvert(Expression ex)
        {
            if (ex.NodeType == ExpressionType.Convert)
            {
                var cex = ex as UnaryExpression;
                ex = cex.Operand;
            }
            return ex;
        }

        private static Expression GetRootMember(MemberExpression expr)
        {
            var accessee = expr.Expression as MemberExpression;
            var current = expr.Expression;
            while (accessee != null)
            {
                accessee = accessee.Expression as MemberExpression;
                if (accessee != null) current = accessee.Expression;
            }
            return current;
        }

        private static bool IsScopedParameterAccess(Expression expr)
        {
            if (expr.NodeType == ExpressionType.Parameter) return true;
            var ex = expr as MemberExpression;
            if (ex == null) return false;

            var root = GetRootMember(ex);
            if (root == null) return false;
            if (root.NodeType != ExpressionType.Parameter) return false;
            return true;
        }
    }
}
