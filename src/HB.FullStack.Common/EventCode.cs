using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
    public readonly struct EventCode : IEquatable<EventCode>
    {
        public static readonly EventCode Empty = new EventCode(0);

        public int Id { get; }

        public string? Name { get; }

        public string? Message { get; }


        public static implicit operator EventCode(int i)
        {
            return new EventCode(i);
        }

        public static bool operator ==(EventCode left, EventCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventCode left, EventCode right)
        {
            return !left.Equals(right);
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

        public bool Equals(EventCode other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is EventCode)
            {
                EventCode other = (EventCode)obj;
                return Equals(other);
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