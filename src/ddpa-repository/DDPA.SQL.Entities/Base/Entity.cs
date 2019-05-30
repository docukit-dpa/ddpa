using System;
using System.Collections.Generic;
using System.Text;

namespace DDPA.SQL.Entities
{
    public abstract class Entity<T> : IEntity<T>
    {
        public T Id { get; set; }
    }
}
