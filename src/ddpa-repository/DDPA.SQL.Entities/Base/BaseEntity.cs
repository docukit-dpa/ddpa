using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DDPA.SQL.Entities
{
    public abstract class BaseEntity<T> : IEntity<T>
    {
        public T Id { get; set; }

        //object IEntity.Id
        //{
        //    set { }
        //    get { return this.Id; }
        //}

        public byte[] RowVersion { get; set; }
    }
}
