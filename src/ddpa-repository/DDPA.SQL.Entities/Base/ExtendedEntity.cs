using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DDPA.SQL.Entities
{
    public abstract class ExtendedEntity<T> : IExtendedEntity<T>
    {
        private DateTime? _createdDate;

        public T Id { get; set; }

        //object IEntity.Id
        //{
        //    set { }
        //    get { return this.Id; }
        //}

        public DateTime CreatedDate
        {
            get { return _createdDate ?? DateTime.UtcNow; }
            set { _createdDate = value; }
        }

        public DateTime? ModifiedDate { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedBy { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
