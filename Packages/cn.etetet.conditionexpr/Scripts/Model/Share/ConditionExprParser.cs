using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    [EnableClass]
    public class ConditionExprParser
    {
        private readonly List<ConditionToken> tokens;
        private readonly int defaultErrorCode;
        private int position;
        private int nextNodeId = 1;

        public ConditionExprParser(List<ConditionToken> tokens, int defaultErrorCode)
        {
            this.tokens = tokens;
            this.defaultErrorCode = defaultErrorCode;
        }

        public ConditionRoot Parse()
        {
            ConditionRoot root = this.CreateNode<ConditionRoot>();
            if (this.Current.Type == ConditionTokenType.End)
            {
                return root;
            }

            root.Children.Add(this.ParseExpression());
            this.Expect(ConditionTokenType.End);
            return root;
        }

        private BTNode ParseExpression()
        {
            return this.ParseOr();
        }

        private BTNode ParseOr()
        {
            BTNode left = this.ParseAnd();
            while (this.Match(ConditionTokenType.Or))
            {
                BTNode right = this.ParseAnd();
                BTSelector selector = this.CreateNode<BTSelector>();
                this.AddSelectorChild(selector, left);
                this.AddSelectorChild(selector, right);
                left = selector;
            }

            return left;
        }

        private BTNode ParseAnd()
        {
            BTNode left = this.ParseUnary();
            while (this.Match(ConditionTokenType.And))
            {
                BTNode right = this.ParseUnary();
                BTSequence sequence = this.CreateNode<BTSequence>();
                this.AddSequenceChild(sequence, left);
                this.AddSequenceChild(sequence, right);
                left = sequence;
            }

            return left;
        }

        private BTNode ParseUnary()
        {
            if (!this.Match(ConditionTokenType.Not))
            {
                return this.ParsePrimary();
            }

            BTNot not = this.CreateNode<BTNot>();
            not.ErrorCode = this.defaultErrorCode;
            not.Children.Add(this.ParseUnary());
            if (this.Match(ConditionTokenType.Colon))
            {
                not.ErrorCode = this.ReadErrorCode();
            }

            return not;
        }

        private BTNode ParsePrimary()
        {
            if (this.Match(ConditionTokenType.LeftParen))
            {
                BTNode node = this.ParseExpression();
                this.Expect(ConditionTokenType.RightParen);
                if (this.Match(ConditionTokenType.Colon))
                {
                    this.ApplyErrorCode(node, this.ReadErrorCode());
                }
                return node;
            }

            return this.ParseCompare();
        }

        private BTNode ParseCompare()
        {
            ConditionToken variableToken = this.Expect(ConditionTokenType.Identifier);
            ConditionCompareOp op = this.ReadCompareOp();
            ConditionToken valueToken = this.Expect(ConditionTokenType.Number);
            int errorCode = this.defaultErrorCode;
            if (this.Match(ConditionTokenType.Colon))
            {
                errorCode = this.ReadErrorCode();
            }

            ConditionVariableRegistry registry = ConditionVariableRegistry.Instance;
            if (registry == null)
            {
                throw new Exception("condition variable registry is not initialized");
            }

            if (!registry.TryGetNodeType(variableToken.Text, out Type nodeType))
            {
                throw new Exception($"condition variable not registered: {variableToken.Text}");
            }

            BTCondition node = Activator.CreateInstance(nodeType) as BTCondition;
            if (node == null)
            {
                throw new Exception($"condition variable node create error: {nodeType.FullName}");
            }

            node.Id = this.NextNodeId();
            if (node is BTNumericCompare numericCompare)
            {
                if (registry.TryGetNumericType(variableToken.Text, out int numericType))
                {
                    numericCompare.NumericType = numericType;
                }

                numericCompare.Op = op;
                numericCompare.Value = valueToken.Number;
                numericCompare.ErrorCode = errorCode;
                return numericCompare;
            }

            this.SetCompareField(node, nameof(BTNumericCompare.Op), op);
            this.SetCompareField(node, nameof(BTNumericCompare.Value), valueToken.Number);
            this.SetCompareField(node, nameof(BTNumericCompare.ErrorCode), errorCode);
            return node;
        }

        private ConditionCompareOp ReadCompareOp()
        {
            ConditionToken token = this.Current;
            ++this.position;
            return token.Type switch
            {
                ConditionTokenType.Greater => ConditionCompareOp.Greater,
                ConditionTokenType.GreaterEqual => ConditionCompareOp.GreaterEqual,
                ConditionTokenType.Less => ConditionCompareOp.Less,
                ConditionTokenType.LessEqual => ConditionCompareOp.LessEqual,
                ConditionTokenType.Equal => ConditionCompareOp.Equal,
                ConditionTokenType.NotEqual => ConditionCompareOp.NotEqual,
                _ => throw new Exception($"condition compare op expected, actual: {token.Text}")
            };
        }

        private int ReadErrorCode()
        {
            ConditionToken token = this.Expect(ConditionTokenType.Number);
            if (token.Number < int.MinValue || token.Number > int.MaxValue)
            {
                throw new Exception($"condition error code out of range: {token.Number}");
            }

            return (int)token.Number;
        }

        private void ApplyErrorCode(BTNode node, int errorCode)
        {
            if (node is BTNumericCompare numericCompare)
            {
                numericCompare.ErrorCode = errorCode;
                return;
            }

            if (node is BTCondition condition)
            {
                this.SetCompareField(condition, nameof(BTNumericCompare.ErrorCode), errorCode);
                return;
            }

            if (node is BTNot not)
            {
                not.ErrorCode = errorCode;
                return;
            }

            throw new Exception($"condition group error code only support leaf or not node: {node.GetType().Name}");
        }

        private void SetCompareField(BTCondition node, string fieldName, object value)
        {
            FieldInfo fieldInfo = node.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null)
            {
                throw new Exception($"condition node field not found: {node.GetType().FullName}.{fieldName}");
            }

            fieldInfo.SetValue(node, value);
        }

        private void AddSequenceChild(BTSequence sequence, BTNode child)
        {
            if (child is BTSequence childSequence)
            {
                sequence.Children.AddRange(childSequence.Children);
                return;
            }

            sequence.Children.Add(child);
        }

        private void AddSelectorChild(BTSelector selector, BTNode child)
        {
            if (child is BTSelector childSelector)
            {
                selector.Children.AddRange(childSelector.Children);
                return;
            }

            selector.Children.Add(child);
        }

        private T CreateNode<T>() where T : BTNode, new()
        {
            return new T { Id = this.NextNodeId() };
        }

        private int NextNodeId()
        {
            return this.nextNodeId++;
        }

        private bool Match(ConditionTokenType type)
        {
            if (this.Current.Type != type)
            {
                return false;
            }

            ++this.position;
            return true;
        }

        private ConditionToken Expect(ConditionTokenType type)
        {
            ConditionToken token = this.Current;
            if (token.Type != type)
            {
                throw new Exception($"condition token expected: {type}, actual: {token.Text}");
            }

            ++this.position;
            return token;
        }

        private ConditionToken Current => this.tokens[this.position];
    }
}
