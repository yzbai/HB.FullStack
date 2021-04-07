using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
    public class EventCode : IEquatable<EventCode>
    {
        public static readonly EventCode Empty = new EventCode(0);

        public int Id { get; }

        public string? Name { get; }

        public string? Message { get; }


        public static implicit operator EventCode(int i)
        {
            return new EventCode(i);
        }

        public static bool operator ==(EventCode? left, EventCode? right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(EventCode? left, EventCode? right)
        {
            return !(left == right);
        }

        public EventCode(int id, string? name = null, string? message = null)
        {
            Id = id;
            Name = name;
            Message = message;
        }

        public override string ToString()
        {
            return Name ?? Id.ToString(CultureInfo.InvariantCulture);
        }

        public bool Equals(EventCode? other)
        {
            if(ReferenceEquals(other,null))
            {
                return false;
            }

            return Id == other.Id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is EventCode eventCode)
            {
                return Equals(eventCode);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static EventCode FromInt32(int code)
        {
            return new EventCode(code);
        }

        public Microsoft.Extensions.Logging.EventId ToEventId()
        {
            return new Microsoft.Extensions.Logging.EventId(Id, Name);
        }
    }
}