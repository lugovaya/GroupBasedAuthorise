using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GroupBasedAuthorise.DAL
{
    [Serializable]
    class GroupExistsException : Exception
    {
        public GroupExistsException()
        {
        }

        public GroupExistsException(string message)
            : base(message)
        {
        }

        public GroupExistsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected GroupExistsException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
