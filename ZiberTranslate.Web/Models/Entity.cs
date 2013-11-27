using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ZiberTranslate.Web.Models
{
    public abstract class Entity
    {
        private int? _oldHashCode;

        public virtual int Id { get; set; }

        public override bool Equals(object obj)
        {
            Entity other = obj as Entity;
            if (other == null) return false;
            if (Id == 0 && other.Id == 0)
                return (object)this == other;
            else
                return Id == other.Id;
        }

        public override int GetHashCode()
        {
            // Once we have a hash code we'll never change it
            if (_oldHashCode.HasValue)
                return _oldHashCode.Value;

            bool thisIsTransient = Id == 0;
            
            // When this instance is transient, we use the base GetHashCode()
            // and remember it, so an instance can NEVER change its hash code.

            if (thisIsTransient)
            {
                _oldHashCode = base.GetHashCode();

                return _oldHashCode.Value;
            }
            return Id.GetHashCode();
        }
    }
}