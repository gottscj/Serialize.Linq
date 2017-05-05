using System;

namespace Serialize.Linq.Exceptions
{
    public class MemberNotFoundException : Exception
    {
        public MemberNotFoundException(string message, Type declaringType, string memberSignature)
            : base(message)
        {
            DeclaringType = declaringType;
            MemberSignature = memberSignature;
        }

        public Type DeclaringType { get; private set; }

        public string MemberSignature { get; private set; }

        public override string ToString()
        {
            return string.Format("{1}.{0}Declaring Type: '{2}'{0}Member Signature: '{3}'",
                Environment.NewLine,
                Message, DeclaringType, MemberSignature);
        }
    }
}