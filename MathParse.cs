using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MathParse
{
    static class Parser
    {
        private enum ReadState_t { Start, Klammerausdruck, ZahlStart, ZahlVorKomma, ZahlNachKomma, Pending, Operator, SyntaxError }

        private static double Ziffernwert(char c) => c switch
        {
            '0' => 0.0,
            '1' => 1.0,
            '2' => 2.0,
            '3' => 3.0,
            '4' => 4.0,
            '5' => 5.0,
            '6' => 6.0,
            '7' => 7.0,
            '8' => 8.0,
            '9' => 9.0,
            _ => throw new ArgumentException()
        };

        private static int OperatorBindungskraft(char c) => c switch
        {
            '+' => 1,
            '-' => 1,
            '*' => 2,
            '/' => 2,
            '^' => 3,
            _ => throw new ArgumentException()
        };

        private static readonly char[] Zahlstartzeichen = "+-0123456789.".ToCharArray();
        private static readonly char Dezimaltrennzeichen = '.';
        private static readonly char PositivesVorzeichen = '+';
        private static readonly char NegativesVorzeichen = '-';
        private static readonly char[] Ziffern = "0123456789".ToCharArray();
        private static readonly char[] Operators = "+-*/^".ToCharArray();

        private static readonly char ÖffnendeKlammer = '(';
        private static readonly char SchließendeKlammer = ')';

        public static async Task<double> Evaluate(string expression, TextWriter output = null)
        {
            var ReadState = ReadState_t.Start;
            int OperatorPosition = -1;
            int minOperatorBindungskraft = int.MaxValue;
            bool isKlammerausdruck = false;
            int KlammerLevel = 0;
            double ZahlWert = 0.0;
            int ZahlVorzeichen = 0;
            double ZahlNachkommastelle = 1.0;
            
            for (int pos = 0; pos < expression.Length; pos++)
            {
                char c = expression[pos];

                switch (ReadState)
                {
                    case ReadState_t.Start:
                        if      (c == ÖffnendeKlammer)         { ReadState = ReadState_t.Klammerausdruck; goto case ReadState_t.Klammerausdruck; }
                        else if (Zahlstartzeichen.Contains(c)) { ReadState = ReadState_t.ZahlStart; goto case ReadState_t.ZahlStart; }
                        else if (!char.IsWhiteSpace(c))        { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.Klammerausdruck:
                        isKlammerausdruck = true;
                        if      (c == ÖffnendeKlammer)    { KlammerLevel++; }
                        else if (c == SchließendeKlammer) { if (--KlammerLevel == 0) ReadState = ReadState_t.Pending; }
                        break;

                    case ReadState_t.ZahlStart:
                        ZahlWert = 0.0;
                        ZahlVorzeichen = 1;
                        ZahlNachkommastelle = 1.0;
                        if      (c == PositivesVorzeichen) { ZahlVorzeichen = 1; ReadState = ReadState_t.ZahlVorKomma; }
                        else if (c == NegativesVorzeichen) { ZahlVorzeichen = -1; ReadState = ReadState_t.ZahlVorKomma; }
                        else if (c == Dezimaltrennzeichen) { ReadState = ReadState_t.ZahlNachKomma; }
                        else if (Ziffern.Contains(c))      { ReadState = ReadState_t.ZahlVorKomma; goto case ReadState_t.ZahlVorKomma; }
                        else                               { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.ZahlVorKomma:
                        if      (c == Dezimaltrennzeichen) { ReadState = ReadState_t.ZahlNachKomma; }
                        else if (Ziffern.Contains(c))      { ZahlWert = ZahlWert * 10.0 + Ziffernwert(c); }
                        else if (char.IsWhiteSpace(c))     { ReadState = ReadState_t.Pending; }
                        else if (Operators.Contains(c))    { ReadState = ReadState_t.Operator; goto case ReadState_t.Operator; }
                        else                               { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.ZahlNachKomma:
                        if      (Ziffern.Contains(c))   { ZahlNachkommastelle *= 10.0; ZahlWert += Ziffernwert(c) / ZahlNachkommastelle; }
                        else if (char.IsWhiteSpace(c))  { ReadState = ReadState_t.Pending; }
                        else if (Operators.Contains(c)) { ReadState = ReadState_t.Operator; goto case ReadState_t.Operator; }
                        else                            { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.Pending:
                        if      (Operators.Contains(c)) { ReadState = ReadState_t.Operator; goto case ReadState_t.Operator; }
                        else if (!char.IsWhiteSpace(c)) { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.Operator:
                        if (Operators.Contains(c))
                        {
                            var Kraft = OperatorBindungskraft(c);
                            if (Kraft <= minOperatorBindungskraft)
                            {
                                OperatorPosition = pos;
                                minOperatorBindungskraft = Kraft;
                            }
                            ReadState = ReadState_t.Start;
                        }
                        else { ReadState = ReadState_t.SyntaxError; goto case ReadState_t.SyntaxError; }
                        break;

                    case ReadState_t.SyntaxError:
                        throw new ArgumentException(expression, nameof(expression));
                }
            }

            if (ReadState == ReadState_t.Klammerausdruck)
                throw new ArgumentException(expression, nameof(expression));

            if (OperatorPosition != -1)
            {
                String lhs = expression.Substring(0, OperatorPosition);
                String rhs = expression.Substring(OperatorPosition + 1);
                char operation = expression[OperatorPosition];

                double lhsValue = await Evaluate(lhs, output);
                double rhsValue = await Evaluate(rhs, output);
                double result = operation switch
                {
                    '+' => lhsValue + rhsValue,
                    '-' => lhsValue - rhsValue,
                    '*' => lhsValue * rhsValue,
                    '/' => lhsValue / rhsValue,
                    '^' => Math.Pow(lhsValue, rhsValue),
                    _ => throw new ArgumentException(expression, nameof(expression))
                };

                if (output != null)
                    await output.WriteLineAsync($"[{lhsValue}] <{operation}> [{rhsValue}] = [{result}]");

                return result;
            }
            else if (ZahlVorzeichen != 0)
            {
                double result = ZahlVorzeichen * ZahlWert;
                return result;
            }
            else if (isKlammerausdruck)
            {
                expression = expression.Trim()[1..^1];
                return await Evaluate(expression, output);
            }
            else
            {
                throw new ArgumentException(expression, nameof(expression));
            }
        }
    }
}
